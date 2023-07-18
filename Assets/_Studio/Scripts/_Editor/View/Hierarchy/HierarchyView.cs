using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PlayShifu.Terra;
using System.Collections;
using System.Collections.Generic;

namespace Terra.Studio
{
    public class HierarchyView : View
    {
        private const string HIERARCHY_CONTENT_LOC = "HierarchyContent";
        private const string RESOURCE_HIERARCHY_ITEM = "HierarchyItem";
        private const string NORMAL_UNSELECTED_COLOR = "#D0E4E5";
        private const string NORMAL_SELECTED_COLOR = "#79EF8A";
        private GameObject resHierarchyItem;
        private Transform hierarchyContentHolder;
        private Dictionary<string, GameObject> hierarchyItems;
        private string currentSelectedId;

        private void Awake()
        {
            Interop<EditorInterop>.Current.Register(this);
        }

        public override void Init()
        {
            hierarchyContentHolder = Helper.FindDeepChild(transform, HIERARCHY_CONTENT_LOC, true);
            hierarchyItems = new();
            Interop<EditorInterop>.Current.Resolve<SelectionManager>().onSelectionOccured += OnSelectedGameObject;
            resHierarchyItem = Resources.Load<GameObject>(RESOURCE_HIERARCHY_ITEM);
        }

        private void OnSelectedGameObject(StudioGameObject go)
        {
            if (go == null)
            {
                HighlightSelection(false, currentSelectedId);
            }
            else
            {
                if (!string.IsNullOrEmpty(currentSelectedId))
                {
                    HighlightSelection(false, currentSelectedId);
                }
                if (!hierarchyItems.ContainsKey(go.Id))
                {
                    var newItem = SpawnHierarchyItem(go);
                    hierarchyItems.Add(go.Id, newItem);
                }
                HighlightSelection(true, go.Id);
            }
        }

        private GameObject SpawnHierarchyItem(StudioGameObject go)
        {
            var spawnedItem = Instantiate(resHierarchyItem, hierarchyContentHolder);
            var text = spawnedItem.GetComponentInChildren<TextMeshProUGUI>();
            text.text = go.Ref.name;
            var btn = spawnedItem.GetComponentInChildren<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => { Interop<EditorInterop>.Current.Resolve<SelectionManager>().OnSelected(go); }); //Do Something
            return spawnedItem;
        }

        private void HighlightSelection(bool isSelected, string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return;
            }
            var item = hierarchyItems[id];
            var bg = item.GetComponentInChildren<Image>();
            if (!isSelected)
            {
                if (ColorUtility.TryParseHtmlString(NORMAL_UNSELECTED_COLOR, out Color color))
                {
                    bg.color = color;
                }
                currentSelectedId = null;
            }
            else
            {
                if (ColorUtility.TryParseHtmlString(NORMAL_SELECTED_COLOR, out Color color))
                {
                    bg.color = color;
                }
                currentSelectedId = id;
            }
        }

        public override void Draw()
        {
            //Do nothing for now
        }

        public override void Flush()
        {
            Destroy(this);
        }

        public override void Repaint()
        {
            //Do nothing for now
        }

        private void OnDestroy()
        {
            Interop<EditorInterop>.Current.Unregister(this);
        }
    }
}
