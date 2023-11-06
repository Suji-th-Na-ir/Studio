using Terra.Studio.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Terra.Studio.Behaviour
{
    public class DraggableBehaviour : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI displayNameText;

        private GltfObjectLoader _loaderPrefab;
        private bool _running = false;
        private AssetData _assetData;
        private Material _ghostMaterial;

        private GltfObjectLoader currGo;
        public void Init(AssetData data, GltfObjectLoader lp, Material ghMat)
        {
            _running = true;
            displayNameText.text = data.display_name;
            _assetData = data;
            _loaderPrefab = lp;
            _ghostMaterial = ghMat;
        }

        public void OnPointerDown()
        {
            if (!_running)
                return;

            if (currGo == null)
            {
                currGo = Instantiate(_loaderPrefab);
                currGo.cloudUrl = _assetData.gltf[0].Replace("https","http");
                currGo.unique_name = _assetData.unique_name;
                currGo.LoadModel(ModelDownloaded);
            }
        }

        private void ModelDownloaded()
        {
            var renderers = currGo.GetComponentsInChildren<Renderer>();
            foreach (var renderer1 in renderers)
            {
                renderer1.material = _ghostMaterial;
            }
        }

        public void OnPointerDrag()
        {
            if (!_running)
                return;
            
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit))
            {
                currGo.transform.position = hit.point;
            }
        }

        public void OnPointerUp()
        {
            if (!_running)
                return;

            if (EventSystem.current.IsPointerOverGameObject())
            {
                Destroy(currGo.gameObject);
            }
            else
            {
                currGo.LoadTextures();
            }

            currGo = null;
        }
    }
}