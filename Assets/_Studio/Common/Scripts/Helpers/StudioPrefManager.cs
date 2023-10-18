using UnityEngine;

namespace Terra.Studio
{
    public static class StudioPrefManager
    {
        public static bool HasKeyInPrefs(this string key)
        {
            return PlayerPrefs.HasKey(key);
        }

        public static void SetPref(this string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
        }

        public static void SetPref(this string key, string value)
        {
            PlayerPrefs.SetString(key, value);
        }

        public static bool TryGetPrefString(this string key, out string value)
        {
            value = PlayerPrefs.GetString(key, string.Empty);
            return !string.IsNullOrEmpty(value);
        }
    }
}