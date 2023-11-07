using System;
using UnityEngine;
using UnityEngine.UI;

namespace Terra.Studio
{
    public class LeftToolbar : MonoBehaviour
    {
        [SerializeField] private CanvasGroup defaultGroup;
        [SerializeField] private CanvasGroup assetsWindowGroup;
        [SerializeField] private CanvasGroup hierarchyGroup;

        private void Start()
        {
            var nonDefaultGroup = defaultGroup == assetsWindowGroup ? hierarchyGroup : assetsWindowGroup;
            EnableGroup(defaultGroup);
            DisableGroup(nonDefaultGroup);
        }

        public void AssetsWindowClicked()
        {
            EnableGroup(assetsWindowGroup);
            DisableGroup(hierarchyGroup);
        }

        public void HierarchyWindowClicked()
        {
            EnableGroup(hierarchyGroup);
            DisableGroup(assetsWindowGroup);
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
    }
}