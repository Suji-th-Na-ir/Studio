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

        private GltfObjectLoader currGo;
        public void LoadStuff()
        {
            currGo = new GameObject().AddComponent<GltfObjectLoader>();
            currGo.Init( itemData.AbsolutePath,itemData.Name, new ImportSettings()
            {
                lazyLoadTextures = true,
                customBasePathForTextures = true,
                additionToBasePath = "textures/"
            });
            currGo.LoadModel((_,_) =>
            {
                currGo.LoadTextures();
                transform.GetPositionAndRotation(out var pos, out var rot);
                currGo.transform.SetPositionAndRotation(pos, rot);
                currGo.transform.localScale = transform.localScale;
            }, null);
        }
        // private void Start()
        // {
        //     if (assetType == AssetType.RemotePrefab)
        //     {
        //         LoadModel();
        //     }
        // }

        // private GltfObjectLoader _loader;
        //
        // public void LoadModel()
        // {
        //     if (_loader == null)
        //     {
        //         _loader = gameObject.AddComponent<GltfObjectLoader>();
        //         _loader.unique_name = itemData.Name;
        //         _loader.importSettings = new ImportSettings
        //         {
        //             lazyLoadTextures = true,
        //             customBasePathForTextures = true,
        //             additionToBasePath = "textures/"
        //         };
        //         _loader.cloudUrl = itemData.AbsolutePath;
        //     }
        //     // _loader.LoadModel(ModelLoaded(null));
        // }
        //
        // public void ModelLoaded()
        // {
        //     RuntimeWrappers.ResolveTRS(gameObject, itemData);
        //     _loader.LoadTextures();   
        // }
    }
}
