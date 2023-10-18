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
        private static Action<bool> removeCallback;
        private static Action<string> onFetchedData;

        [DllImport("__Internal")]
        public static extern void OpenIndexedDB();

        [DllImport("__Internal")]
        public static extern void SaveData(string key, string value, Action<int> callback);

        [DllImport("__Internal")]
        public static extern void GetData(string key, Action<string> callback);

        [DllImport("__Internal")]
        public static extern void RemoveData(string key, Action<int> callback);

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
            onFetchedData = callback;
            GetData(key, GetDataCallback);
        }

        public void RemoveDataFromIndexedDB(string key, Action<bool> callback)
        {
            removeCallback = callback;
            RemoveData(key, RemoveDataCallback);
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

        [MonoPInvokeCallback(typeof(Action<int>))]
        public static void RemoveDataCallback(int value)
        {
            var res = value == 1;
            removeCallback?.Invoke(res);
            removeCallback = null;
        }
    }
}
#endif