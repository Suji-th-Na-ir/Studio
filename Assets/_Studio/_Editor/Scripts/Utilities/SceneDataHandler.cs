using System;
using UnityEngine;
using PlayShifu.Terra;
using Newtonsoft.Json;
using RuntimeInspectorNamespace;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Terra.Studio
{
    public class SceneDataHandler : IDisposable
    {
        public Func<GameObject, string> TryGetAssetPath;
        public Func<string> GetAssetName;
        private Camera editorCamera;
        public Vector3 PlayerSpawnPoint { get; private set; }

        public SceneDataHandler()
        {
            if (!Helper.IsInUnityEditorMode())
            {
                EditorOp.Register(new EditorEssentialsLoader());
            }
        }

        public void Dispose()
        {
            if (!Helper.IsInUnityEditorMode())
            {
                EditorOp.Unregister<EditorEssentialsLoader>();
            }
        }

        public void Save()
        {
            var sceneData = ExportSceneData();
            if (!Helper.IsInUnityEditorMode())
            {
                SystemOp.Resolve<FileService>().WriteFile(sceneData, FileService.GetSavedFilePath(SystemOp.Resolve<System>().ConfigSO.SceneDataToLoad.name));
            }
            else
            {
                new FileService().WriteFile(sceneData, FileService.GetSavedFilePath(GetAssetName?.Invoke()));
            }
        }

        public void PrepareSceneDataToRuntime()
        {
            var sceneData = ExportSceneData();
            SystemOp.Resolve<CrossSceneDataHolder>().Set(sceneData);
        }

        #region Load Scene Data

        public void LoadScene()
        {
            InitializeScene();
            SetupSceneDefaultObjects();
            EditorOp.Resolve<EditorEssentialsLoader>().LoadEssentials();
        }

        private void InitializeScene()
        {
            string data;
            var prevState = SystemOp.Resolve<System>().PreviousStudioState;
            if (prevState != StudioState.Runtime && SystemOp.Resolve<System>().ConfigSO.PickupSavedData)
            {
                var saveFilePath = FileService.GetSavedFilePath(SystemOp.Resolve<System>().ConfigSO.SceneDataToLoad.name);
                data = SystemOp.Resolve<FileService>().ReadFromFile(saveFilePath);
            }
            else
            {
                data = SystemOp.Resolve<CrossSceneDataHolder>().Get();
            }
            if (string.IsNullOrEmpty(data))
            {
                return;
            }
            RecreateScene(data);
        }

#if UNITY_EDITOR
        public
#endif
            void RecreateScene(string data)
        {
            var worldData = JsonConvert.DeserializeObject<WorldData>(data);
            for (int i = 0; i < worldData.entities.Length; i++)
            {
                var entity = worldData.entities[i];
                SpawnObjects(entity);
            }
            if (!Helper.IsInUnityEditorMode())
            {
                var metaData = worldData.metaData;
                if (metaData.Equals(default(WorldMetaData))) return;
                PlayerSpawnPoint = metaData.playerSpawnPoint;
            }
        }

        private void SpawnObjects(VirtualEntity entity)
        {
            GameObject generatedObj;
            var trs = new Vector3[] { entity.position, entity.rotation, entity.scale };
            generatedObj = RuntimeWrappers.SpawnObject(entity.assetType, entity.assetPath, entity.primitiveType, trs);
            if (generatedObj == null)
            {
                return;
            }
            generatedObj.name = entity.name;
            SetColliderData(generatedObj, entity.metaData);
            AttachComponents(generatedObj, entity);
            HandleChildren(generatedObj, entity.children);
        }

        private void AttachComponents(GameObject gameObject, VirtualEntity entity, Action<GameObject, VirtualEntity> onComponentDependencyFound = null)
        {
            for (int i = 0; i < entity.components.Length; i++)
            {
                if (Helper.IsInUnityEditorMode())
                {
                    var metaData = gameObject.AddComponent<EditorMetaData>();
                    metaData.componentData = entity.components[i];
                }
                else
                {
                    if (EditorOp.Resolve<DataProvider>() != null)
                    {
                        var type = EditorOp.Resolve<DataProvider>().GetVariance(entity.components[i].type);
                        if (type == null || string.IsNullOrEmpty((string)entity.components[i].data))
                        {
                            continue;
                        }
                        var component = gameObject.AddComponent(type) as IComponent;
                        component?.Import(entity.components[i]);
                    }
                    else
                    {
                        onComponentDependencyFound?.Invoke(gameObject, entity);
                    }
                }
            }
        }

        public void HandleChildren(GameObject gameObject, VirtualEntity[] children, Action<GameObject, VirtualEntity> onComponentDependencyFound = null)
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
                    GameObject generatedObj;
                    var trs = new Vector3[] { childEntity.position, childEntity.rotation, childEntity.scale };
                    generatedObj = RuntimeWrappers.SpawnObject(childEntity.assetType, childEntity.assetPath, childEntity.primitiveType, trs);
                    child = generatedObj.transform;
                    child.SetParent(tr);
                    child.localScale = childEntity.scale.WorldToLocalScale(tr);
                }
                else
                {
                    child = gameObject.transform.GetChild(i);
                    RuntimeWrappers.AttachPrerequisities(child.gameObject, ResourceDB.GetItemData(childEntity.assetPath));
                    child.SetPositionAndRotation(childEntity.position, Quaternion.Euler(childEntity.rotation));
                    child.localScale = childEntity.scale.WorldToLocalScale(child.parent);
                }
                child.name = childEntity.name;
                SetColliderData(child.gameObject, childEntity.metaData);
                AttachComponents(child.gameObject, childEntity, onComponentDependencyFound);
                HandleChildren(child.gameObject, childEntity.children, onComponentDependencyFound);
            }
        }

        #endregion

        #region Export Scene Data

        private string ExportSceneData()
        {
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
                    TryHandleUnpackableGameObjectData(allGos[i], virtualEntities, ref worldMetaData);
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
            return json;
        }

        private VirtualEntity GetVirtualEntity(GameObject go, int index, bool shouldCheckForAssetPath)
        {
            var newEntity = new VirtualEntity
            {
                id = index,
                name = go.name,
                assetPath = shouldCheckForAssetPath ? GetAssetPath(go) : null,
                position = go.transform.position,
                rotation = go.transform.eulerAngles,
                scale = go.transform.parent != null ? go.transform.localScale.LocalToWorldScale(go.transform.parent) : go.transform.localScale,
                assetType = !string.IsNullOrEmpty(GetAssetPath(go)) ? AssetType.Prefab : GetAssetType(go),
                shouldLoadAssetAtRuntime = true
            };
            if (newEntity.assetType == AssetType.Primitive)
            {
                newEntity.primitiveType = GetPrimitiveType(go);
            }
            EntityBasedComponent[] entityComponents;
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
                var editorComponents = go.GetComponents<IComponent>();
                entityComponents = new EntityBasedComponent[editorComponents.Length];
                for (int j = 0; j < editorComponents.Length; j++)
                {
                    var (type, data) = editorComponents[j].Export();
                    entityComponents[j] = new EntityBasedComponent()
                    {
                        type = type,
                        data = data
                    };
                }
            }
            newEntity.components = entityComponents;
            var childrenEntities = new VirtualEntity[go.transform.childCount];
            for (int k = 0; k < go.transform.childCount; k++)
            {
                childrenEntities[k] = GetVirtualEntity(go.transform.GetChild(k).gameObject, k, true);
            }
            newEntity.children = childrenEntities;
            GetColliderData(go, ref newEntity.metaData);
            return newEntity;
        }

        private string GetAssetPath(GameObject go)
        {
            if (!Helper.IsInUnityEditorMode())
            {
                if (go.TryGetComponent(out StudioGameObject component) && component.itemData != null)
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
                    if (component.itemData != null && !string.IsNullOrEmpty(component.itemData.ResourcePath))
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
                if (go.TryGetComponent(out StudioGameObject component) && component.itemData != null)
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

        private void TryHandleUnpackableGameObjectData(GameObject go, List<VirtualEntity> virtualEntities, ref WorldMetaData metaData)
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

        public GameObject TimerManagerObj;
        public GameObject ScoreManagerObj;
        private List<string> modifiers = new();

        private void SetupSceneDefaultObjects()
        {
            editorCamera = Camera.main;
            var isDataPresent = SystemOp.Resolve<CrossSceneDataHolder>().Get("CameraPos", out var data);
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
            SystemOp.Resolve<CrossSceneDataHolder>().Set("CameraPos", editorCamera.transform.position);
            SystemOp.Resolve<CrossSceneDataHolder>().Set("CameraRot", editorCamera.transform.eulerAngles);
        }

        public void UpdateScoreModifiersCount(bool add, string id)
        {
            if (add)
            {
                if (modifiers.Contains(id))
                {
                    return;
                }
                modifiers.Add(id);
                SetupScoreManager(true);
            }
            else
            {
                if (modifiers.Contains(id))
                {
                    modifiers.Remove(id);
                }
            }
            if (modifiers.Count == 0)
            {
                SetupScoreManager(false);
            }
        }

        private void SetupScoreManager(bool create)
        {
            if (create)
            {
                if (ScoreManagerObj)
                {
                    return;
                }
                EditorOp.Resolve<EditorEssentialsLoader>().Load(EditorObjectType.Score, out GameObject scoreManagerObj);
                scoreManagerObj.transform.SetAsFirstSibling();
            }
            else if (ScoreManagerObj)
            {
                Object.Destroy(ScoreManagerObj);
            }
        }

        #endregion
    }
}
