using System;
using Newtonsoft.Json;
using PlayShifu.Terra;

namespace Terra.Studio
{
    public class Flow
    {
        public void DoCloudSaveCheck(Action onDone)
        {
            SystemOp
                .Resolve<User>()
                .GetProjectDetails(
                    (status, response) =>
                    {
                        if (status)
                        {
                            var unpackedData = JsonConvert.DeserializeObject<APIResponse>(response);
                            if (unpackedData.status == 200)
                            {
                                OnCloudSaveDataReceived(unpackedData.data, onDone);
                            }
                            else
                            {
                                DoCloudSetup(onDone);
                            }
                        }
                        else
                        {
                            DoLocalSaveCheck(onDone);
                        }
                    }
                );
        }

        private void OnCloudSaveDataReceived(string data, Action onDone)
        {
            var unpackedData = JsonConvert.DeserializeObject<GetProjectDetailsAPI.Data>(data);
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
                        (response) =>
                        {
                            var isEmpty = string.IsNullOrEmpty(response);
                            if (isEmpty)
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
                        if (status)
                        {
                            var unpackedData = JsonConvert.DeserializeObject<APIResponse>(response);
                            if (unpackedData.status == 200)
                            {
                                CheckAndUploadSaveData(onDone);
                            }
                            else
                            {
                                DoLocalSaveCheck(onDone);
                            }
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
                SystemOp
                    .Resolve<SaveSystem>()
                    .GetManualSavedData(
                        (response) =>
                        {
                            var isAvailable = !string.IsNullOrEmpty(response);
                            if (isAvailable)
                            {
                                SystemOp.Resolve<User>().UploadSaveDataToCloud(response, null);
                                onDone?.Invoke();
                            }
                            else
                            {
                                DoLocalSaveCheck(onDone, true);
                            }
                        }
                    );
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
    }
}
