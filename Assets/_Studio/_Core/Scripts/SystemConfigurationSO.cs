using UnityEngine;

namespace Terra.Studio
{
    [CreateAssetMenu(fileName = "SystemSettings", menuName = "Terra/SystemSettings")]
    public class SystemConfigurationSO : ScriptableObject
    {
        [SerializeField] private StudioState defaultStudioState;
        [SerializeField] private string editorSceneName;
        [SerializeField] private string runtimeSceneName;

        public StudioState DefaultStudioState { get { return defaultStudioState; } }
        public string EditorSceneName { get { return editorSceneName; } }
        public string RuntimeSceneName { get { return runtimeSceneName; } }

#if UNITY_EDITOR
        [Space(10), Header("Editor only")]
        [SerializeField] private TextAsset SceneData;
        public TextAsset SceneDataToLoad { get { return SceneData; } }
        [SerializeField] private bool pickupSavedData;
        public bool PickupSavedData { get { return pickupSavedData; } }
        [SerializeField] private bool loadDefaultSceneOnPlay;
        public bool LoadDefaultSceneOnPlay { get { return loadDefaultSceneOnPlay; } }
#endif
    }
}
