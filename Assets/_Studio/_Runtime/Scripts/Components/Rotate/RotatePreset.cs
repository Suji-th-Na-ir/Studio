using UnityEngine;

namespace Terra.Studio
{
    [CreateAssetMenu(fileName = "Rotate_{TYPE}_PresetSO", menuName = "Terra/Presets/Rotate")]
    public class RotatePreset : ComponentPresetsSO<RotateComponentData>
    {
        [SerializeField]
        private RotateComponentData value;
        public override RotateComponentData Value => value;
    }
}
