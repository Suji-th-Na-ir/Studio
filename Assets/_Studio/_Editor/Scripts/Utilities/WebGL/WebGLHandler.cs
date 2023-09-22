#if UNITY_WEBGL && !UNITY_EDITOR
#define ENABLE_WEBGL_HANDLER
using System.IO;
using UnityEngine;
#endif

using System;
using UnityEngine.Scripting;

namespace Terra.Studio
{
    [Preserve]
    public class WebGLHandler : IDisposable
    {
#if ENABLE_WEBGL_HANDLER
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void WebGLHandlerInit()
        {
            SystemOp.Register(new WebGLHandler());
            SystemOp.Register(new IndexedDBManager());
        }
#endif

        public void DoesStoreHasData(string fullFilePath, Action<bool> callback)
        {
#if ENABLE_WEBGL_HANDLER
            var fileName = Path.GetFileNameWithoutExtension(fullFilePath);
            SystemOp.Resolve<IndexedDBManager>().GetDataFromIndexedDB(fileName, (res) =>
            {
                var isDataPresent = !string.IsNullOrEmpty(res);
                callback?.Invoke(isDataPresent);
            });
#endif
        }

        public void WriteDataIntoStore(string data, string fullFilePath)
        {
#if ENABLE_WEBGL_HANDLER
            var fileName = Path.GetFileNameWithoutExtension(fullFilePath);
            SystemOp.Resolve<IndexedDBManager>().SaveDataToIndexedDB(fileName, data, null);
#endif
        }

        public void ReadDataFromStore(string fullFilePath, Action<string> callback)
        {
#if ENABLE_WEBGL_HANDLER
            var fileName = Path.GetFileNameWithoutExtension(fullFilePath);
            SystemOp.Resolve<IndexedDBManager>().GetDataFromIndexedDB(fileName, callback);
#endif
        }

        public void Dispose()
        {
#if ENABLE_WEBGL_HANDLER
            SystemOp.Unregister<IndexedDBManager>();
#endif
        }

    }
}
