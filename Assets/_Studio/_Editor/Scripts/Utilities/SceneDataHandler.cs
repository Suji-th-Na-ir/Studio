using System;
using UnityEngine;
using PlayShifu.Terra;
using Newtonsoft.Json;
using RuntimeInspectorNamespace;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Terra.Studio
{
    public class SceneDataHandler
    {
        public Func<GameObject, string> TryGetAssetPath;

        public void Save()
        {
            var sceneData = ExportSceneData();
            if (Helper.IsInRTEditModeInUnityEditor())
            {
                SystemOp.Resolve<FileService>().WriteFile(sceneData, FileService.GetSavedFilePath());
            }
            else
            {
                new FileService().WriteFile(sceneData, FileService.GetSavedFilePath());
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
        }

        private void InitializeScene()
        {
            string data;
            var prevState = SystemOp.Resolve<System>().PreviousStudioState;
            if (prevState != StudioState.Runtime && SystemOp.Resolve<System>().ConfigSO.PickupSavedData)
            {
                var saveFilePath = FileService.GetSavedFilePath();
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
        }

        private void SpawnObjects(VirtualEntity entity)
        {
            GameObject generatedObj = null;
            var trs = new Vector3[] { entity.position, entity.rotation, entity.scale };
            switch (entity.assetType)
            {
                case AssetType.Empty:
                    generatedObj = RuntimeWrappers.SpawnEmpty(null, trs);
                    break;
                case AssetType.Primitive:
                    generatedObj = RuntimeWrappers.SpawnPrimitive(entity.primitiveType, ResourceDB.GetDummyItemData(entity.primitiveType), trs);
                    break;
                case AssetType.Prefab:
                    generatedObj = RuntimeWrappers.SpawnGameObject(entity.assetPath, ResourceDB.GetItemData(entity.assetPath), trs);
                    break;
            }
            if (generatedObj == null)
            {
                return;
            }
            generatedObj.name = entity.name;
            AttachComponents(generatedObj, entity.components);
            HandleChildren(generatedObj, entity.children);
        }

        private void AttachComponents(GameObject gameObject, EntityBasedComponent[] components)
        {
            for (int i = 0; i < components.Length; i++)
            {
                var type = EditorOp.Resolve<DataProvider>().GetVariance(components[i].type);
                if (type == null)
                {
                    continue;
                }
                var component = gameObject.AddComponent(type) as IComponent;
                component?.Import(components[i]);
            }
        }

        private void HandleChildren(GameObject gameObject, VirtualEntity[] children)
        {
            var tr = gameObject.transform;
            for (int i = 0; i < children.Length; i++)
            {
                var childEntity = children[i];
                Transform child;
                if (tr.childCount < i + 1)
                {
                    GameObject generatedObj = null;
                    var trs = new Vector3[] { childEntity.position, childEntity.rotation, childEntity.scale };
                    switch (childEntity.assetType)
                    {
                        case AssetType.Empty:
                            generatedObj = RuntimeWrappers.SpawnEmpty(null, trs);
                            break;
                        case AssetType.Primitive:
                            generatedObj = RuntimeWrappers.SpawnPrimitive(childEntity.primitiveType, ResourceDB.GetDummyItemData(childEntity.primitiveType), trs);
                            break;
                        case AssetType.Prefab:
                            generatedObj = RuntimeWrappers.SpawnGameObject(childEntity.assetPath, ResourceDB.GetItemData(childEntity.assetPath), trs);
                            break;
                    }
                    child = generatedObj.transform;
                    child.SetParent(tr);
                }
                else
                {
                    child = gameObject.transform.GetChild(i);
                    RuntimeWrappers.AttachPrerequisities(child.gameObject, ResourceDB.GetItemData(childEntity.assetPath));
                    child.SetPositionAndRotation(childEntity.position, Quaternion.Euler(childEntity.rotation));
                    child.localScale = childEntity.scale;
                }
                child.name = childEntity.name;
                AttachComponents(child.gameObject, childEntity.components);
                HandleChildren(child.gameObject, childEntity.children);
            }

            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                var child = children[i];
                var childTrs = gameObject.transform.GetChild(i);
                childTrs.SetPositionAndRotation(child.position, Quaternion.Euler(child.rotation));
                childTrs.localScale = child.scale;
                childTrs.name = child.name;
                AttachComponents(childTrs.gameObject, child.components);
                HandleChildren(childTrs.gameObject, child.children);
            }
        }

        #endregion

        #region Export Scene Data

        private string ExportSceneData()
        {
            var allGos = SceneManager.GetActiveScene().GetRootGameObjects();
            var virtualEntities = new List<VirtualEntity>();
            for (int i = 0; i < allGos.Length; i++)
            {
                if (!allGos[i].activeSelf || allGos[i].TryGetComponent(out HideInHierarchy _))
                {
                    continue;
                }
                Debug.Log($"Appending: {allGos[i]}");
                var entity = GetVirtualEntity(allGos[i], i, true);
                virtualEntities.Add(entity);
            }
            var worldData = new WorldData()
            {
                entities = virtualEntities.ToArray()
            };
            var json = JsonConvert.SerializeObject(worldData);
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
                scale = go.transform.localScale,
                assetType = !string.IsNullOrEmpty(GetAssetPath(go)) ? AssetType.Prefab : GetAssetType(go)
            };
            if (newEntity.assetType == AssetType.Primitive)
            {
                newEntity.primitiveType = GetPrimitiveType(go);
            }
            var editorComponents = go.GetComponents<IComponent>();
            var entityComponents = new EntityBasedComponent[editorComponents.Length];
            for (int j = 0; j < editorComponents.Length; j++)
            {
                var (type, data) = editorComponents[j].Export();
                entityComponents[j] = new EntityBasedComponent()
                {
                    type = type,
                    data = data
                };
            }
            newEntity.components = entityComponents;
            var childrenEntities = new VirtualEntity[go.transform.childCount];
            for (int k = 0; k < go.transform.childCount; k++)
            {
                childrenEntities[k] = GetVirtualEntity(go.transform.GetChild(k).gameObject, k, true);
            }
            newEntity.children = childrenEntities;
            return newEntity;
        }

        private string GetAssetPath(GameObject go)
        {
            if (Helper.IsInRTEditModeInUnityEditor())
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
            if (Helper.IsInRTEditModeInUnityEditor())
            {
                if (go.TryGetComponent(out StudioGameObject component))
                {
                    if (!string.IsNullOrEmpty(component.itemData.ResourcePath))
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
            if (Helper.IsInRTEditModeInUnityEditor())
            {
                if (go.TryGetComponent(out StudioGameObject component))
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

        #endregion
    }
}
