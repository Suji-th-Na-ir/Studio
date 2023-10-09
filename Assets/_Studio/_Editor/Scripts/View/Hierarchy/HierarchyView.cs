using UnityEngine;
using RuntimeInspectorNamespace;

namespace Terra.Studio
{
    public class HierarchyView : View
    {
        [SerializeField] private RuntimeHierarchy _hierarchy;

        private void Awake()
        {
            EditorOp.Register(this);
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
