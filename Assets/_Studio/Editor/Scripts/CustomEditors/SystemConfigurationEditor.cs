using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;

namespace Terra.Studio.RTEditor
{
    [CustomEditor(typeof(SystemConfigurationSO))]
    public class SystemConfigurationEditor : Editor
    {
        private System system;
        private string username;
        private string publishedProjId;
        private int selectedStudioMode;

        private const string USERNAME_PREF = "UserName";
        private const string STUDIO_PASSWORD = "Studio@12345";
        private static readonly string[] STATES = new[] { "Editor", "Runtime" };

        private void OnEnable()
        {
            username = PlayerPrefs.GetString(USERNAME_PREF, string.Empty);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUILayout.Space(10);
            EditorGUILayout.HelpBox("Note: Load from cloud has to be disabled for the data sync to work in editor.", MessageType.Info);
            ProvidePilotConsole();
        }

        private void ProvidePilotConsole()
        {
            GUILayout.Space(15);
            if (GUILayout.Button("Clear All Prefs"))
            {
                PlayerPrefs.DeleteAll();
            }
            GUILayout.Space(5);
            EditorGUILayout.LabelField("Pilot Console", EditorStyles.boldLabel);
            GUILayout.Space(5);
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Cannot use pilot console while not playing!", MessageType.Warning);
                return;
            }
            var currentScene = SceneManager.GetActiveScene();
            if (!currentScene.name.Equals("Bootstrap"))
            {
                EditorGUILayout.HelpBox("Cannot use pilot console when bootstrap is not active scene!", MessageType.Warning);
                return;
            }
            if (!system)
            {
                system = FindObjectOfType<System>();
            }
            if (!system)
            {
                EditorGUILayout.HelpBox("Cannot use pilot console since system is not found in bootstrap!", MessageType.Warning);
                return;
            }
            GUILayout.Space(5);
            username = EditorGUILayout.TextField("Username", username);
            selectedStudioMode = EditorGUILayout.Popup("Studio Mode", selectedStudioMode, STATES);
            publishedProjId = EditorGUILayout.TextField("Published Project Id", publishedProjId);
            if (string.IsNullOrEmpty(username))
            {
                return;
            }
            GUILayout.Space(5);
            if (GUILayout.Button("Setup"))
            {
                SetupUser();
            }
        }

        private void SetupUser()
        {
            if (selectedStudioMode == 1 && string.IsNullOrEmpty(publishedProjId))
            {
                EditorUtility.DisplayDialog("Terra Studio", "Cannot setup if the expected state to load is runtime and published proj id is empty", "Ok");
                return;
            }
            SystemOp.Resolve<User>().UpdateUserName(username).UpdatePassword(STUDIO_PASSWORD);
            SystemOp.Resolve<User>().AttemptCloudLogin(null, OnResponseReceived);
        }

        private void OnResponseReceived(bool status, string response)
        {
            if (!status)
            {
                EditorUtility.DisplayDialog("Terra Studio", "Login failed! Please retry later Or check password in the code line 17 matching with API in backend", "Ok");
                return;
            }
            SystemOp.Resolve<User>().UpdateUserName(username);
            var unpackedData = JsonConvert.DeserializeObject<LoginAPI.Data>(response);
            unpackedData.IsAutoLoggedIn = false;
            var projectDetails = new ProjectData()
            {
                ParamValue = selectedStudioMode + 1,
                ProjectId = selectedStudioMode == 0 ? unpackedData.ProjectId : publishedProjId,
            };
            SystemOp.Resolve<User>()
                .UpdateUserId(unpackedData.UserId)
                .UpdateUserName(unpackedData.Username)
                .LogLoginEvent(unpackedData.IsAutoLoggedIn);
            var projectJson = JsonConvert.SerializeObject(projectDetails);
            SystemOp.Resolve<Flow>().OnProjectDetailsReceived(projectJson, SystemOp.Resolve<System>().UpdateDefaultStudioState);
            SystemOp.Resolve<System>().LoadSceneData();
            SystemOp.Resolve<System>().OnSubsystemLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded()
        {
            if (SystemOp.Resolve<System>().CurrentStudioState == StudioState.Editor)
            {
                CoroutineService.RunCoroutine(() =>
                {
                    EditorOp.Resolve<ToolbarView>().OnPublishRequested += () =>
                    {
                        PublishProject();
                    };
                },
                CoroutineService.DelayType.WaitForXFrames, 10);
                SystemOp.Resolve<System>().OnSubsystemLoaded += OnSceneLoaded;
            }
        }

        private void PublishProject()
        {
            SystemOp.Resolve<User>().PublishProject((status, response) =>
            {
                if (status)
                {
                    EditorUtility.DisplayDialog("Terra Studio", $"Published Successfully! Response: {response}", "Ok");
                }
                else
                {
                    EditorUtility.DisplayDialog("Terra Studio", $"Publish Failed! Response: {response}", "Ok");
                }
            });
        }
    }
}
