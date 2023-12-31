using TMPro;
using System;
using GLTFast;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.EventSystems;

namespace Terra.Studio
{
    public class DraggableBehaviour : MonoBehaviour
    {
        [SerializeField] private Image outline;
        [SerializeField] private Transform loadingIcon;
        [SerializeField] private RawImage image;
        [SerializeField] private TextMeshProUGUI displayNameText;

        private Camera Camera => _camera = _camera != null ? _camera : Camera.main;

        private bool _running;
        private AssetData _assetData;
        private Material _ghostMaterial;

        private GltfObjectLoader _currGo;
        private GameObject _actuallyLoadedGo;
        private Coroutine _textureDownloadRoutine;
        private UnityWebRequest _textureRequest;
        private Camera _camera;
        private AssetsView _view;
        private bool isPointerUp;
        private bool isInitOnPointerDownDone;

        public void Init(AssetsView view, AssetData data, Material ghMat)
        {
            _view = view;
            _running = true;
            displayNameText.text = data.display_name;
            _assetData = data;
            _ghostMaterial = ghMat;
            StartCoroutine(SendThumbnailRequest());
        }

        private IEnumerator SendThumbnailRequest()
        {
            _textureRequest = UnityWebRequestTexture.GetTexture(_assetData.thumbnails[0]);
            _textureRequest.SendWebRequest();
            while (!_textureRequest.isDone)
            {
                loadingIcon.RotateAround(loadingIcon.position, loadingIcon.forward, 100 * Time.deltaTime);
                yield return null;
            }

            if (_textureRequest.result == UnityWebRequest.Result.Success)
            {
                image.texture = (_textureRequest.downloadHandler as DownloadHandlerTexture)?.texture;
                image.enabled = true;
                Destroy(loadingIcon.gameObject);
            }
            else
            {
                _textureRequest.Abort();
                _textureRequest.Dispose();
            }
        }

        public void OnPointerDown()
        {
            isPointerUp = false;
            isInitOnPointerDownDone = false;
            return;
        }

        private void ModelDownloaded(GameObject loadedGo, bool preloaded)
        {
            SetLayerOfAllChildren(loadedGo, "Ignore Raycast");
            _actuallyLoadedGo = loadedGo;
            if (!preloaded)
            {
                var renderers = loadedGo.GetComponentsInChildren<Renderer>();
                foreach (var renderer1 in renderers)
                {
                    Material[] materials = new Material[renderer1.sharedMaterials.Length];
                    for (int i = 0; i < materials.Length; i++)
                    {
                        materials[i] = _ghostMaterial;
                    }

                    renderer1.sharedMaterials = materials;
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

            if (!isPointerUp && !isInitOnPointerDownDone)
            {
                isInitOnPointerDownDone = true;
                _view.Dragger(this, true);
                if (_currGo == null)
                {
                    _currGo = new GameObject().AddComponent<GltfObjectLoader>();
                    _currGo.transform.forward = Vector3.ProjectOnPlane(-Camera.transform.forward, Vector3.up);
                    _currGo.gameObject.AddComponent<HideInHierarchy>();
                    _currGo.Init(_assetData.gltf[0], _assetData.unique_name, new ImportSettings()
                    {
                        lazyLoadTextures = true,
                        customBasePathForTextures = true,
                        additionToBasePath = "textures/"
                    });
                    _currGo.LoadModel(ModelDownloaded, null);
                }
                return;
            }

            var ray = Camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.SphereCast(ray, 1f, out var hit))
            {
                if (_currGo)
                {
                    _currGo.transform.position = hit.point;
                    _currGo.SetPositionForDragInAssetsWindow(hit.point);
                    _currGo.transform.forward = Vector3.ProjectOnPlane(-Camera.transform.forward, Vector3.up);
                }
                else if (_actuallyLoadedGo)
                {
                    _actuallyLoadedGo.transform.position = hit.point;
                    _actuallyLoadedGo.transform.forward = Vector3.ProjectOnPlane(-Camera.transform.forward, Vector3.up);
                }
            }
        }

        public void OnPointerUp()
        {
            if (!_running) return;
            isPointerUp = true;
            _view.Dragger(this, false);
            if (_currGo)
            {
                if (_currGo.LoadedObject)
                {
                    SetLayerOfAllChildren(_currGo.LoadedObject.gameObject, "Default");
                }
                _currGo.LoadTextures((status) =>
                {
                    if (!status)
                    {
                        Debug.Log($"Textures failed downloading. Why?");
                    }
                });
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    Destroy(_currGo.gameObject);
                }
            }
            _currGo = null;
            if (_actuallyLoadedGo)
            {
                EventSystem.current.SetSelectedGameObject(null);
                SetLayerOfAllChildren(_actuallyLoadedGo, "Default");
                EditorOp.Resolve<SelectionHandler>().Select(_actuallyLoadedGo);
            }
        }

        public void OnPointerEnter()
        {
            _view.Selected(this, true);
        }
        public void OnPointerExit()
        {
            _view.Selected(this, false);
        }

        public void SetHighlight(bool toSet)
        {
            var col = outline.color;
            col.a = toSet ? 1 : 0;
            outline.color = col;
        }

        private void SetLayerOfAllChildren(GameObject go, string layer)
        {
            var layerNum = LayerMask.NameToLayer(layer);
            var all = go.GetComponentsInChildren<Transform>();
            foreach (var transform1 in all)
            {
                transform1.gameObject.layer = layerNum;
            }
        }
        private void OnDestroy()
        {
            if (_textureDownloadRoutine != null)
            {
                StopCoroutine(_textureDownloadRoutine);
            }

            if (_textureRequest != null)
            {
                try
                {
                    if (!_textureRequest.isDone)
                    {
                        _textureRequest.Abort();
                    }
                    if (_textureRequest.downloadHandler is DownloadHandlerTexture dht && _textureRequest.isDone)
                    {
                        Destroy(dht.texture);
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log($"Exception while destroying texture request is: {ex}");
                }
                finally
                {
                    _textureRequest.Dispose();
                }
            }
        }
    }
}