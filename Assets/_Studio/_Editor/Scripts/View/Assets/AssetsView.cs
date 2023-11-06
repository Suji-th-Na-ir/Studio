using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Terra.Studio.Behaviour;
using Terra.Studio.Data;
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
        private int numberOfAssetsToShowForNow = 30;
        private AssetsAPIResponse _fullData;
        private ButtonScroll _scroll;
        private SearchBar _search;
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
                api.DoRequest(OnDataFetchSuccess);
            }
            else
            {
                var text = EditorOp.Load<TextAsset>(sampleJson);
                OnDataFetchSuccess(true, text.text);
            }
        }

        private void OnDataFetchSuccess(bool success, string response)
        {
            _fullData = JsonConvert.DeserializeObject<AssetsAPIResponse>(response);

            currentData = _fullData.data;
            
            _scroll = GetComponentInChildren<ButtonScroll>();
            _scroll.Init(_fullData.data.Length/numberOfAssetsToShowForNow, PageChanged);

            _search = GetComponentInChildren<SearchBar>();
            _search.Init(OnSearch);
        }
        
        private void OnSearch(string searchString)
        {
            if (!string.IsNullOrEmpty(searchString))
            {
                StartCoroutine(SearchRoutine(searchString));
            }
            else
            {
                currentData = _fullData.data;
                PageChanged(1);
            }
        }

        private IEnumerator SearchRoutine(string query)
        {
            Debug.Log($"Started searching for {query}");
            List<AssetData> bla = new(); 
            for (var i = 0; i < _fullData.data.Length; i++)
            {
                var d = _fullData.data[i];
                var pass = d.display_name.Contains(query) || d.unique_name.Contains(query) || d.category.Contains(query);
                if (pass)
                {
                    bla.Add(d);
                }

                if (i%1000 == 0)
                {
                    yield return null;
                }
            }

            Debug.Log($"Ended searching for {query}");
            currentData = bla.ToArray();
            PageChanged(1);
        }

        private AssetData[] currentData;
        private void PageChanged(int obj)
        {
            ClearDataIfAny();
            
            var loaderPrefab = EditorOp.Load<GltfObjectLoader>(gltfObjectLoader);
            var buttonPrefab = EditorOp.Load<GameObject>(assetsWindowButtonPath);
            var ghostMat = EditorOp.Load<Material>(ghostMatPath);
            var temp = GetComponentInChildren<GridLayoutGroup>();
            var x = obj - 1;
            int counter = 0;
            for (var i = numberOfAssetsToShowForNow * x; i < currentData.Length && counter<numberOfAssetsToShowForNow; i++)
            {
                counter++;
                var data = currentData[i];
                var button = Instantiate(buttonPrefab, temp.transform);
                button.GetComponent<DraggableBehaviour>().Init(data, loaderPrefab, ghostMat);
            }
            
        }

        private void ClearDataIfAny()
        {
            var temp = GetComponentInChildren<GridLayoutGroup>().transform;
            foreach (Transform o in temp)
            {
                Destroy(o.gameObject);
            }
        }

        private Texture GetThumbnailForModelID(AssetData data)
        {
            return null;
        }
    }
}