using System.Collections.Generic;
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
        [SerializeField] private List<TextAsset> SceneData;
        [SerializeField] private TextAsset selectedSceneData;

        public StudioState DefaultStudioState { get { return defaultStudioState; } }
        public string EditorSceneName { get { return editorSceneName; } }
        public string RuntimeSceneName { get { return runtimeSceneName; } }
        public TextAsset SceneDataToLoad
        {
            get { return selectedSceneData; }
            set { selectedSceneData = value; }
        }

        public List<TextAsset> GetAllSceneDatas()
        {
            return SceneData;
        }
        
        
        public bool PickupSavedData
        {
            get { return pickupSavedData; }
#if UNITY_EDITOR
            set { pickupSavedData = value; }
#endif
        }

#if UNITY_EDITOR
        [Space(10), Header("Editor only")]
        [SerializeField] private bool loadDefaultSceneOnPlay;
        public bool LoadDefaultSceneOnPlay { get { return loadDefaultSceneOnPlay; } }
#endif
    }
}
