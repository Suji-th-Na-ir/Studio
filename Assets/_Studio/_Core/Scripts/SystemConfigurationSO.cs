using UnityEngine;

namespace Terra.Studio
{
    [CreateAssetMenu(fileName = "SystemSettings", menuName = "Terra/Studio/SystemSettings")]
    public class SystemConfigurationSO : ScriptableObject
    {
        [SerializeField] private StudioState defaultStudioState;
        [SerializeField] private string editorSceneName;
        [SerializeField] private string runtimeSceneName;
#if UNITY_EDITOR
        [Space(10), Header("Editor only")]
        [SerializeField] private bool pickupSavedData;
        public bool PickupSavedData { get { return pickupSavedData; } }
        [SerializeField] private bool loadDefaultSceneOnPlay;
        public bool LoadDefaultSceneOnPlay { get { return loadDefaultSceneOnPlay; } }
#endif

        public StudioState DefaultStudioState { get { return defaultStudioState; } }
        public string EditorSceneName { get { return editorSceneName; } }
        public string RuntimeSceneName { get { return runtimeSceneName; } }
    }
}
