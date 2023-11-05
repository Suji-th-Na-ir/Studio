#if UNITY_WEBGL && !UNITY_EDITOR

using UnityEngine;
using Newtonsoft.Json;
using System.Collections;
using System.Runtime.InteropServices;

namespace Terra.Studio
{
    public class WebGLWrapper : MonoBehaviour
    {
        [DllImport("__Internal")]
        public static extern void GetUserData(string callbackTo);

        [DllImport("__Internal")]
        public static extern void GetProjectData(string callbackTo);

        [DllImport("__Internal")]
        public static extern void HideLoadingScreen();

        [DllImport("__Internal")]
        public static extern void PublishGame(string userName, string projectId, string callback);

        private void OnEnable()
        {
            SystemOp.Register(this);
        }

        private void OnDisable()
        {
            SystemOp.Unregister(this);
        }

        private void OnSubsystemLoaded()
        {
            HideLoadingScreen();
            if (SystemOp.Resolve<System>().CurrentStudioState == StudioState.Editor)
            {
                StartCoroutine(WaitAndFetchReferencesForScene());
            }
        }

        private IEnumerator WaitAndFetchReferencesForScene()
        {
            for (int i = 0; i < 10; i++)
            {
                yield return null;
            }
            EditorOp.Resolve<ToolbarView>().OnPublishRequested += () =>
            {
                var userName = SystemOp.Resolve<User>().UserId;
                var projectId = SystemOp.Resolve<User>().ProjectId;
                PublishGame(userName, projectId, nameof(TogglePublishButton));
            };
        }

        public void OnReactReady()
        {
            GetUserData(nameof(OnUserDataReceived));
            SystemOp.Resolve<System>().OnSubsystemLoaded += OnSubsystemLoaded;
        }

        public void OnUserDataReceived(string username)
        {
            SystemOp.Resolve<User>().UpdateUserName(username);
            GetProjectData(nameof(OnProjectDataReceived));
        }

        public void OnProjectDataReceived(string response)
        {
            var unpackedData = JsonConvert.DeserializeObject<LoginAPI.Data>(response);
            SystemOp.Resolve<User>()
                .UpdateUserId(unpackedData.UserId)
                .UpdateUserName(unpackedData.Username)
                .LogLoginEvent(unpackedData.IsAutoLoggedIn);
            SystemOp.Resolve<Flow>().OnProjectDetailsReceived(unpackedData.ProjectId, SystemOp.Resolve<System>().UpdateDefaultStudioState);
            SystemOp.Resolve<System>().LoadSceneData();
        }

        public void TogglePublishButton(int state)
        {
            var shouldEnable = state == 1;
            if (EditorOp.Resolve<ToolbarView>())
            {
                EditorOp.Resolve<ToolbarView>().SetPublishButtonActive(shouldEnable);
            }
        }
    }
}

#endif