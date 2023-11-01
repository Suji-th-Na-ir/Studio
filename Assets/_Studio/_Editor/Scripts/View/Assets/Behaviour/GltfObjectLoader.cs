using System;
using System.IO;
using GLTFast;
using UnityEngine;

namespace Terra.Studio
{
    public class GltfObjectLoader : MonoBehaviour
    {
        [SerializeField] public string url;
        [SerializeField] private ImportSettings importSettings;
        [SerializeField] private bool loadFromStreamingAssets;
        
        private GltfImport importer;
        private GameObjectInstantiator _instantiator;
        private bool modelLoaded;
        
        public string FullUrl => loadFromStreamingAssets ? Path.Combine(Application.streamingAssetsPath, url) : url;
        
        public async void LoadModel(Action callback)
        {
            importer = new GltfImport();
            // try
            {
                await importer.Load(FullUrl, importSettings);
                Debug.Log($"Finished importing.");
                _instantiator = new GameObjectInstantiator(importer, transform);
                Debug.Log($"Instantiating.");
                modelLoaded = await importer.InstantiateMainSceneAsync(_instantiator, default);
                Debug.Log($"Finished Instantiating.");
            }
            // catch (Exception e)
            {
                // Debug.LogError($"JAI MATA DI {e}...{e.Message}....{e.Source}");
            }
            callback?.Invoke();
            // LoadTextures();
        }
        
        public async void LoadTextures()
        {
            if (!modelLoaded)
            {
                return;
            }
            await importer.PrepareTextures(UriHelper.GetBaseUri(new Uri(FullUrl)));
            foreach (var (ren, meshResult) in _instantiator.results)
            {
                var materials = new Material[meshResult.materialIndices.Length];
                for (var index = 0; index < materials.Length; index++)
                {
                    var material = importer.GetMaterial(meshResult.materialIndices[index]) ?? importer.GetDefaultMaterial();
                    materials[index] = material;
                }
                ren.sharedMaterials = materials;
            }
        }
    }
}