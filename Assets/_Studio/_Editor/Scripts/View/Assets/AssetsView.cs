using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using PlayShifu.Terra;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Terra.Studio
{
    public class AssetsView : View
    {
        private enum SearchType
        {
            Search,
            Filter,
            FilteredSearch
        }
        
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

        [SerializeField] private TMP_Dropdown catDropDown;
        [SerializeField] private TMP_Dropdown subCatDropDown;
        [SerializeField] private TMP_Dropdown themeDropDown;
        [SerializeField] private Toggle downloadToggle;
        [SerializeField] private Button clearFiltersButton;

        private DraggableBehaviour _currHighlight;
        private bool _currentlyDragging;

        private SearchType _searchTypeToUse;
        private int _totalMaxAssets;
        private int _currentMaxAssets;

        private AssetsCategoriesData _assetsCategories;

        private string _lastSearchedString;
        private string currCategory;
        private List<TMP_Dropdown.OptionData> catDatas;
        private List<TMP_Dropdown.OptionData> subCatDatas = new();
        private List<TMP_Dropdown.OptionData> themeDatas = new();
        private string _activeCategory;
        private string _activeSubCategory;
        private string _activeTheme;
        private bool _downloadCheck;
        private const string NONE = "None";
        private CacheValidator _cacheValidator;
        
        public void Awake()
        {
            EditorOp.Register(this);
        }

        public override void Init()
        {
            _cacheValidator ??= SystemOp.Resolve<CacheValidator>();
            _scroll = GetComponentInChildren<ButtonScroll>();
            _search = GetComponentInChildren<SearchBar>();
            StartCoroutine(SendApis());
        }

        private IEnumerator SendApis()
        {
            var catApi = new AssetsCategoryAPI();
            catApi.DoRequest(OnCategoryDataFetchCompleted);
            if (!useSampleJson)
            {
                var assetsApi = new AssetsWindowAPI(1, SmoothNumberPerApi);
                assetsApi.DoRequest((status, response) =>
                {
                    OnAssetsDataFetchCompleted(status, response, assetsApi.ResponseData.count);
                });
            }
            else
            {
                var text = EditorOp.Load<TextAsset>(sampleJson);
                var json = text.text;
                var temp = JsonConvert.DeserializeObject<APIResponse>(json);
                OnAssetsDataFetchCompleted(true, temp.data, 2000);
            }

            yield return new WaitUntil(() => _assetJsonDownloaded && _categoryJsonDownloaded);
            UpdateThemeNames(false);
            UpdateCategoryNames(false);
            CategoryChanged(0);
            _search.Init((x) =>
            {
                OnSearch(x,SearchType.FilteredSearch);
            });
        }

        private bool _assetJsonDownloaded;
        private bool _categoryJsonDownloaded;
        
        private void OnAssetsDataFetchCompleted(bool success, string response, int count)
        {
            _assetJsonDownloaded = true;
            _totalMaxAssets = count;
            _currentMaxAssets = count;
            _fullData = ParseAssetDataFrom64Zipped(response);
            // _fullData = _fullData.Where(x => x.flags != null && x.flags.Any(y => y == "Premium")).ToArray();
            _currentData = _fullData;
           
        }
        private void OnCategoryDataFetchCompleted(bool success, string response)
        {
            _categoryJsonDownloaded = true;
            _assetsCategories = JsonConvert.DeserializeObject<AssetsCategoriesData>(response);
            
            catDropDown.onValueChanged.AddListener(CategoryChanged);
            subCatDropDown.onValueChanged.AddListener(SubCatChanged);
            themeDropDown.onValueChanged.AddListener(ThemeChanged);
            downloadToggle.isOn = false;
            downloadToggle.onValueChanged.AddListener(DownloadOnlyToggleChanged);
            clearFiltersButton.onClick.AddListener(ClearFiltersClicked);
        }
        
        private void PageChanged(int obj)
        {
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
        
        private int GetCurrentMaxPageNumbers()
        {
            var maxPageCount = _currentMaxAssets / AssetsPerPage;
            if (maxPageCount % AssetsPerPage != 0)
            {
                maxPageCount++;
            }
            return maxPageCount;
        }
        
        private void SetSearchTypeInProgress(SearchType searchType)
        {
            _searchTypeToUse = searchType;
        }

        
        #region Local String Search

        private void OnSearch(string searchString, SearchType searchType)
        {
            _lastSearchedString = searchString;
            if (!string.IsNullOrEmpty(searchString))
            {
                SetSearchTypeInProgress(searchType);
                ClearDataIfAny();
                StartCoroutine(LocalSearchRoutine(searchString));
            }
            else
            {
                _currentData = _fullData;
                _currentMaxAssets = _totalMaxAssets;
                SetSearchTypeInProgress(SearchType.FilteredSearch);
                StartCoroutine(LocalSearchRoutine(null));
            }
        }

        private IEnumerator LocalSearchRoutine(string query)
        {
            List<AssetData> queriedAssetData = new();
            var willSearch = !string.IsNullOrEmpty(query);
            for (var i = 0; i < _fullData.Length; i++)
            {
                var d = _fullData[i];
                var pass = true;
                if (_downloadCheck)
                {
                    pass = _cacheValidator.IsFileInCache($"{d.unique_name}.gltf", out _);
                }
                if (_searchTypeToUse is SearchType.Filter or SearchType.FilteredSearch)
                {
                    if (_activeCategory != NONE)
                    {
                        pass = d.category.Contains(_activeCategory);
                        if (pass && _activeSubCategory != NONE && d.sub_category != null)
                        {
                            pass = d.sub_category.Contains(_activeSubCategory);
                        }
                    }
                    else if(_activeTheme != NONE)
                    {
                        pass = d.theme == _activeTheme;
                    }
                }
                
                if (pass && willSearch && _searchTypeToUse is SearchType.Search or SearchType.FilteredSearch)
                {
                    pass = d.display_name.Contains(query) || d.unique_name.Contains(query) || d.category.Contains(query);
                }
                
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

        #endregion

        #region Local Filter Search

        private void ClearFiltersClicked()
        {
            DownloadOnlyToggleChanged(false);
        }
        private void DownloadOnlyToggleChanged(bool newVal)
        {
            downloadToggle.isOn = newVal;
            _downloadCheck = newVal;
            UpdateThemeNames(newVal);
            ThemeChanged(0);
        }
        private void ThemeChanged(int idx)
        {
            themeDropDown.value = idx;
            _activeTheme = themeDatas[idx].text;

            if (idx != 0)
            {
                UpdateCategoryNames(true);
                CategoryChanged(0);
            }
            else
            {
                UpdateCategoryNames(false);
                CategoryChanged(0);
            }
        }
        
        private void CategoryChanged(int idx)
        {
            catDropDown.value = idx;
            _activeCategory = catDatas[idx].text;
            UpdateSubCategories();
        }

        private void SubCatChanged(int idx)
        {
            subCatDropDown.value = idx;
            _activeSubCategory = subCatDatas[idx].text;
            OnSearch(_lastSearchedString, SearchType.FilteredSearch);
        }

        private void UpdateThemeNames(bool downloadOnlyActive)
        {
            themeDatas ??= new List<TMP_Dropdown.OptionData>(_assetsCategories.themes.Length+1);
            themeDatas.Clear();
            themeDatas.Add(new TMP_Dropdown.OptionData(NONE));
            if (!downloadOnlyActive)
            {
                foreach (var themeName in _assetsCategories.themes)
                {
                    themeDatas.Add(new TMP_Dropdown.OptionData(themeName));
                }
            }
            themeDropDown.options = themeDatas;
        }
        private void UpdateCategoryNames(bool isThemesActive)
        {
            if (!isThemesActive && !_downloadCheck)
            {
                catDatas ??= new List<TMP_Dropdown.OptionData>(_assetsCategories.categoriesData.Length + 1);
                catDatas.Clear();
                catDatas.Add(new TMP_Dropdown.OptionData(NONE));
                foreach (var categoryData in _assetsCategories.categoriesData)
                {
                    catDatas.Add(new TMP_Dropdown.OptionData(categoryData.category));
                }
            }
            else
            {
                catDatas ??= new List<TMP_Dropdown.OptionData>(_assetsCategories.themes.Length + 1);
                catDatas.Clear();
                catDatas.Add(new TMP_Dropdown.OptionData(NONE));
            }
            catDropDown.options = catDatas;
        }

        private void UpdateSubCategories()
        {
            var x = _assetsCategories.categoriesData.FirstOrDefault(x => x.category == _activeCategory);
            if (x == null)
            {
                subCatDatas.Clear();
                subCatDatas.Add(new TMP_Dropdown.OptionData(NONE));
                subCatDropDown.options = subCatDatas;
                SubCatChanged(0);
                return;
            }

            subCatDatas.Clear();
            subCatDatas.Add(new TMP_Dropdown.OptionData(NONE));
            foreach (var subCatName in x.subCategory)
            {
                subCatDatas.Add(new TMP_Dropdown.OptionData(subCatName));
            }

            subCatDropDown.options = subCatDatas;
            SubCatChanged(0);
        }

        #endregion
        
        #region Cells

        private void ClearDataIfAny()
        {
            var temp = GetComponentInChildren<GridLayoutGroup>().transform;
            foreach (Transform o in temp)
            {
                Destroy(o.gameObject);
            }
        }

        private void MakeUIBetter()
        {
            int columnCount = m_refMainGridGroup.constraintCount;
            // Debug.Log("m_refMainGridGroup : " + m_refMainGridGroup.gameObject.GetComponent<RectTransform>().rect);
            float ySize = m_refMainGridGroup.gameObject.GetComponent<RectTransform>().rect.height / ((float)AssetsPerPage / columnCount);
            m_refMainGridGroup.cellSize =
                new Vector2(m_refMainGridGroup.cellSize.x, ySize - m_refMainGridGroup.spacing.y);
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

        #endregion

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