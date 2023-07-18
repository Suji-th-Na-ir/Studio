using UnityEngine;

namespace Terra.Studio
{
    public class CrossSceneDataHolder : MonoBehaviour
    {
        private string crossSharableData;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void Init()
        {
            var go = new GameObject("Cross-Scene Data Holder", typeof(CrossSceneDataHolder));
        }

        private void Awake()
        {
            Interop<SystemInterop>.Current.Register(this);
        }

        public void Set(string data)
        {
            crossSharableData = data;
        }

        public string Get()
        {
            return crossSharableData;
        }

        private void OnDestroy()
        {
            Interop<SystemInterop>.Current.Unregister(this);
        }
    }
}
