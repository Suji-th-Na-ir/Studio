using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using PlayShifu.Terra;
using UnityEngine;
using UnityEngine.UI;

namespace Terra.Studio
{
    public class AssetsView : View
    {
        private const string ghostMatPath = "Editortime/Materials/Ghost";
        private const string assetsWindowButtonPath = "Editortime/Prefabs/AssetsWindow/AssetWindow_Button";
        private const string sampleJson = "Editortime/Prefabs/AssetsWindow/Sample";

        private const int PreCountPageApiHit = 3;
        private const int AssetsPerPage = 21;
        private const int SmoothNumberPerApi = 200;

        private bool useSampleJson = false;

        private AssetData[] _fullData;
        private ButtonScroll _scroll;
        private SearchBar _search;
        private AssetData[] _currentData;
        [SerializeField] private GridLayoutGroup m_refMainGridGroup;

        private DraggableBehaviour _currHighlight;
        private bool _currentlyDragging;

        private bool _searchInProgress;
        private string _currentSearchKey;
        private int _paginationNumber = 1;
        private int _searchPaginationNumber = 1;
        private int _lastWindowPage = 1;
        private int _totalMaxAssets;
        private int _currentMaxAssets;

        public void Awake()
        {
            EditorOp.Register(this);
        }

        public override void Init()
        {
            if (!useSampleJson)
            {
                var api = new AssetsWindowAPI(_paginationNumber, SmoothNumberPerApi);
                api.DoRequest((status, response) => { OnDataFetchSuccess(status, response, api.ResponseData.count); });
            }
            else
            {
                var text = EditorOp.Load<TextAsset>(sampleJson);
                var json = text.text;
                var temp = JsonConvert.DeserializeObject<APIResponse>(json);
                OnDataFetchSuccess(true, temp.data, 2000);
            }
        }

        private void OnDataFetchSuccess(bool success, string response, int count)
        {
            _totalMaxAssets = count;
            _currentMaxAssets = count;
            _fullData = ParseAssetDataFrom64Zipped(response);
            // _fullData = _fullData.Where(x => x.flags != null && x.flags.Any(y => y == "Premium")).ToArray();
            _currentData = _fullData;
            _scroll = GetComponentInChildren<ButtonScroll>();
            

            _scroll.Init(GetCurrentMaxPageNumbers(), PageChanged);
            _search = GetComponentInChildren<SearchBar>();
            _search.Init(OnSearch);
            MakeUIBetter();
        }

        private int GetCurrentMaxPageNumbers()
        {
            int bla;
            if (_searchInProgress)
            {
                bla = _currentMaxAssets / AssetsPerPage;
            }
            else
            {
                bla = _totalMaxAssets/ AssetsPerPage;
            }

            
            if (bla % AssetsPerPage != 0)
            {
                bla++;
            }
            return bla;
        }

        private void MakeUIBetter()
        {
            int columnCount = m_refMainGridGroup.constraintCount;
            // Debug.Log("m_refMainGridGroup : " + m_refMainGridGroup.gameObject.GetComponent<RectTransform>().rect);
            float ySize = m_refMainGridGroup.gameObject.GetComponent<RectTransform>().rect.height /
                          (AssetsPerPage / columnCount);
            m_refMainGridGroup.cellSize =
                new Vector2(m_refMainGridGroup.cellSize.x, ySize - m_refMainGridGroup.spacing.y);
        }

        private void OnSearch(string searchString)
        {
            _searchPaginationNumber = 1;

            if (!string.IsNullOrEmpty(searchString))
            {
                SetSearchInProgress(true, searchString);
                ClearDataIfAny();
                // var searchAPI = new SearchAPI(searchString, _searchPaginationNumber, SmoothNumberPerApi);
                // searchAPI.DoRequest((x, y) => { SearchCompleted(searchString, x, y, searchAPI.ResponseData.count); });
                StartCoroutine(LocalSearchRoutine(searchString));
            }
            else
            {
                _currentData = _fullData;
                _currentMaxAssets = _totalMaxAssets;
                SetSearchInProgress(false, null);
                _scroll.Init(_currentData.Length / AssetsPerPage, PageChanged);
            }
        }

        private void SetSearchInProgress(bool isSearching, string key)
        {
            _searchInProgress = isSearching;
            _currentSearchKey = key;
        }

        private void SearchCompleted(string searchResultFor, bool success, string response, int maxCount)
        {
            if (!success)
            {
                Debug.LogError($"Search failed status != 200");
                // return;
            }

            if (_currentSearchKey != searchResultFor)
            {
                Debug.Log($"Older search result");
                return;
            }

            var queriedData = ParseAssetDataFrom64Zipped(response);
            ClearDataIfAny();
            _currentData = queriedData;
            _currentMaxAssets = maxCount;
            _scroll.Init(GetCurrentMaxPageNumbers(), PageChanged);
        }

        private IEnumerator LocalSearchRoutine(string query)
        {
            List<AssetData> queriedAssetData = new();
            for (var i = 0; i < _fullData.Length; i++)
            {
                var d = _fullData[i];
                var pass = d.display_name.Contains(query) || d.unique_name.Contains(query) ||
                           d.category.Contains(query);
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
            _currentMaxAssets = _currentData.Length;
            _scroll.Init(GetCurrentMaxPageNumbers(), PageChanged);
        }

        private void PageChanged(int obj)
        {
            if (_lastWindowPage != -1)
            {
                if (_lastWindowPage < obj)
                {
                    CheckIfMoreAssetsAreRequired();
                }
            }

            _lastWindowPage = obj;

            ClearDataIfAny();

            var buttonPrefab = EditorOp.Load<GameObject>(assetsWindowButtonPath);
            var ghostMat = EditorOp.Load<Material>(ghostMatPath);
            var temp = GetComponentInChildren<GridLayoutGroup>();
            var x = Mathf.Max(obj - 1, 0);
            int counter = 0;
            for (var i = AssetsPerPage * x; i < _currentData.Length && counter < AssetsPerPage; i++)
            {
                counter++;
                var data = _currentData[i];
                var button = Instantiate(buttonPrefab, temp.transform);
                button.GetComponent<DraggableBehaviour>().Init(this, data, ghostMat);
            }

            MakeUIBetter();
        }

        private void CheckIfMoreAssetsAreRequired()
        {
            return;
            if (_lastWindowPage * AssetsPerPage + AssetsPerPage * PreCountPageApiHit > _currentData.Length)
            {
                var max = _searchInProgress ? _currentMaxAssets : _totalMaxAssets;
                if (_lastWindowPage * AssetsPerPage >= max)
                {
                    Debug.LogError($"no more assets.");
                    return;
                }
                
                StudioAPI api = default;
                if (_searchInProgress)
                {
                    api = new SearchAPI(_currentSearchKey, ++_searchPaginationNumber, SmoothNumberPerApi);
                }
                else
                {
                    api = new AssetsWindowAPI(++_paginationNumber, SmoothNumberPerApi);
                }
                    
                api.DoRequest((success, response) =>
                {
                    if (!success)
                    {
                        Debug.LogError($"API FAILED.");
                        return;
                    }
                    var dataToBeAdded = ParseAssetDataFrom64Zipped(response);
                    foreach (var assetData in dataToBeAdded)
                    {
                        Debug.Log($"ass => {assetData.unique_name}");
                    }
                    if (api.GetType() == typeof(AssetsWindowAPI))
                    {
                        _currentData = _currentData.Concat(dataToBeAdded).ToArray();
                        _fullData = _currentData;
                        _scroll.Init(GetCurrentMaxPageNumbers(), PageChanged, true);
                    }
                    else if (api.GetType() == typeof(SearchAPI) && _currentSearchKey == ((SearchAPI)api).SearchKey)
                    {
                        _currentData = _currentData.Concat(dataToBeAdded).ToArray();
                        _scroll.Init(GetCurrentMaxPageNumbers(), PageChanged, true);
                    }
                });
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

        public void Selected(DraggableBehaviour currHover, bool highlight)
        {
            if (_currentlyDragging)
                return;

            if (_currHighlight == currHover && !highlight)
            {
                _currHighlight.SetHighlight(false);
            }

            if (_currHighlight != null && highlight)
            {
                _currHighlight.SetHighlight(false);
            }

            if (highlight)
            {
                _currHighlight = currHover;
                _currHighlight.SetHighlight(true);
            }
        }

        public void Dragger(DraggableBehaviour assetCell, bool highlight)
        {
            _currentlyDragging = highlight;
            if (highlight)
            {
                if (_currHighlight != null)
                {
                    _currHighlight.SetHighlight(false);
                }

                _currHighlight = assetCell;
                _currHighlight.SetHighlight(true);
            }
            else
            {
                if (assetCell == _currHighlight)
                {
                    _currHighlight = null;
                }

                assetCell.SetHighlight(false);
            }
        }

        private void OnDestroy()
        {
            EditorOp.Unregister(this);
        }

        private static AssetData[] ParseAssetDataFrom64Zipped(string base64String)
        {
            var json = Helper.UnzipBase64String(base64String);
            return JsonConvert.DeserializeObject<AssetData[]>(json);
        }
    }
}