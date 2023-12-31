using System;
using System.Linq;
using UnityEngine;
using PlayShifu.Terra;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Terra.Studio
{
    public class SceneDataHandler : IDisposable
    {
        public Func<string> GetAssetName;
        public event Action OnSceneSetupDone;
        public Func<GameObject, string> TryGetAssetPath;
        public Vector3 PlayerSpawnPoint { get; private set; }

        private Camera editorCamera;
        private SceneState sceneState;

        public SceneDataHandler()
        {
            if (!Helper.IsInUnityEditorMode() && EditorOp.Resolve<EditorEssentialsLoader>() == null)
            {
                EditorOp.Register(new EditorEssentialsLoader());
            }
        }

        public void Dispose()
        {
            if (!Helper.IsInUnityEditorMode() && IsEditorState())
            {
                EditorOp.Unregister<EditorEssentialsLoader>();
            }
        }

        public bool IsEditorState()
        {
            return SystemOp.Resolve<System>().CurrentStudioState == StudioState.Editor;
        }

        public void Save(Action onSaveDone = null)
        {
            var sceneData = ExportSceneData();
            if (!Helper.IsInUnityEditorMode())
            {
                EditorOp.Resolve<ToolbarView>().SetSaveMessage(false, SaveState.Saving);
                SystemOp
                    .Resolve<SaveSystem>()
                    .SaveManualData(
                        sceneData,
                        false,
                        (status) =>
                        {
                            if (SystemOp.Resolve<System>().ConfigSO.ServeFromCloud && status)
                            {
                                SystemOp
                                    .Resolve<User>()
                                    .UploadSaveDataToCloud(
                                        sceneData,
                                        (status, _) =>
                                        {
                                            OnCloudSaveAttempted(status);
                                            onSaveDone?.Invoke();
                                        }
                                    );
                            }
                            else
                            {
                                EditorOp
                                    .Resolve<ToolbarView>()
                                    .SetSaveMessage(true, SaveState.Empty);
                                OnCloudSaveAttempted(false);
                                onSaveDone?.Invoke();
                            }
                        }
                    );
            }
            else
            {
                new FileService().WriteFile(sceneData, GetAssetName?.Invoke(), false, null);
            }
        }

        private void OnCloudSaveAttempted(bool status)
        {
            var saveState = status ? SaveState.SavedToCloud : SaveState.ChangesSavedOffline;
            EditorOp.Resolve<ToolbarView>().SetSaveMessage(false, saveState);
        }

        public void PrepareSceneDataToRuntime(Action onPreparationDone)
        {
            var sceneData = ExportSceneData();
            SystemOp.Resolve<CrossSceneDataHolder>().Set(sceneData);
            SystemOp
                .Resolve<SaveSystem>()
                .SaveManualData(
                    sceneData,
                    false,
                    (status) =>
                    {
                        if (status)
                        {
                            if (SystemOp.Resolve<System>().ConfigSO.ServeFromCloud)
                            {
                                SystemOp.Resolve<User>().UploadSaveDataToCloud(sceneData, null);
                            }
                            onPreparationDone?.Invoke();
                        }
                    }
                );
        }

        #region Load Scene Data

        public void LoadScene()
        {
            sceneState = SceneState.Importing;
            var prevState = SystemOp.Resolve<System>().PreviousStudioState;
            if (prevState != StudioState.Runtime)
            {
                if (SystemOp.Resolve<System>().ConfigSO.PickupSavedData)
                {
                    SystemOp.Resolve<SaveSystem>().GetManualSavedData(OnDataReceived);
                }
                else
                {
                    OnDataReceived(false, null);
                }
            }
            else
            {
                var data = SystemOp.Resolve<CrossSceneDataHolder>().Get();
                var isDataAvailable = !string.IsNullOrEmpty(data);
                OnDataReceived(isDataAvailable, data);
            }
        }

        private void OnDataReceived(bool status, string data)
        {
            if (status)
            {
                RecreateScene(data);
            }
            SetupSceneDefaultObjects();
            EditorOp.Resolve<EditorEssentialsLoader>().LoadEssentials();
            OnSceneSetupDone?.Invoke();
        }

#if UNITY_EDITOR
        public
#endif
        void RecreateScene(string data)
        {
            var worldData = JsonConvert.DeserializeObject<WorldData>(data);
            if (!Helper.IsInUnityEditorMode())
            {
                var metaData = worldData.metaData;
                if (!metaData.Equals(default(WorldMetaData)))
                {
                    PlayerSpawnPoint = metaData.playerSpawnPoint;
                }
                SystemOp.Resolve<RequestValidator>().Prewarm(ref worldData, () =>
                {
                    for (int i = 0; i < worldData.entities.Length; i++)
                    {
                        var entity = worldData.entities[i];
                        SpawnObjects(entity);
                    }
#if UNITY_WEBGL && !UNITY_EDITOR
            WebGLWrapper.HideLoadingScreen();
#endif
                    sceneState = SceneState.Stale;
                });
            }
            else
            {
                for (int i = 0; i < worldData.entities.Length; i++)
                {
                    var entity = worldData.entities[i];
                    SpawnObjects(entity);
                    sceneState = SceneState.Stale;
                }
            }
        }

        private void SpawnObjects(VirtualEntity entity)
        {
            GameObject generatedObj = default;
            var trs = new Vector3[] { entity.position, entity.rotation, entity.scale };
            RuntimeWrappers.SpawnObject(
                entity.assetType,
                entity.assetPath,
                entity.primitiveType,
                bla,
                entity.uniqueName,
                trs
            );

            void bla(GameObject x)
            {
                generatedObj = x;
                generatedObj.name = entity.name;
                SetColliderData(generatedObj, entity.metaData);
                AttachComponents(generatedObj, entity.components);
                HandleChildren(generatedObj, entity.children);
            }
        }

        public void AttachComponents(
            GameObject gameObject,
            EntityBasedComponent[] components,
            Action<GameObject, EntityBasedComponent[]> onComponentDependencyFound = null
        )
        {
            for (int i = 0; i < components.Length; i++)
            {
                if (Helper.IsInUnityEditorMode())
                {
                    var metaData = gameObject.AddComponent<EditorMetaData>();
                    metaData.componentData = components[i];
                }
                else
                {
                    if (EditorOp.Resolve<DataProvider>() != null)
                    {
                        var type = EditorOp
                            .Resolve<DataProvider>()
                            .GetVariance(components[i].type);
                        if (type == null || string.IsNullOrEmpty(components[i].data))
                        {
                            continue;
                        }
                        var component = gameObject.AddComponent(type) as IComponent;
                        component?.Import(components[i]);
                    }
                }
            }
            if (components != null && components.Length != 0)
            {
                onComponentDependencyFound?.Invoke(gameObject, components);
            }
        }

        public void HandleChildren(
            GameObject gameObject,
            VirtualEntity[] children,
            Action<GameObject, EntityBasedComponent[]> onComponentDependencyFound = null
        )
        {
            if (children == null || children.Length == 0)
            {
                return;
            }
            var tr = gameObject.transform;
            for (int i = 0; i < children.Length; i++)
            {
                var childEntity = children[i];
                Transform child;
                if (tr.childCount < i + 1)
                {
                    GameObject generatedObj = default;
                    var trs = new Vector3[]
                    {
                        childEntity.position,
                        childEntity.rotation,
                        childEntity.scale
                    };
                    RuntimeWrappers.SpawnObject(
                        childEntity.assetType,
                        childEntity.assetPath,
                        childEntity.primitiveType,
                        bla,
                        childEntity.uniqueName,
                        trs
                    );


                    void bla(GameObject gb)
                    {
                        generatedObj = gb;
                        child = generatedObj.transform;
                        child.SetParent(tr);
                        child.localScale = childEntity.scale.WorldToLocalScale(tr);
                        child.name = childEntity.name;
                        SetColliderData(child.gameObject, childEntity.metaData);
                        AttachComponents(child.gameObject, childEntity.components, onComponentDependencyFound);
                        HandleChildren(child.gameObject, childEntity.children, onComponentDependencyFound);
                    }
                }
                else
                {
                    child = gameObject.transform.GetChild(i);
                    child.SetPositionAndRotation(
                        childEntity.position,
                        Quaternion.Euler(childEntity.rotation)
                    );
                    child.localScale = childEntity.scale.WorldToLocalScale(child.parent);
                    child.name = childEntity.name;
                    SetColliderData(child.gameObject, childEntity.metaData);
                    AttachComponents(child.gameObject, childEntity.components, onComponentDependencyFound);
                    HandleChildren(child.gameObject, childEntity.children, onComponentDependencyFound);
                }
            }
        }

        #endregion

        #region Export Scene Data

        private string ExportSceneData()
        {
            sceneState = SceneState.Exporting;
            var worldMetaData = new WorldMetaData();
            var allGos = SceneManager.GetActiveScene().GetRootGameObjects();
            var virtualEntities = new List<VirtualEntity>();
            for (int i = 0; i < allGos.Length; i++)
            {
                if (!allGos[i].activeSelf || allGos[i].TryGetComponent(out HideInHierarchy _))
                {
                    continue;
                }
                if (allGos[i].TryGetComponent(out IgnoreToPackObject _))
                {
                    TryHandleUnpackableGameObjectData(
                        allGos[i],
                        virtualEntities,
                        ref worldMetaData
                    );
                    continue;
                }
                var entity = GetVirtualEntity(allGos[i], i, true);
                virtualEntities.Add(entity);
            }
            var worldData = new WorldData()
            {
                entities = virtualEntities.ToArray(),
                metaData = worldMetaData
            };
            var json = JsonConvert.SerializeObject(worldData);
            Debug.Log($"Generated json: {json}");
            sceneState = SceneState.Stale;
            return json;
        }

        public VirtualEntity GetVirtualEntity(GameObject go, int index, bool shouldCheckForAssetPath)
        {
            var newEntity = new VirtualEntity
            {
                id = index,
                name = go.name,
                assetPath = shouldCheckForAssetPath ? GetAssetPath(go) : null,
                position = go.transform.position,
                rotation = go.transform.eulerAngles,
                scale =
                    go.transform.parent != null
                        ? go.transform.localScale.LocalToWorldScale(go.transform.parent)
                        : go.transform.localScale,
                assetType = GetAssetType(go),
                shouldLoadAssetAtRuntime = true
            };
            if (newEntity.assetType == AssetType.Primitive)
            {
                newEntity.primitiveType = GetPrimitiveType(go);
            }

            if (go.TryGetComponent<StudioGameObject>(out var x))
            {
                newEntity.uniqueName = x.itemData.Name;
            }
            EntityBasedComponent[] entityComponents;
            var isInstantiable = false;
            if (Helper.IsInUnityEditorMode())
            {
                var metaComponents = go.GetComponents<EditorMetaData>();
                entityComponents = new EntityBasedComponent[metaComponents.Length];
                for (int j = 0; j < metaComponents.Length; j++)
                {
                    entityComponents[j] = metaComponents[j].componentData;
                }
            }
            else
            {
                var attachedComponents = go.GetComponents<IComponent>();
                var instantiableTypeName = nameof(InstantiateStudioObject);
                isInstantiable = attachedComponents.Any(x => x.ComponentName.Equals(instantiableTypeName));
                var isSubsystemEnabled = SystemOp.Resolve<System>().CanInitiateSubsystemProcess?.Invoke() ?? true;
                if (isInstantiable && isSubsystemEnabled)
                {
                    entityComponents = attachedComponents.
                        Where(x => x.ComponentName.Equals(instantiableTypeName)).
                        Select(y =>
                        {
                            var (type, data) = y.Export();
                            return new EntityBasedComponent()
                            {
                                type = type,
                                data = data
                            };
                        }).
                        ToArray();
                }
                else
                {
                    entityComponents = new EntityBasedComponent[attachedComponents.Length];
                    for (int j = 0; j < attachedComponents.Length; j++)
                    {
                        var (type, data) = attachedComponents[j].Export();
                        entityComponents[j] = new EntityBasedComponent() { type = type, data = data };
                    }
                }
            }
            newEntity.components = entityComponents;
            GetColliderData(go, ref newEntity.metaData);
            if (!isInstantiable)
            {
                var childrenEntities = new VirtualEntity[go.transform.childCount];
                for (int k = 0; k < go.transform.childCount; k++)
                {
                    childrenEntities[k] = GetVirtualEntity(go.transform.GetChild(k).gameObject, k, true);
                }
                newEntity.children = childrenEntities;
            }
            return newEntity;
        }

        private string GetAssetPath(GameObject go)
        {
            if (!Helper.IsInUnityEditorMode())
            {
                if (
                    go.TryGetComponent(out StudioGameObject component) && component.itemData != null
                )
                {
                    return component.itemData.ResourcePath;
                }
                return null;
            }
            else
            {
                var res = TryGetAssetPath?.Invoke(go);
                return res;
            }
        }

        private AssetType GetAssetType(GameObject go)
        {
            if (!Helper.IsInUnityEditorMode())
            {
                if (go.TryGetComponent(out StudioGameObject component))
                {
                    if (component.assetType == AssetType.RemotePrefab)
                    {
                        return AssetType.RemotePrefab;
                    }
                    if (
                        component.itemData != null
                        && !string.IsNullOrEmpty(component.itemData.ResourcePath)
                    )
                    {
                        return AssetType.Prefab;
                    }
                    if (go.IsPrimitive(out var _))
                    {
                        return AssetType.Primitive;
                    }
                }
                return AssetType.Empty;
            }
            else
            {
                var res = TryGetAssetPath?.Invoke(go);
                if (!string.IsNullOrEmpty(res))
                {
                    return AssetType.Prefab;
                }
                if (go.IsPrimitive(out _))
                {
                    return AssetType.Primitive;
                }
                return AssetType.Empty;
            }
        }

        private PrimitiveType GetPrimitiveType(GameObject go)
        {
            if (!Helper.IsInUnityEditorMode())
            {
                if (
                    go.TryGetComponent(out StudioGameObject component) && component.itemData != null
                )
                {
                    if (component.itemData.IsPrimitive && go.IsPrimitive(out var type))
                    {
                        return type;
                    }
                }
                return default;
            }
            else
            {
                var isPrimitive = go.IsPrimitive(out PrimitiveType type);
                if (isPrimitive)
                {
                    return type;
                }
                else
                {
                    return default;
                }
            }
        }

        private void GetColliderData(GameObject go, ref EntityMetaData metaData)
        {
            if (go.TryGetComponent(out Collider collider))
            {
                metaData.colliderData = new ColliderData()
                {
                    doesHaveCollider = true,
                    isTrigger = collider.isTrigger,
                    type = collider.GetType().AssemblyQualifiedName
                };
                switch (collider.GetType().Name)
                {
                    case nameof(BoxCollider):
                        var bx = (BoxCollider)collider;
                        metaData.colliderData.size = bx.size;
                        metaData.colliderData.center = bx.center;
                        break;
                    case nameof(SphereCollider):
                        var sc = (SphereCollider)collider;
                        metaData.colliderData.center = sc.center;
                        metaData.colliderData.radius = sc.radius;
                        break;
                    case nameof(CapsuleCollider):
                        var cc = (CapsuleCollider)collider;
                        metaData.colliderData.center = cc.center;
                        metaData.colliderData.radius = cc.radius;
                        metaData.colliderData.height = cc.height;
                        break;
                }
            }
        }

        public void SetColliderData(GameObject go, EntityMetaData metaData)
        {
            var colliderData = metaData.colliderData;
            var doesColliderExist = colliderData.doesHaveCollider;
            if (!doesColliderExist)
            {
                return;
            }
            var type = Type.GetType(colliderData.type);
            if (!go.TryGetComponent(out Collider collider))
            {
                collider = (Collider)go.AddComponent(type);
            }
            switch (type.Name)
            {
                case nameof(BoxCollider):
                    var bx = (BoxCollider)collider;
                    bx.size = colliderData.size;
                    bx.center = colliderData.center;
                    break;
                case nameof(SphereCollider):
                    var sc = (SphereCollider)collider;
                    sc.center = colliderData.center;
                    sc.radius = colliderData.radius;
                    break;
                case nameof(CapsuleCollider):
                    var cc = (CapsuleCollider)collider;
                    cc.center = colliderData.center;
                    cc.radius = colliderData.radius;
                    cc.height = colliderData.height;
                    break;
            }
            if (colliderData.isTrigger)
            {
                if (type.Equals(typeof(MeshCollider)))
                {
                    ((MeshCollider)collider).convex = true;
                }
                collider.isTrigger = true;
            }
        }

        private void TryHandleUnpackableGameObjectData(
            GameObject go,
            List<VirtualEntity> virtualEntities,
            ref WorldMetaData metaData
        )
        {
            if (go.TryGetComponent(out StudioGameObject studioGameObject))
            {
                if (studioGameObject.type == EditorObjectType.SpawnPoint)
                {
                    metaData.playerSpawnPoint = go.transform.position;
                }
                if (studioGameObject.type == EditorObjectType.Score)
                {
                    var entity = GetVirtualEntity(go, -1000, true);
                    entity.shouldLoadAssetAtRuntime = false;
                    virtualEntities.Add(entity);
                }
            }
        }

        #endregion

        #region Miscellaneous

        public GameObject ScoreManagerObj;
        private GameObject playerHealthObj;
        private int playerHealthObjReqs;
        private List<string> modifiers = new();

        private void SetupSceneDefaultObjects()
        {
            editorCamera = Camera.main;
            var isDataPresent = SystemOp
                .Resolve<CrossSceneDataHolder>()
                .Get("CameraPos", out var data);
            if (isDataPresent)
            {
                editorCamera.transform.position = (Vector3)data;
            }
            isDataPresent = SystemOp.Resolve<CrossSceneDataHolder>().Get("CameraRot", out data);
            if (isDataPresent)
            {
                editorCamera.transform.rotation = Quaternion.Euler((Vector3)data);
            }
        }

        public void SaveQoFDetails()
        {
            if (editorCamera)
            {
                SystemOp
                    .Resolve<CrossSceneDataHolder>()
                    .Set("CameraPos", editorCamera.transform.position);
                SystemOp
                    .Resolve<CrossSceneDataHolder>()
                    .Set("CameraRot", editorCamera.transform.eulerAngles);
            }
        }

        public void UpdateScoreModifiersCount(bool add, string id, bool setupScoreManager = true)
        {
            if (add)
            {
                if (modifiers.Contains(id))
                {
                    return;
                }
                modifiers.Add(id);
                if (setupScoreManager)
                {
                    SetupScoreManager(true);
                }
            }
            else
            {
                if (!modifiers.Contains(id))
                {
                    return;
                }
                modifiers.Remove(id);
            }
            if (modifiers.Count == 0 && setupScoreManager)
            {
                SetupScoreManager(false);
            }
        }

        private void SetupScoreManager(bool create)
        {
            if (create)
            {
                if (ScoreManagerObj || (ScoreManagerObj && ScoreManagerObj.activeSelf))
                {
                    return;
                }
                EditorOp
                    .Resolve<EditorEssentialsLoader>()
                    .Load(EditorObjectType.Score, out GameObject scoreManagerObj);
                scoreManagerObj.transform.SetAsFirstSibling();
            }
            else if (ScoreManagerObj)
            {
                Object.Destroy(ScoreManagerObj);
            }
        }

        public void SetupTimerManager()
        {
            if (EditorOp.Resolve<InGameTimer>()) return;
            EditorOp.
                Resolve<EditorEssentialsLoader>().
                Load(EditorObjectType.Timer, out var timerObj);
            timerObj.transform.SetAsFirstSibling();
        }

        public void SetupPlayerHealthManager(bool add)
        {
            if (add)
            {
                playerHealthObjReqs++;
                if (sceneState != SceneState.Stale) return;
                if (EditorOp.Resolve<PlayerHealth>()) return;
                playerHealthObj = new GameObject("PlayerHealth");
                playerHealthObj.AddComponent<PlayerHealth>();
            }
            else
            {
                playerHealthObjReqs--;
                if (playerHealthObjReqs != 0) return;
                if (!EditorOp.Resolve<PlayerHealth>()) return;
                Object.Destroy(playerHealthObj);
            }
        }

        #endregion

        private enum SceneState
        {
            Stale,
            Importing,
            Exporting
        }
    }
}
