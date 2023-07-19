using System.Collections;
using System.Collections.Generic;
using RuntimeInspectorNamespace;
using UnityEngine;

namespace Terra.Studio
{
    public class InspectorGroup : MonoBehaviour
    {
        [SerializeField] private RuntimeInspector _inspector;

        private void Awake()
        {   
            HideInspector();
        }

        public void ShowHierarchy()
        {
            _inspector.gameObject.SetActive(true);
        }
        
        public void HideInspector()
        {
            _inspector.gameObject.SetActive(false);
        }
    }
}
