
using UnityEngine;
using RuntimeInspectorNamespace;
namespace Terra.Studio
{
    public class TooltipContent : MonoBehaviour, ITooltipContent
    {
        [SerializeField] string tooltipText;
        bool ITooltipContent.IsActive { get { return this && gameObject.activeSelf; } }
        string ITooltipContent.TooltipText => tooltipText.ToString();
        private void Start()
        {
           gameObject.AddComponent<TooltipArea>().Initialize(EditorOp.Resolve<ToolbarView>().TooltipListener, this);
        }
    }
}
