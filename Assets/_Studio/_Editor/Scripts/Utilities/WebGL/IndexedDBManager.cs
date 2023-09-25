#if UNITY_WEBGL && !UNITY_EDITOR
using AOT;
using System;
using UnityEngine.Scripting;
using System.Runtime.InteropServices;

namespace Terra.Studio
{
    [Preserve]
    public class IndexedDBManager
    {
        private static Action<bool> saveCallback;
        private static Action<string> onFetchedData;

        [DllImport("__Internal")]
        public static extern void OpenIndexedDB();

        [DllImport("__Internal")]
        public static extern void SaveData(string key, string value, Action<int> callback);

        [DllImport("__Internal")]
        public static extern void GetData(string key, Action<string> callback);

        public IndexedDBManager()
        {
            OpenIndexedDB();
        }

        public void SaveDataToIndexedDB(string key, string value, Action<bool> callback)
        {
            saveCallback = callback;
            SaveData(key, value, SaveDataCallback);
        }

        public void GetDataFromIndexedDB(string key, Action<string> callback)
        {
            GetData(key, GetDataCallback);
            onFetchedData = callback;
        }

        [MonoPInvokeCallback(typeof(Action<int>))]
        public static void SaveDataCallback(int value)
        {
            var res = value == 1;
            saveCallback?.Invoke(res);
            saveCallback = null;
        }

        [MonoPInvokeCallback(typeof(Action<string>))]
        public static void GetDataCallback(string value)
        {
            onFetchedData?.Invoke(value);
            onFetchedData = null;
        }
    }
}
#endif