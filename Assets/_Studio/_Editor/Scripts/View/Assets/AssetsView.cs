using Newtonsoft.Json;
using Terra.Studio.Behaviour;
using Terra.Studio.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Terra.Studio
{
    public class AssetsView : View
    {
        private const string ghostMatPath = "Editortime/Materials/Ghost";
        private const string assetsWindowButtonPath = "Editortime/Prefabs/AssetsWindow/AssetWindow_Button";
        private const string gltfObjectLoader = "Editortime/Prefabs/AssetsWindow/GltfObjectLoader";
        private const string sampleJson = "Editortime/Prefabs/AssetsWindow/Sample";

        private bool useSampleJson = true;
        [SerializeField] private int numberOfAssetsToShowForNow = 10;
        private AssetsAPIResponse _fullData; 
        public void Awake()
        {
            EditorOp.Register(this);
        }

        private void OnDestroy()
        {
            EditorOp.Unregister(this);
        }
        
        public override void Init()
        {
            AssetsWindowAPI api = new AssetsWindowAPI();
            Debug.Log($"Started downloading!");
            if (!useSampleJson)
            {
                api.DoRequest(Callback);
            }
            else
            {
                var text = EditorOp.Load<TextAsset>(sampleJson);
                Callback(true, text.text);
            }
        }

        private void Callback(bool success, string response)
        {
            Debug.Log($"Success? => {success}");
            _fullData = JsonConvert.DeserializeObject<AssetsAPIResponse>(response);
            Debug.Log($"{_fullData.success}...{_fullData.message}");
            Setup();
        }

        private void Setup()
        {
            var loaderPrefab = EditorOp.Load<GltfObjectLoader>(gltfObjectLoader);
            var buttonPrefab = EditorOp.Load<GameObject>(assetsWindowButtonPath);
            var ghostMat = EditorOp.Load<Material>(ghostMatPath);
            var temp = GetComponentInChildren<GridLayoutGroup>();
            for (var i = 0; i < _fullData.data.Length && i < numberOfAssetsToShowForNow; i++)
            {
                var data = _fullData.data[i];
                var button = Instantiate(buttonPrefab, temp.transform);
                button.GetComponent<DraggableBehaviour>().Init(data, loaderPrefab, ghostMat);
            }
        }

        private Texture GetThumbnailForModelID(AssetData data)
        {
            return null;
        }
        

    }
}