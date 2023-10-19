using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

namespace Terra.Studio
{
    public class NetworkManager : MonoBehaviour
    {
        private void Awake()
        {
            SystemOp.Register(this);
        }

        private void OnDestroy()
        {
            SystemOp.Unregister(this);
        }

        public void DoRequest(StudioAPI api, Action<bool, string> callback)
        {
            if (api.RequestType == RequestType.Get)
            {
                StartCoroutine(Get(api, callback));
            }
            else
            {
                StartCoroutine(Post(api, callback));
            }
        }

        private IEnumerator Get(StudioAPI api, Action<bool, string> callback)
        {
            using var request = UnityWebRequest.Get(api.URL);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                callback?.Invoke(true, request.downloadHandler.text);
            }
            else
            {
                Debug.Log($"API failed. Result is: {request.result} | Response is: {request.responseCode}");
                callback?.Invoke(false, request.downloadHandler?.text);
            }
        }

        private IEnumerator Post(StudioAPI api, Action<bool, string> callback)
        {
            var form = new WWWForm();
            foreach (var data in api.FormData)
            {
                form.AddField(data.Key, data.Value);
            }
            using var request = UnityWebRequest.Post(api.URL, form);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Got response: {request.downloadHandler.text}");
                callback?.Invoke(true, request.downloadHandler.text);
            }
            else
            {
                Debug.Log($"API failed. Result is: {request.result} | Response is: {request.responseCode}");
                callback?.Invoke(false, null);
            }
        }
    }
}
