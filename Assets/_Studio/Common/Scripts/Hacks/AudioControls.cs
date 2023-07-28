#if UNITY_EDITOR
#define EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace Terra.Studio
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioControls : MonoBehaviour
    {
        private AudioSource audioSource;

        private void Start() => audioSource = GetComponent<AudioSource>();

        public void Play()
        {
            audioSource.Play();
        }
    }

#if EDITOR
    [CustomEditor(typeof(AudioControls))]
    public class AudioControlsEditor : Editor
    {
        private AudioControls controls;

        private void OnEnable() => controls = (AudioControls)target;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Play"))
            {
                controls.Play();
            }
        }
    }
#endif
}
