using UnityEngine;

namespace Terra.Studio
{
    public class CrossSceneDataHolder
    {
        private string crossSharableData;

        public void Set(string data)
        {
            crossSharableData = data;
        }

        public string Get()
        {
            return crossSharableData;
        }
    }
}
