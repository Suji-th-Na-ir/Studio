using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PlayShifu.Terra;

namespace Terra.Studio
{
    public class TRSComponent
    {
        private const string POSITION_TEXT_LOC = "PText";
        private const string ROTATION_TEXT_LOC = "RText";
        private const string SCALE_TEXT_LOC = "SText";

        public static void Init(GameObject spawnedRef, Transform parentUI)
        {
            var positionText = Helper.FindDeepChild<TextMeshProUGUI>(parentUI, POSITION_TEXT_LOC);
            var rotationText = Helper.FindDeepChild<TextMeshProUGUI>(parentUI, ROTATION_TEXT_LOC);
            var scaleText = Helper.FindDeepChild<TextMeshProUGUI>(parentUI, SCALE_TEXT_LOC);
            var trsBinder = spawnedRef.AddComponent<TRSBinder>();
            trsBinder.onPositionChanged += (newPos) => { positionText.text = $"Position: {newPos.ToString()}"; };
            trsBinder.onRotationChanged += (newRot) => { rotationText.text = $"Rotation: {newRot.ToString()}"; };
            trsBinder.onScaleChanged += (newScale) => { scaleText.text = $"Scale: {newScale.ToString()}"; };
        }

        public static void Flush(GameObject spawnedRef)
        {
            if (spawnedRef != null && spawnedRef.TryGetComponent(out TRSBinder binder))
            {
                Object.Destroy(binder);
            }
        }
    }
}