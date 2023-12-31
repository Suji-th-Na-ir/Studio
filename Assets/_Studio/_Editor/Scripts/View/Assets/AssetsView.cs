using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace Terra.Studio
{
    public class AssetsView : View
    {
        private const string ghostMatPath = "Editortime/Materials/Ghost";
        private const string assetsWindowButtonPath = "Editortime/Prefabs/AssetsWindow/AssetWindow_Button";
        private const string sampleJson = "Editortime/Prefabs/AssetsWindow/Sample";

        private bool useSampleJson = true;
        private const int NumberOfAssetsToShowForNow = 21;
        private AssetsAPIResponse _fullData;
        private ButtonScroll _scroll;
        private SearchBar _search;
        private AssetData[] _currentData;
        [SerializeField] private GridLayoutGroup m_refMainGridGroup;

        public void Awake()
        {
            EditorOp.Register(this);
        }

        public override void Init()
        {
            var api = new AssetsWindowAPI();
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
            var data = JsonConvert.DeserializeObject<AssetsAPIResponse>(response);
            data.data = data.data.Where(x => x.flags != null && x.flags.Any(y => y == "Premium")).ToArray();
            _fullData = data;
            _currentData = _fullData.data;
            _scroll = GetComponentInChildren<ButtonScroll>();
            _scroll.Init(_fullData.data.Length / NumberOfAssetsToShowForNow, PageChanged);
            _search = GetComponentInChildren<SearchBar>();
            _search.Init(OnSearch);
            MakeUIBetter();
        }

        private void MakeUIBetter()
        {
            int columnCount = m_refMainGridGroup.constraintCount;
            float ySize = m_refMainGridGroup.gameObject.GetComponent<RectTransform>().rect.height / (NumberOfAssetsToShowForNow / columnCount);
            m_refMainGridGroup.cellSize = new Vector2(m_refMainGridGroup.cellSize.x, ySize - m_refMainGridGroup.spacing.y);
        }

        private void OnSearch(string searchString)
        {
            if (!string.IsNullOrEmpty(searchString))
            {
                StartCoroutine(SearchRoutine(searchString));
            }
            else
            {
                _currentData = _fullData.data;
                _scroll.Init(_currentData.Length / NumberOfAssetsToShowForNow, PageChanged);
            }
        }

        private IEnumerator SearchRoutine(string query)
        {
            List<AssetData> queriedAssetData = new(); 
            for (var i = 0; i < _fullData.data.Length; i++)
            {
                var d = _fullData.data[i];
                var pass = d.display_name.Contains(query) || d.unique_name.Contains(query) || d.category.Contains(query);
                if (pass)
                {
                    queriedAssetData.Add(d);
                }

                if (i % 1000 == 0)
                {
                    yield return null;
                }
            }
            
            _currentData = queriedAssetData.ToArray();
            _scroll.Init(_currentData.Length/NumberOfAssetsToShowForNow, PageChanged);
        }

        private void PageChanged(int obj)
        {
            ClearDataIfAny();

            var buttonPrefab = EditorOp.Load<GameObject>(assetsWindowButtonPath);
            var ghostMat = EditorOp.Load<Material>(ghostMatPath);
            var temp = GetComponentInChildren<GridLayoutGroup>();
            var x = Mathf.Max(obj - 1, 0);
            int counter = 0;
            for (var i = NumberOfAssetsToShowForNow * x; i < _currentData.Length && counter < NumberOfAssetsToShowForNow; i++)
            {
                counter++;
                var data = _currentData[i];
                var button = Instantiate(buttonPrefab, temp.transform);
                button.GetComponent<DraggableBehaviour>().Init(this, data, ghostMat);
            }
            MakeUIBetter();
        }

        private void ClearDataIfAny()
        {
            var temp = GetComponentInChildren<GridLayoutGroup>().transform;
            foreach (Transform o in temp)
            {
                Destroy(o.gameObject);
            }
        }


        private DraggableBehaviour currHighlight;
        private bool currentlyDragging;

        public void Selected(DraggableBehaviour currHover, bool highlight)
        {
            if (currentlyDragging)
                return;

            if (currHighlight == currHover && !highlight)
            {
                currHighlight.SetHighlight(false);
            }

            if (currHighlight != null && highlight)
            {
                currHighlight.SetHighlight(false);
            }

            if (highlight)
            {
                currHighlight = currHover;
                currHighlight.SetHighlight(true);
            }
        }

        public void Dragger(DraggableBehaviour assetCell, bool highlight)
        {
            currentlyDragging = highlight;
            if (highlight)
            {
                if (currHighlight != null)
                {
                    currHighlight.SetHighlight(false);
                }
                currHighlight = assetCell;
                currHighlight.SetHighlight(true);
            }
            else
            {
                if (assetCell == currHighlight)
                {
                    currHighlight = null;
                }
                assetCell.SetHighlight(false);
            }
        }

        private void OnDestroy()
        {
            EditorOp.Unregister(this);
        }
    }
}