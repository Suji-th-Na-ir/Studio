using UnityEngine;
using UnityEngine.UI;

namespace Terra.Studio
{
    public class LeftToolbar : View
    {
        [SerializeField] private bool showAssetsWindowByDefault;
        [SerializeField] private CanvasGroup assetsWindowGroup;
        [SerializeField] private CanvasGroup hierarchyGroup;
        
        [SerializeField] private Button assetsWindowButton;
        [SerializeField] private Button hierarchyWindowButton;
        
        private CrossSceneDataHolder _crossSceneDataHolder;
        private const string LastActiveSideView = "LastActiveSideView";
        private void Awake()
        {
            EditorOp.Register(this);
        }

        public override void Init()
        {
            _crossSceneDataHolder = SystemOp.Resolve<CrossSceneDataHolder>();
            
            if (_crossSceneDataHolder.Get(LastActiveSideView, out var data))
            {
                showAssetsWindowByDefault = (bool)data;
            }


            if (showAssetsWindowByDefault)
            {
                assetsWindowButton.onClick.Invoke();
            }
            else
            {
                hierarchyWindowButton.onClick.Invoke();
            }
        }
        
        public void AssetsWindowClicked()
        {
            UpdateLastActiveToAssetsWindow(true);
            EnableGroup(assetsWindowGroup);
            DisableGroup(hierarchyGroup);
        }

        public void HierarchyWindowClicked()
        {
            UpdateLastActiveToAssetsWindow(false);
            EnableGroup(hierarchyGroup);
            DisableGroup(assetsWindowGroup);
        }
        
        private void UpdateLastActiveToAssetsWindow(bool isAssetsWindow)
        {
            _crossSceneDataHolder.Set(LastActiveSideView, isAssetsWindow);
        }

        private void EnableGroup(CanvasGroup group)
        {
            group.alpha = 1;
            group.interactable = true;
            group.blocksRaycasts = true;
        }
        
        private void DisableGroup(CanvasGroup group)
        {
            group.alpha = 0;
            group.interactable = false;
            group.blocksRaycasts = false;
        }

        private void OnDestroy()
        {
            EditorOp.Unregister(this);
        }
    }
}