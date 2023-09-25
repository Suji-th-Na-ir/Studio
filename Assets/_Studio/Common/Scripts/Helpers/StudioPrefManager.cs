using UnityEngine;

namespace Terra.Studio
{
    public static class StudioPrefManager
    {
        public static bool HasKeyInPrefs(this string key)
        {
            return PlayerPrefs.HasKey(key);
        }

        public static void SetInt(this string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
        }
    }
}