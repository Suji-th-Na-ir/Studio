using System;
using PlayShifu.Terra;

namespace Terra.Studio
{
    public class SaveSystem : IDisposable
    {
        public event Action OnPreCheckDone;
        private const char DELIMITER = '_';
        private readonly string backwardCompatibilityFileName;
        private readonly string savedFileName;
        private const string LAST_SAVED_KEY_PREF = "LastSavedAt";
        private const string DATETIME_SAVE_FORMAT = "yyyy-MM-ddTHH:mm:ss.fffffffZ";

        public SaveSystem()
        {
            SystemOp.Register(new FileService());
            var projectName = SystemOp.Resolve<User>().ProjectName;
            var userName = SystemOp.Resolve<User>().UserName;
            backwardCompatibilityFileName = projectName;
            savedFileName = string.Concat(userName, DELIMITER, projectName);
        }

        public void PerformPrecheck()
        {
            var isFreshInstall = string.IsNullOrEmpty(backwardCompatibilityFileName);
            if (isFreshInstall)
            {
                OnPrecheckDone();
                return;
            }
            SystemOp.Resolve<FileService>().DoesFileExist?.Invoke(
                FileService.GetSavedFilePath(backwardCompatibilityFileName),
                PrepareSavedDataForBackwardCompatibility);
        }

        public void GetManualSavedData(Action<string> savedData)
        {
            var filePath = FileService.GetSavedFilePath(savedFileName);
            SystemOp.Resolve<FileService>().ReadFileFromLocal?.Invoke(filePath, (response) =>
            {
                savedData?.Invoke(response);
            });
        }

        public void SaveManualData(string data, bool shouldIgnoreIfExistsAlready, Action<bool> callback)
        {
            var filePath = FileService.GetSavedFilePath(savedFileName);
            SystemOp.Resolve<FileService>().WriteFile(data, filePath, shouldIgnoreIfExistsAlready, callback);
            RegisterTimestampForLastSave();
        }

        public string CheckAndGetFreshSaveDateTimeIfLastSavedTimestampNotPresent()
        {
            var doesPrefExist = LAST_SAVED_KEY_PREF.HasKeyInPrefs();
            if (!doesPrefExist) return DateTime.UtcNow.ToString(DATETIME_SAVE_FORMAT);
            LAST_SAVED_KEY_PREF.TryGetPrefString(out var dateTime);
            return dateTime;
        }

        public bool TryGetLastSavedTimestamp(out string dateTime)
        {
            LAST_SAVED_KEY_PREF.TryGetPrefString(out dateTime);
            return !string.IsNullOrEmpty(dateTime);
        }

        private void PrepareSavedDataForBackwardCompatibility(bool doFileExist)
        {
            if (!doFileExist)
            {
                OnPrecheckDone();
                return;
            }
            RegisterTimestampForLastSave();
            if (Helper.IsPlatformWebGL())
            {
                SystemOp.Resolve<FileService>().RenameKeyFromDBStore?.Invoke(backwardCompatibilityFileName, savedFileName, (_) =>
                {
                    OnPrecheckDone();
                });
            }
            else
            {
                var oldSaveFilePath = FileService.GetSavedFilePath(backwardCompatibilityFileName);
                var newSaveFilePath = FileService.GetSavedFilePath(savedFileName);
                SystemOp.Resolve<FileService>().ReadFileFromLocal.Invoke(oldSaveFilePath, (response) =>
                {
                    SystemOp.Resolve<FileService>().WriteFile(response, newSaveFilePath, false, null);
                    SystemOp.Resolve<FileService>().RemoveFileFromLocal?.Invoke(oldSaveFilePath, null);
                    OnPrecheckDone();
                });
            }
        }

        private void OnPrecheckDone()
        {
            OnPreCheckDone?.Invoke();
            OnPreCheckDone = null;
        }

        private void RegisterTimestampForLastSave()
        {
            var timestamp = DateTime.UtcNow.ToString(DATETIME_SAVE_FORMAT);
            LAST_SAVED_KEY_PREF.SetPref(timestamp);
        }

        public void Dispose()
        {
            SystemOp.Unregister<FileService>();
        }
    }
}