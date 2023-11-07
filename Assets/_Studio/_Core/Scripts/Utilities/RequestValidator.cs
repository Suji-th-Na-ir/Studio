using System;
using System.Collections.Generic;
using GLTFast;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Terra.Studio
{
    public class RequestValidator
    {
        private Action _onComplete;

        private Dictionary<string, string> uniqueToCloud;
        private LoadedPoolValidator _loadedPoolValidator;
        public void Prewarm(ref WorldData world, Action onComplete)
        {
            _loadedPoolValidator = SystemOp.Resolve<LoadedPoolValidator>();
            uniqueToCloud = new();
            _onComplete = onComplete;
            
            CheckForChildrenAlso(world.entities);
            if (uniqueToCloud.Count == 0)
            {
                _onComplete?.Invoke();
                return;
            }
            StartDownloading();
        }

        private void CheckForChildrenAlso(VirtualEntity[] objects)
        {
            foreach (var virtualEntity in objects)
            {
                if (virtualEntity.assetType == AssetType.RemotePrefab)
                {
                    if (!_loadedPoolValidator.IsInLoadedPool(virtualEntity.uniqueName))
                    {
                        uniqueToCloud.TryAdd(virtualEntity.uniqueName, virtualEntity.assetPath);    
                    }
                }
                CheckForChildrenAlso(virtualEntity.children);
            }
        }

        private void StartDownloading()
        {
            foreach (var (uniqueName, url) in uniqueToCloud)
            {
                DownloadedModel(uniqueName, url);
            }
        }

        private void DownloadedModel(string uniqueName, string url)
        {
            var go = new GameObject();
            go.AddComponent<HideInHierarchy>();
            var x= go.AddComponent<GltfObjectLoader>();
            
            x.Init( url,uniqueName, new ImportSettings()
            {
                lazyLoadTextures = true,
                customBasePathForTextures = true,
                additionToBasePath = "textures/"
            });
            
            x.LoadModel((loadedObject, preloaded) =>
            {
                if (loadedObject != null)
                {
                    x.LoadTextures(success =>
                    {
                        ModelFullyCompleted(uniqueName,loadedObject, success);
                        Object.Destroy(go);
                    });    
                }
                else
                {
                    ModelFullyCompleted(uniqueName, null,false);
                    Object.Destroy(go);
                }
                
            }, null);
        }

        private void ModelFullyCompleted(string uniqueName, GameObject loadedGo,bool success)
        {
            if (success)
            {
                uniqueToCloud.Remove(uniqueName);
                if (loadedGo)
                {
                    Object.Destroy(loadedGo);
                }
            }
            else
            {
                Debug.LogError($"{uniqueName} failed downloading");
                uniqueToCloud.Remove(uniqueName);
            }

            if (uniqueToCloud.Count == 0)
            {
                _onComplete?.Invoke();
            }
        }
    }
}