using System;
using System.IO;
using System.Text;
using GLTFast;
using GLTFast.Loading;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;

namespace Terra.Studio
{
    [Serializable]
    public class TempStruct
    {
        public BufferData[] buffers;
    }

    [Serializable]
    public struct BufferData
    {
        public string uri;
        public int byteLength;
    }
    public class GltfObjectLoader : MonoBehaviour
    {
        [SerializeField] public string cloudUrl;
        public ImportSettings importSettings;

        [NonSerialized] public string unique_name;
        private GltfImport importer;
        private GameObjectInstantiator _instantiator;
        private bool modelLoaded;


        private CacheValidator _cacheValidator;

        public CacheValidator CacheValidator
        {
            get
            {
                if (_cacheValidator == null)
                {
                    _cacheValidator = SystemOp.Resolve<CacheValidator>();
                }

                return _cacheValidator;
            }
        }

        public string FullUrl
        {
            get => cloudUrl;
            set
            {
                cloudUrl = value;
            }
        }
        public string OgUrl { get; private set; }

        private async void Test(bool inCache, string localPath, Action callback)
        {
            if (inCache)
            {
                importer = new GltfImport(new LocalFileProvider());
                // FullUrl = "file://" + localPath;
                FullUrl = localPath;
                Debug.Log($"Loading from cache!...{FullUrl}");
                var success = await importer.Load(FullUrl, importSettings);
                if (!success)
                {
                    Debug.LogError($"Model not loaded");
                    return;
                }

                Debug.Log($"Model loaded.");
                importer.ForceCleanBuffers();
                _instantiator = new GameObjectInstantiator(importer, transform);
                modelLoaded = await importer.InstantiateMainSceneAsync(_instantiator, default);
                if (modelLoaded)
                {
                    callback?.Invoke();
                }
                else
                {
                    Debug.LogError($"Model loading failed");
                }
            }
            else
            {
                importer = new GltfImport(new DefaultDownloadProvider());
                Debug.Log($"Loading normally");
                var success = await importer.Load(FullUrl, importSettings);
                if (!success)
                {
                    Debug.LogError($"Model not loaded");
                    return;
                }
                Debug.Log($"Model loaded.");

                var json = importer.FinalJson;
                var buffers = importer.m_Buffers;
                
                var converted = JsonConvert.DeserializeObject<TempStruct>(json);
                for (var i = 0; i < buffers.Length; i++)
                {
                    var bufferData = buffers[i];
                    var bufferName = converted.buffers[i].uri;
                    // string result = Encoding.UTF8.GetString(bufferData, 0, bufferData.Length);
                    var path = Path.Combine(unique_name, bufferName);
                    CacheValidator.Save(path, bufferData, $"{unique_name}.bin");
                }
                
                Uri uri = new Uri(FullUrl);
                var gltfFileName = Path.GetFileName(uri.LocalPath);
                CacheValidator.Save(Path.Combine(unique_name, gltfFileName), json, $"{unique_name}.gltf");
                importer.ForceCleanBuffers();
                _instantiator = new GameObjectInstantiator(importer, transform);
                modelLoaded = await importer.InstantiateMainSceneAsync(_instantiator, default);

                if (modelLoaded)
                {
                    callback?.Invoke();
                }
                else
                {
                    Debug.LogError($"Model loading failed");
                }
            }
        }
        
        public void LoadModel(Action cb)
        {
            OgUrl = FullUrl;
            Debug.Log($"Loading started.");
            CacheValidator.IsFileInCache($"{unique_name}.gltf",(inCache,localPath) =>
            {
                Test(inCache, localPath, cb);
            });
        }
        
        public async void LoadTextures()
        {
            Debug.Log($"Loading textures");
            if (!modelLoaded)
            {
                return;
            }
            Debug.Log($"textures!");
            await importer.PrepareTextures(UriHelper.GetBaseUri(new Uri(OgUrl)));
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