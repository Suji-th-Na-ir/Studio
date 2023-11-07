using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        private LoadedPoolValidator _loadedPoolValidator;

        public CacheValidator CacheValidator => _cacheValidator ??= SystemOp.Resolve<CacheValidator>();

        public LoadedPoolValidator LoadedPoolValidator => _loadedPoolValidator ??= SystemOp.Resolve<LoadedPoolValidator>();

        public string FullUrl
        {
            get => cloudUrl;
            set { cloudUrl = value; }
        }

        public string OgUrl { get; private set; }

        private Transform loadedObject;
        private CancellationTokenSource _cts;
        private CancellationToken _token;

        public void Init(string fullUrl, string uniqueName,ImportSettings settings)
        {
            cloudUrl = fullUrl;
            this.unique_name = uniqueName;
            this.importSettings = settings;
            _cts = new CancellationTokenSource();
        }

        private async void Test(bool inCache, string localPath, Action<GameObject, bool> callback, Transform finalParent)
        {
            if (inCache)
            {
                importer = new GltfImport(new LocalFileProvider());
                FullUrl = localPath;
                Debug.Log($"Loading from cache!...{FullUrl}");
                var success = await importer.Load(FullUrl, importSettings, _cts.Token);
                if (!success)
                {
                    Debug.LogError($"Model not loaded");
                    return;
                }
                Debug.Log($"Model loaded.");
                importer.ForceCleanBuffers();
                _instantiator = new GameObjectInstantiator(importer, transform);
                modelLoaded = await importer.InstantiateMainSceneAsync(_instantiator, default);
                Debug.Log($"Model instantiated...{modelLoaded}");
                if (modelLoaded)
                {
                    loadedObject = transform.GetChild(0);
                    loadedObject.SetParent(finalParent);
                    callback?.Invoke(loadedObject.gameObject, false);
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
                var success = await importer.Load(FullUrl, importSettings, _cts.Token);
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
                    loadedObject = transform.GetChild(0);
                    loadedObject.SetParent(finalParent);
                    callback?.Invoke(loadedObject.gameObject, false);
                }
                else
                {
                    Debug.LogError($"Model loading failed");
                }
            }

            if (loadTexturesCalled)
            {
                LoadTextures();
            }
        }

        public void SetPosition(Vector3 pos)
        {
            if (loadedObject != null)
            {
                loadedObject.transform.position = pos;
            }
        }

        public void LoadModel(Action<GameObject, bool> cb, Transform finalParent)
        {
            OgUrl = FullUrl;
            if (LoadedPoolValidator.IsInLoadedPool(unique_name))
            {
                Debug.Log($"Getting from pool");
                var temp = LoadedPoolValidator.GetDuplicateFromPool(unique_name);
                modelLoaded = true;
                cb?.Invoke(temp, true);
                // temp.transform.SetParent(transform);
                loadedObject = temp.transform;
                loadedObject.SetParent(finalParent);
                CompletedFlow();
            }
            else
            {
                Debug.Log($"Loading started.");
                CacheValidator.IsFileInCache($"{unique_name}.gltf",
                    (inCache, localPath) => { Test(inCache, localPath, cb, finalParent); });
            }
        }

        private bool loadTexturesCalled;
        public async void LoadTextures()
        {
            if (!modelLoaded)
            {
                loadTexturesCalled = true;
                Debug.LogError($"Still Loading the model.", gameObject);
                return;
            }
            
            await importer.PrepareTextures(UriHelper.GetBaseUri(new Uri(OgUrl)));
            
            foreach (var (ren, meshResult) in _instantiator.results)
            {
                var materials = new Material[meshResult.materialIndices.Length];
                for (var index = 0; index < materials.Length; index++)
                {
                    var material = importer.GetMaterial(meshResult.materialIndices[index]) ??
                                   importer.GetDefaultMaterial();
                    materials[index] = material;
                }

                ren.sharedMaterials = materials;
            }
            // Debug.Log($"{_loadedPoolValidator}....{transform.GetChild(0)}");
            // Debug.Break();
            LoadedPoolValidator.AddToPool(unique_name, loadedObject.gameObject);
            importer.DisposeVolatileDataFromTextures();
            CompletedFlow();
        }

        private void CompletedFlow()
        {
            if (loadedObject != null)
            {
                var sgo = loadedObject.gameObject.AddComponent<StudioGameObject>();
                sgo.assetType = AssetType.RemotePrefab;
                sgo.itemData = new ResourceDB.ResourceItemData(unique_name, OgUrl, OgUrl, "","",remoteAsset:true);
                
                transform.GetPositionAndRotation(out var pos, out var rot);
                loadedObject.SetPositionAndRotation(pos,rot);
                loadedObject.transform.localScale = transform.localScale;
            }
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
        }
    }
}