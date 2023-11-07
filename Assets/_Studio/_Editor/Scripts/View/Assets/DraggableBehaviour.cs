using System;
using System.Collections;
using DG.Tweening;
using GLTFast;
using Terra.Studio.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Terra.Studio.Behaviour
{
    public class DraggableBehaviour : MonoBehaviour
    {
        [SerializeField] private Transform loadingIcon;
        [SerializeField] private RawImage image;
        [SerializeField] private TextMeshProUGUI displayNameText;
        
        private GltfObjectLoader _loaderPrefab;
        private bool _running = false;
        private AssetData _assetData;
        private Material _ghostMaterial;

        private GltfObjectLoader currGo;
        private GameObject actuallyLoadedGo;
        
        public void Init(AssetData data, GltfObjectLoader lp, Material ghMat)
        {
            _running = true;
            displayNameText.text = data.display_name;
            _assetData = data;
            _loaderPrefab = lp;
            _ghostMaterial = ghMat;

            StartCoroutine(SendThumbnailRequest());
        }

        public void OnPointerDown()
        {
            if (!_running)
                return;

            if (currGo == null)
            {
                currGo = new GameObject().AddComponent<GltfObjectLoader>();
                currGo.Init( _assetData.gltf[0],_assetData.unique_name, new ImportSettings()
                {
                    lazyLoadTextures = true,
                    customBasePathForTextures = true,
                    additionToBasePath = "textures/"
                });
                currGo.LoadModel(ModelDownloaded, null);
                // currGo.assetType = AssetType.RemotePrefab;
                // var url = _assetData.gltf[0].Replace("https", "http");
                // currGo.itemData = new ResourceDB.ResourceItemData(_assetData.unique_name,url,url, "","", remoteAsset:true );
                // currGo.LoadModel();
            }
        }

        private void ModelDownloaded(GameObject loadedGo, bool preloaded)
        {
            actuallyLoadedGo = loadedGo;
            if (!preloaded)
            {
                var renderers = loadedGo.GetComponentsInChildren<Renderer>();
                foreach (var renderer1 in renderers)
                {
                    renderer1.material = _ghostMaterial;
                }
            }
            else
            {
                // was preloaded no need for ghost mat, as textures are loaded.
            }
        }

        public void OnPointerDrag()
        {
            if (!_running)
                return;
            
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit))
            {
                if (currGo)
                {
                    currGo.transform.position = hit.point;
                    currGo.SetPosition(hit.point);
                }
                else if (actuallyLoadedGo)
                {
                    actuallyLoadedGo.transform.position = hit.point;
                    // actuallyLoadedGo.SetPosition(hit.point);
                }
            }
        }

        public void OnPointerUp()
        {
            if (!_running)
                return;
            if (currGo != null)
            {
                currGo.LoadTextures();
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    Destroy(currGo.gameObject);
                }
            }
            currGo = null;
        }

        private void OnDestroy()
        {
            if (textureDownloadRoutine != null)
            {
                StopCoroutine(textureDownloadRoutine);
            }
            
            if (textureRequest != null)
            {
                textureRequest.Abort();
                if (textureRequest.downloadHandler is DownloadHandlerTexture dht && textureRequest.isDone)
                {
                    Destroy(dht.texture);
                }
                textureRequest.Dispose();
            }
        }

        private Coroutine textureDownloadRoutine;
        private UnityWebRequest textureRequest;
        private IEnumerator SendThumbnailRequest()
        {
            textureRequest = UnityWebRequestTexture.GetTexture(_assetData.thumbnails[0].Replace("https","http"));
            textureRequest.SendWebRequest();
            while (!textureRequest.isDone)
            {
                loadingIcon.RotateAround(loadingIcon.position, loadingIcon.forward, 100 * Time.deltaTime);
                yield return null;
            }

            if (textureRequest.result == UnityWebRequest.Result.Success)
            {
                image.texture = (textureRequest.downloadHandler as DownloadHandlerTexture)?.texture;
                image.enabled = true;
                Destroy(loadingIcon.gameObject);
            }
            else
            {
                textureRequest.Abort();
                textureRequest.Dispose();
            }
        }
    }
}