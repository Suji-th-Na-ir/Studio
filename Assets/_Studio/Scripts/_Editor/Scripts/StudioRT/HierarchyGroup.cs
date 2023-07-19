using System;
using System.Collections;
using System.Collections.Generic;
using RuntimeInspectorNamespace;
using UnityEngine;

namespace Terra.Studio
{
    public class HierarchyGroup : MonoBehaviour
    {
        [SerializeField] private RuntimeHierarchy _hierarchy;

        private void Awake()
        {   
            HideHierarchy();
        }

        public void ShowHierarchy()
        {
            _hierarchy.gameObject.SetActive(true);
        }
        
        public void HideHierarchy()
        {
            _hierarchy.gameObject.SetActive(false);
        }
    }
}
