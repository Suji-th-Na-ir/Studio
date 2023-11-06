using UnityEngine;

namespace Terra.Studio
{
    [CreateAssetMenu(fileName = "SystemSettings", menuName = "Terra/SystemSettings")]
    public class SystemConfigurationSO : ScriptableObject
    {
        [SerializeField] private StudioState defaultStudioState;
        [SerializeField] private string editorSceneName;
        [SerializeField] private string runtimeSceneName;
        [SerializeField] private bool pickupSavedData;
        [SerializeField] private TextAsset SceneData;

        public StudioState DefaultStudioState { get { return defaultStudioState; } }
        public string EditorSceneName { get { return editorSceneName; } }
        public string RuntimeSceneName { get { return runtimeSceneName; } }
        public TextAsset SceneDataToLoad
        {
            get { return SceneData; }
#if UNITY_EDITOR
            set { SceneData = value; }
#endif
        }
        public bool PickupSavedData
        {
            get { return pickupSavedData; }
#if UNITY_EDITOR
            set { pickupSavedData = value; }
#endif
        }
        public bool ServeFromCloud
        {
            get
            {
#if UNITY_EDITOR
                return serveFromCloud;
#else
                return true;
#endif
            }
        }

#if UNITY_EDITOR
        [Space(10), Header("Editor only")]
        [SerializeField] private bool loadDefaultSceneOnPlay;
        [SerializeField] private bool serveFromCloud;
        public bool LoadDefaultSceneOnPlay { get { return loadDefaultSceneOnPlay; } }
#endif
    }
}
