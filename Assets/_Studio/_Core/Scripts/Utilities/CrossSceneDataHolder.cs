using System.Collections.Generic;

namespace Terra.Studio
{
    public class CrossSceneDataHolder
    {
        private string crossSharableData;
        private Dictionary<string, object> keyToSharedData = new();

        private List<string> broadcastStrings = new List<string>() { "None", "Game Win", "Game Lose" };
        public List<string> BroadcastStrings { get { return broadcastStrings; } }

        public void Dispose()
        {
            broadcastStrings.Clear();
        }

        public void UpdateNewBroadcast(string newValue)
        {
            if (string.IsNullOrEmpty(newValue))
                return;

            if (!broadcastStrings.Contains(newValue))
                broadcastStrings.Add(newValue);
        }

        public void Set(string data)
        {
            crossSharableData = data;
        }

        public void Set(string key, object data)
        {
            if (!keyToSharedData.ContainsKey(key))
            {
                keyToSharedData.Add(key, data);
            }
            else
            {
                keyToSharedData[key] = data;
            }
        }

        public string Get()
        {
            return crossSharableData;
        }

        public bool Get(string key, out object data)
        {
            data = null;
            if (!keyToSharedData.ContainsKey(key))
            {
                return false;
            }
            data = keyToSharedData[key];
            keyToSharedData.Remove(key);
            return data != null;
        }
    }
}