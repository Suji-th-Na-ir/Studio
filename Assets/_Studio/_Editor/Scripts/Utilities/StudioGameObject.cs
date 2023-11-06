using System;
using GLTFast;
using GLTFast.Schema;
using UnityEngine;

namespace Terra.Studio
{
    public class StudioGameObject : MonoBehaviour
    {
        public AssetType assetType;
        public ResourceDB.ResourceItemData itemData;
        public EditorObjectType type;

        private void Start()
        {
            if (assetType == AssetType.RemotePrefab)
            {
                LoadModel();
            }
        }

        private GltfObjectLoader _loader;

        public void LoadModel()
        {
            if (_loader == null)
            {
                _loader = gameObject.AddComponent<GltfObjectLoader>();
                _loader.unique_name = itemData.Name;
                _loader.importSettings = new ImportSettings
                {
                    lazyLoadTextures = true,
                    customBasePathForTextures = true,
                    additionToBasePath = "textures/"
                };
                _loader.cloudUrl = itemData.AbsolutePath;
            }
            _loader.LoadModel(ModelLoaded);
        }

        public void ModelLoaded()
        {
            RuntimeWrappers.ResolveTRS(gameObject, itemData);
            _loader.LoadTextures();   
        }
    }
}
