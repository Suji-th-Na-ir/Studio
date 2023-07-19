using System;
using System.Collections;
using System.Collections.Generic;
using RuntimeCommon;
using UnityEngine;

namespace Terra.Studio
{
    public class RuntimeEditor : MonoSingleton<RuntimeEditor>
    {
        [SerializeField] private InspectorGroup _inspectorGroup;
        [SerializeField] private HierarchyGroup _hierarchyGroup;
        [SerializeField] private SelectionHandler _selectionHandler;

        private void Start()
        {
            _inspectorGroup.ShowHierarchy();
            _hierarchyGroup.ShowHierarchy();
        }
    }
}
