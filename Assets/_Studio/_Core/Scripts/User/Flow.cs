using System;
using Newtonsoft.Json;
using PlayShifu.Terra;

namespace Terra.Studio
{
    public class Flow
    {
        public void DoCloudSaveCheck(Action onDone)
        {
            var isProjectAvailable = !string.IsNullOrEmpty(SystemOp.Resolve<User>().ProjectId);
            if (!isProjectAvailable)
            {
                DoCloudSetup(onDone);
                return;
            }
            SystemOp
                .Resolve<User>()
                .GetProjectDetails(
                    (status, response) =>
                    {
                        if (status)
                        {
                            OnCloudSaveDataReceived(response, onDone);
                        }
                        else
                        {
                            DoCloudSetup(onDone);
                        }
                    }
                );
        }

        private void OnCloudSaveDataReceived(string data, Action onDone)
        {
            var unpackedData = JsonConvert.DeserializeObject<GetProjectDetailsAPI.Data>(data);
            SystemOp.Resolve<User>().UpdateProjectId(unpackedData.id).UpdateProjectName(unpackedData.projectname);
            var cloudDataIntegrityRes = IsCloudDataLatest(unpackedData.timestamp);
            if (cloudDataIntegrityRes == 0)
            {
                onDone?.Invoke();
            }
            else if (cloudDataIntegrityRes > 0)
            {
                SystemOp
                    .Resolve<SaveSystem>()
                    .SaveManualData(
                        unpackedData.content,
                        false,
                        (status) =>
                        {
                            onDone?.Invoke();
                        },
                        unpackedData.timestamp
                    );
            }
            else
            {
                SystemOp
                    .Resolve<SaveSystem>()
                    .GetManualSavedData(
                        (status, response) =>
                        {
                            if (status)
                            {
                                DoLocalSaveCheck(onDone, true);
                            }
                            else
                            {
                                SystemOp.Resolve<User>().UploadSaveDataToCloud(response, null);
                                onDone?.Invoke();
                            }
                        }
                    );
            }
        }

        private int IsCloudDataLatest(string cloudTimestamp)
        {
            var isLastSavedAvailable = SystemOp
                .Resolve<SaveSystem>()
                .TryGetLastSavedTimestamp(out var saveDateTime);
            if (!isLastSavedAvailable)
                return 1;
            var lastSaveDateTime = DateTime.Parse(saveDateTime);
            var cloudSaveDateTime = DateTime.Parse(cloudTimestamp);
            var result = DateTime.Compare(cloudSaveDateTime, lastSaveDateTime);
            return result;
        }

        private void DoCloudSetup(Action onDone)
        {
            SystemOp
                .Resolve<User>()
                .CreateNewProject(
                    (status, response) =>
                    {
                        SystemOp.Resolve<User>().UpdateProjectId(response);
                        if (status)
                        {
                            CheckAndUploadSaveData(onDone);
                        }
                        else
                        {
                            DoLocalSaveCheck(onDone);
                        }
                    }
                );
        }

        private void CheckAndUploadSaveData(Action onDone)
        {
            var isLastSavedAvailable = SystemOp
                .Resolve<SaveSystem>()
                .TryGetLastSavedTimestamp(out var saveDateTime);
            if (isLastSavedAvailable)
            {
                var projectId = SystemOp.Resolve<User>().ProjectId;
                var filePath = FileService.GetSavedFilePath(projectId);
                SystemOp.Resolve<SaveSystem>().GetManualSavedData((status, response) =>
                {
                    if (status)
                    {
                        SystemOp.Resolve<User>().UploadSaveDataToCloud(response, null);
                        onDone?.Invoke();
                    }
                    else
                    {
                        DoLocalSaveCheck(onDone, true);
                    }
                });
            }
            else
            {
                DoLocalSaveCheck(onDone, true);
            }
        }

        public void DoLocalSaveCheck(Action onDone, bool autoSaveToCloud = false)
        {
            var saveData = SystemOp.Resolve<System>().ConfigSO.SceneDataToLoad.text;
            var shouldIgnore = !Helper.IsInUnityEditor();
            SystemOp
                .Resolve<SaveSystem>()
                .SaveManualData(
                    saveData,
                    shouldIgnore,
                    (_) =>
                    {
                        if (autoSaveToCloud)
                        {
                            SystemOp.Resolve<User>().UploadSaveDataToCloud(saveData, null);
                        }
                        onDone?.Invoke();
                    }
                );
        }

        public void OnProjectDetailsReceived(string projectDetails, Action<StudioState> callback)
        {
            var unpackedData = JsonConvert.DeserializeObject<ProjectData>(projectDetails);
            if (string.IsNullOrEmpty(unpackedData.ProjectName))
            {
                unpackedData.ProjectName = SystemOp.Resolve<System>().ConfigSO.SceneDataToLoad.name;
            }
            SystemOp.Resolve<User>().
                UpdateProjectId(unpackedData.ProjectId).
                UpdateProjectName(unpackedData.ProjectName);
            callback?.Invoke(unpackedData.State);
        }

        public void GetAndSetProjectDetailsForRuntime(Action callback)
        {
            var loadFromCloud = SystemOp.Resolve<System>().ConfigSO.ServeFromCloud;
            if (loadFromCloud)
            {
                new GetPublishDataAPI().DoRequest((status, response) =>
                {
                    if (status)
                    {
                        SystemOp.Resolve<CrossSceneDataHolder>().Set(response);
                    }
                    else
                    {
                        ApplyLocalDataToCrossceneData();
                    }
                    callback?.Invoke();
                });
            }
            else
            {
                ApplyLocalDataToCrossceneData();
                callback?.Invoke();
            }
        }

        private void ApplyLocalDataToCrossceneData()
        {
            var saveData = SystemOp.Resolve<System>().ConfigSO.SceneDataToLoad.text;
            SystemOp.Resolve<CrossSceneDataHolder>().Set(saveData);
        }
    }
}