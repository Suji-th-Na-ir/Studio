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
        private static Action<bool> renameCallback;

        [DllImport("__Internal")]
        public static extern void OpenIndexedDB();

        [DllImport("__Internal")]
        public static extern void SaveData(string key, string value, Action<int> callback);

        [DllImport("__Internal")]
        public static extern void GetData(string key, Action<string> callback);

        [DllImport("__Internal")]
        public static extern void RemoveData(string key, Action<int> callback);

        [DllImport("__Internal")]
        public static extern void RenameKey(string oldKey, string newKey, Action<int> callback);

        public IndexedDBManager()
        {
            OpenIndexedDB();
        }

        public void SaveDataToIndexedDB(string key, string value, Action<bool> callback)
        {
            saveCallback = null;
            saveCallback = callback;
            SaveData(key, value, SaveDataCallback);
        }

        public void GetDataFromIndexedDB(string key, Action<string> callback)
        {
            onFetchedData = null;
            onFetchedData = callback;
            GetData(key, GetDataCallback);
        }

        public void RemoveDataFromIndexedDB(string key, Action<bool> callback)
        {
            removeCallback = null;
            removeCallback = callback;
            RemoveData(key, RemoveDataCallback);
        }

        public void RenameKeyFromStore(string lastKey, string newKey, Action<bool> callback)
        {
            renameCallback = null;
            renameCallback = callback;
            RenameKey(lastKey, newKey, RenameKeyCallback);
        }

        [MonoPInvokeCallback(typeof(Action<int>))]
        public static void SaveDataCallback(int value)
        {
            var res = value == 1;
            saveCallback?.Invoke(res);
        }

        [MonoPInvokeCallback(typeof(Action<string>))]
        public static void GetDataCallback(string value)
        {
            onFetchedData?.Invoke(value);
        }

        [MonoPInvokeCallback(typeof(Action<int>))]
        public static void RemoveDataCallback(int value)
        {
            var res = value == 1;
            removeCallback?.Invoke(res);
        }

        [MonoPInvokeCallback(typeof(Action<int>))]
        public static void RenameKeyCallback(int value)
        {
            var res = value == 1;
            renameCallback?.Invoke(res);
        }
    }
}
#endif