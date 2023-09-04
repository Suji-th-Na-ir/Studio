using UnityEngine;

namespace Terra.Studio
{
    [CreateAssetMenu(fileName = "Translate_{TYPE}_PresetSO", menuName = "Terra/Presets/Translate")]
    public class TranslatePreset : ComponentPresetsSO<TranslateComponentData>
    {
        [SerializeField]
        private TranslateComponentData value;
        public override TranslateComponentData Value => value;
    }
}
