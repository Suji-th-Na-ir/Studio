using System;
using PlayShifu.Terra;
using System.Collections.Generic;

namespace Terra.Studio
{
    public class SaveSystem : IDisposable
    {
        public event Action OnPreCheckDone;
        private const char DELIMITER = '_';
        private const string LAST_SAVED_KEY_PREF = "LastSavedAt";
        private const string DATETIME_SAVE_FORMAT = "yyyy-MM-ddTHH:mm:ss";
        private string BackwardCompatibilityFileName => SystemOp.Resolve<User>().ProjectName;
        private string SavedFileName => string.Concat(SystemOp.Resolve<User>().UserName, DELIMITER, SystemOp.Resolve<User>().ProjectName);

        public SaveSystem()
        {
            SystemOp.Register(new FileService());
        }

        public void PerformPrecheck()
        {
            var isFreshInstall = string.IsNullOrEmpty(BackwardCompatibilityFileName);
            if (isFreshInstall)
            {
                OnPrecheckDone();
                return;
            }
            SystemOp
                .Resolve<FileService>()
                .DoesFileExist?.Invoke(
                    FileService.GetSavedFilePath(BackwardCompatibilityFileName),
                    PrepareSavedDataForBackwardCompatibility
                );
        }

        public void GetManualSavedData(Action<string> savedData)
        {
            var filePath = FileService.GetSavedFilePath(SavedFileName);
            SystemOp
                .Resolve<FileService>()
                .ReadFileFromLocal?.Invoke(
                    filePath,
                    (response) =>
                    {
                        savedData?.Invoke(response);
                    }
                );
        }

        public void SaveManualData(
            string data,
            bool shouldIgnoreIfExistsAlready,
            Action<bool> callback,
            string autoFlushNewTimeStamp = null
        )
        {
            var filePath = FileService.GetSavedFilePath(SavedFileName);
            SystemOp
                .Resolve<FileService>()
                .WriteFile(
                    data,
                    filePath,
                    shouldIgnoreIfExistsAlready,
                    (status) =>
                    {
                        if (status)
                        {
                            RegisterTimestampForLastSave(autoFlushNewTimeStamp);
                        }
                        callback?.Invoke(status);
                    }
                );
        }

        public string CheckAndGetFreshSaveDateTimeIfLastSavedTimestampNotPresent()
        {
            var doesPrefExist = LAST_SAVED_KEY_PREF.HasKeyInPrefs();
            if (!doesPrefExist)
            {
                var date = DateTime.Now;
                var timestamp = date.ToUniversalTime().ToString(DATETIME_SAVE_FORMAT);
                return timestamp;
            }
            LAST_SAVED_KEY_PREF.TryGetPrefString(out var dateTime);
            return dateTime;
        }

        public bool TryGetLastSavedTimestamp(out string dateTime)
        {
            LAST_SAVED_KEY_PREF.TryGetPrefString(out dateTime);
            return !string.IsNullOrEmpty(dateTime);
        }

        public void RegisterToAutosaveChangeListener(bool enable)
        {
            if (enable)
            {
                SystemOp.Register(new AutoSave());
            }
            else
            {
                SystemOp.Unregister<AutoSave>();
            }
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
                SystemOp
                    .Resolve<FileService>()
                    .RenameKeyFromDBStore?.Invoke(
                        BackwardCompatibilityFileName,
                        SavedFileName,
                        (_) =>
                        {
                            OnPrecheckDone();
                        }
                    );
            }
            else
            {
                var oldSaveFilePath = FileService.GetSavedFilePath(BackwardCompatibilityFileName);
                var newSaveFilePath = FileService.GetSavedFilePath(SavedFileName);
                SystemOp
                    .Resolve<FileService>()
                    .ReadFileFromLocal.Invoke(
                        oldSaveFilePath,
                        (response) =>
                        {
                            SystemOp
                                .Resolve<FileService>()
                                .WriteFile(response, newSaveFilePath, false, null);
                            SystemOp
                                .Resolve<FileService>()
                                .RemoveFileFromLocal?.Invoke(oldSaveFilePath, null);
                            OnPrecheckDone();
                        }
                    );
            }
        }

        private void OnPrecheckDone()
        {
            OnPreCheckDone?.Invoke();
            OnPreCheckDone = null;
        }

        private void RegisterTimestampForLastSave(string autoFlushNewTimeStamp = null)
        {
            string timestamp;
            if (string.IsNullOrEmpty(autoFlushNewTimeStamp))
            {
                var dateTime = DateTime.Now;
                timestamp = dateTime.ToUniversalTime().ToString(DATETIME_SAVE_FORMAT);
            }
            else
            {
                timestamp = autoFlushNewTimeStamp;
            }
            LAST_SAVED_KEY_PREF.SetPref(timestamp);
        }

        public void Dispose()
        {
            SystemOp.Unregister<FileService>();
        }

        public class AutoSave : IDisposable
        {
            private const int VALIDATE_AUTOSAVE_AT_EVERY = 5;
            private int validateIndex = 0;
            private bool isSaveInProgress;

            public AutoSave()
            {
                EditorOp.Resolve<SelectionHandler>().SelectionChanged += OnSelectionChanged;
            }

            private void OnSelectionChanged(List<UnityEngine.GameObject> gameObjects)
            {
                var previousSelectedObjects = EditorOp
                    .Resolve<SelectionHandler>()
                    .GetPrevSelectedObjects();
                if (gameObjects != previousSelectedObjects)
                {
                    DoValidate();
                }
            }

            private void DoValidate()
            {
                validateIndex++;
                if (validateIndex < VALIDATE_AUTOSAVE_AT_EVERY * 2)
                {
                    EditorOp.Resolve<ToolbarView>().SetSaveMessage(true, SaveState.UnsavedChanges);
                    return;
                }
                validateIndex = 0;
                if (isSaveInProgress)
                {
                    return;
                }
                isSaveInProgress = true;
                EditorOp
                    .Resolve<SceneDataHandler>()
                    .Save(() =>
                    {
                        isSaveInProgress = false;
                    });
            }

            public void Dispose()
            {
                EditorOp.Resolve<SelectionHandler>().SelectionChanged -= OnSelectionChanged;
            }
        }
    }
}
