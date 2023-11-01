#if UNITY_WEBGL && !UNITY_EDITOR

using UnityEngine;
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
        public static extern void PublishGame(string userName, string projectId);

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
                var userName = SystemOp.Resolve<User>().UserName;
                var projectId = SystemOp.Resolve<User>().ProjectId;
                PublishGame(userName, projectId);
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

        public void OnProjectDataReceived(string projectDetails)
        {
            SystemOp.Resolve<Flow>().OnProjectDetailsReceived(projectDetails, SystemOp.Resolve<System>().UpdateDefaultStudioState);
            SystemOp.Resolve<System>().LoadSceneData();
        }
    }
}

#endif
