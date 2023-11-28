using System;
using System.IO;
using System.Threading;
using GLTFast;
using GLTFast.Loading;
using Newtonsoft.Json;
using UnityEngine;

namespace Terra.Studio
{
    public class GltfObjectLoader : MonoBehaviour
    {
        [SerializeField] private string cloudUrl;
        [SerializeField] private ImportSettings importSettings;

        private CacheValidator CacheValidator => _cacheValidator ??= SystemOp.Resolve<CacheValidator>();
        private LoadedPoolValidator LoadedPoolValidator => _loadedPoolValidator ??= SystemOp.Resolve<LoadedPoolValidator>();
        private string FullUrl
        {
            get => cloudUrl;
            set { cloudUrl = value; }
        }
        private string OgUrl { get; set; }

        public Transform LoadedObject { get; private set; }
        private CancellationTokenSource _cts;
        private CancellationToken _token;
        private string _uniqueName;
        private GltfImport _importer;
        private GameObjectInstantiator _instantiator;
        private bool _modelLoaded;

        private CacheValidator _cacheValidator;
        private LoadedPoolValidator _loadedPoolValidator;
        private Action<bool> onTextureLoadCompleted;

        private bool _loadTexturesCalled;
        private static readonly int BaseColorTextureSt = Shader.PropertyToID("baseColorTexture");
        private static readonly int EmissiveTextureSt = Shader.PropertyToID("emissiveTexture");
        private static readonly int OcclusionTextureSt = Shader.PropertyToID("occlusionTexture");
        private static readonly int NormalTextureSt = Shader.PropertyToID("normalTexture");
        private static readonly int MetallicRoughnessSt = Shader.PropertyToID("metallicRoughnessTexture");

        public void Init(string fullUrl, string uniqueName, ImportSettings settings)
        {
            cloudUrl = fullUrl;
            _uniqueName = uniqueName;
            importSettings = settings;
            _cts = new CancellationTokenSource();
        }

        public void SetPositionForDragInAssetsWindow(Vector3 pos)
        {
            if (LoadedObject != null)
            {
                LoadedObject.transform.position = pos;
            }
        }

        public void LoadModel(Action<GameObject, bool> onModelLoaded, Transform finalParent)
        {
            OgUrl = FullUrl;
            if (LoadedPoolValidator.IsInLoadedPool(_uniqueName))
            {
                Debug.Log($"Getting from pool");
                var temp = LoadedPoolValidator.GetDuplicateFromPool(_uniqueName);
                _modelLoaded = true;
                onModelLoaded?.Invoke(temp, true);
                // temp.transform.SetParent(transform);
                LoadedObject = temp.transform;
                LoadedObject.SetParent(finalParent);
                CompleteModelFlow();
                CompletedFlow();
            }
            else
            {
                Debug.Log($"Loading started.");
                
                var x = CacheValidator.IsFileInCache($"{_uniqueName}.gltf", out string localPath);
                LoadFromCacheOrDownload(x, localPath, onModelLoaded, finalParent);
            }
        }

        private async void LoadFromCacheOrDownload(bool inCache, string localPath, Action<GameObject, bool> callback, Transform finalParent)
        {
            if (inCache)
            {
                _importer = new GltfImport(new LocalFileProvider());
                FullUrl = localPath;
                Debug.Log($"Loading from cache!...{FullUrl}");
                var success = await _importer.Load(FullUrl, importSettings, _cts.Token);
                if (!success)
                {
                    Debug.LogError($"Model not loaded");
                    return;
                }
                Debug.Log($"Model loaded.");
                _importer.ForceCleanBuffers();
                _instantiator = new GameObjectInstantiator(_importer, transform);
                _modelLoaded = await _importer.InstantiateMainSceneAsync(_instantiator, default);
                Debug.Log($"Model instantiated...{_modelLoaded}");
                if (_modelLoaded)
                {
                    LoadedObject = transform.GetChild(0);
                    LoadedObject.SetParent(finalParent);
                    callback?.Invoke(LoadedObject.gameObject, false);
                }
                else
                {
                    Debug.LogError($"Model loading failed");
                }
            }
            else
            {
                _importer = new GltfImport(new DefaultDownloadProvider());
                Debug.Log($"Loading normally");
                var success = await _importer.Load(FullUrl, importSettings, _cts.Token);
                if (!success)
                {
                    Debug.LogError($"Model not loaded");
                    return;
                }

                Debug.Log($"Model loaded.");

                var json = _importer.FinalJson;
                var buffers = _importer.m_Buffers;

                var converted = JsonConvert.DeserializeObject<GltfData>(json);
                for (var i = 0; i < buffers.Length; i++)
                {
                    var bufferData = buffers[i];
                    var bufferName = converted.buffers[i].uri;
                    // string result = Encoding.UTF8.GetString(bufferData, 0, bufferData.Length);
                    var path = Path.Combine(_uniqueName, bufferName);
                    CacheValidator.Save(path, bufferData, $"{_uniqueName}.bin");
                }

                Uri uri = new Uri(FullUrl);
                var gltfFileName = Path.GetFileName(uri.LocalPath);
                CacheValidator.Save(Path.Combine(_uniqueName, gltfFileName), json, $"{_uniqueName}.gltf");
                _importer.ForceCleanBuffers();
                _instantiator = new GameObjectInstantiator(_importer, transform);
                _modelLoaded = await _importer.InstantiateMainSceneAsync(_instantiator, default);

                if (_modelLoaded)
                {
                    LoadedObject = transform.GetChild(0);
                    LoadedObject.SetParent(finalParent);
                    callback?.Invoke(LoadedObject.gameObject, false);
                }
                else
                {
                    Debug.LogError($"Model loading failed");
                }
            }

            CompleteModelFlow();
            if (_loadTexturesCalled)
            {
                LoadTextures(onTextureLoadCompleted);
            }
        }

        public async void LoadTextures(Action<bool> onTexturesLoaded = null)
        {
            onTextureLoadCompleted = onTexturesLoaded;
            if (!_modelLoaded)
            {
                _loadTexturesCalled = true;
                // Debug.LogError($"Still Loading the model.", gameObject);
                return;
            }
            
            bool success = await _importer.PrepareTextures(UriHelper.GetBaseUri(new Uri(OgUrl)));

            var s_Parser = new GltfJsonUtilityParser();
            var rootBase = s_Parser.ParseJson(_importer.FinalJson);
            

            foreach (var (ren, meshResult) in _instantiator.results)
            {
                var materials = new Material[meshResult.materialIndices.Length];
                for (var index = 0; index < materials.Length; index++)
                {
                    var material = _importer.GetMaterial(meshResult.materialIndices[index]) ?? _importer.GetDefaultMaterial();

                    if (index >= rootBase.Materials.Count)
                    {
                        Debug.LogError($"Something is wrong with this model/material.");
                        materials[index] = material;
                        continue;
                    }
                    var baseTextureTrs = rootBase.Materials[index].PbrMetallicRoughness?.BaseColorTexture?.Extensions?.KHR_texture_transform;
                    var metallicRoughnessTrs = rootBase.Materials[index].PbrMetallicRoughness?.MetallicRoughnessTexture?.Extensions?.KHR_texture_transform;
                    var emissiveTextureTrs = rootBase.Materials[index].EmissiveTexture?.Extensions?.KHR_texture_transform;
                    var normalTextureTrs = rootBase.Materials[index].NormalTexture?.Extensions?.KHR_texture_transform;
                    var occlusionTextureTrs = rootBase.Materials[index].OcclusionTexture?.Extensions?.KHR_texture_transform;

                    if (baseTextureTrs != null)
                    {
                        material.SetTextureScale(BaseColorTextureSt, new Vector2(baseTextureTrs.scale[0],baseTextureTrs.scale[1]));
                        material.SetTextureOffset(BaseColorTextureSt, new Vector2(baseTextureTrs.offset[0],baseTextureTrs.offset[1]));
                    }

                    if (metallicRoughnessTrs != null)
                    {
                        material.SetTextureScale(MetallicRoughnessSt,new Vector2(metallicRoughnessTrs.scale[0],metallicRoughnessTrs.scale[1]));
                        material.SetTextureOffset(MetallicRoughnessSt,new Vector2(metallicRoughnessTrs.offset[0],metallicRoughnessTrs.offset[1]));
                    }

                    if (emissiveTextureTrs != null)
                    {
                        material.SetTextureScale(EmissiveTextureSt,new Vector2(emissiveTextureTrs.scale[0],emissiveTextureTrs.scale[1]));
                        material.SetTextureOffset(EmissiveTextureSt,new Vector2(emissiveTextureTrs.offset[0],emissiveTextureTrs.offset[1]));
                    }

                    if (normalTextureTrs != null)
                    {
                        material.SetTextureScale(NormalTextureSt,new Vector2(normalTextureTrs.scale[0],normalTextureTrs.scale[1]));
                        material.SetTextureOffset(NormalTextureSt,new Vector2(normalTextureTrs.offset[0],normalTextureTrs.offset[1]));
                    }

                    if (occlusionTextureTrs != null)
                    {
                        material.SetTextureScale(OcclusionTextureSt,new Vector2(occlusionTextureTrs.scale[0],occlusionTextureTrs.scale[1]));
                        material.SetTextureOffset(OcclusionTextureSt,new Vector2(occlusionTextureTrs.offset[0],occlusionTextureTrs.offset[1]));
                    }
                    
                    materials[index] = material;
                }

                ren.sharedMaterials = materials;
            }
            
            LoadedPoolValidator.AddToPool(_uniqueName, LoadedObject.gameObject);
            onTextureLoadCompleted?.Invoke(success);
            _importer.DisposeVolatileDataFromTextures();
            CompletedFlow();
        }

        private void CompleteModelFlow()
        {
            if (LoadedObject != null)
            {
                var renderers = LoadedObject.GetComponentsInChildren<MeshFilter>();
                foreach (var meshFilter in renderers)
                {
                    if (!meshFilter.TryGetComponent(out MeshCollider meshCol))
                    {
                        meshCol = meshFilter.gameObject.AddComponent<MeshCollider>();
                        meshCol.sharedMesh = meshFilter.mesh;
                    }
                    else
                    {
                        meshCol.sharedMesh = meshFilter.mesh;
                    }
                }

                if (!LoadedObject.gameObject.TryGetComponent(out StudioGameObject sgo))
                {
                    sgo = LoadedObject.gameObject.AddComponent<StudioGameObject>();
                }
                sgo.assetType = AssetType.RemotePrefab;
                sgo.itemData = new ResourceDB.ResourceItemData(_uniqueName, OgUrl, OgUrl, "", "");
            }
        }

        private void CompletedFlow()
        {
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
        }
    }
}