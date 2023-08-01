using System;
using System.Collections;
using System.Collections.Generic;
using RuntimeInspectorNamespace;
using UnityEngine;

namespace Terra.Studio
{
    public class HierarchyView : View
    {
        [SerializeField] private RuntimeHierarchy _hierarchy;

        private void Awake()
        {
            EditorOp.Register<HierarchyView>(this);
        }
        public override void Init()
        {
            _hierarchy.gameObject.SetActive(true);
        }

        public override void Draw()
        {

        }

        public override void Flush()
        {

        }

        public override void Repaint()
        {

        }

        private void OnDestroy()
        {
            EditorOp.Unregister<HierarchyView>(this);
        }
    }
}
