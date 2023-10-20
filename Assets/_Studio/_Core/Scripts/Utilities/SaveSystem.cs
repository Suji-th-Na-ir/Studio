using System;
using PlayShifu.Terra;
using System.Collections.Generic;

namespace Terra.Studio
{
    public class SaveSystem : IDisposable
    {
        public event Action OnPreCheckDone;
        private const char DELIMITER = '_';
        private readonly string backwardCompatibilityFileName;
        private readonly string savedFileName;
        private const string LAST_SAVED_KEY_PREF = "LastSavedAt";
        private const string DATETIME_SAVE_FORMAT = "yyyy-MM-ddTHH:mm:ss";

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

        public void SaveManualData(string data, bool shouldIgnoreIfExistsAlready, Action<bool> callback, string autoFlushNewTimeStamp = null)
        {
            var filePath = FileService.GetSavedFilePath(savedFileName);
            SystemOp.Resolve<FileService>().WriteFile(data, filePath, shouldIgnoreIfExistsAlready, (status) =>
            {
                if (status)
                {
                    RegisterTimestampForLastSave(autoFlushNewTimeStamp);
                }
                callback?.Invoke(status);
            });
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
            private int lastURIndex = 0;

            public AutoSave()
            {
                EditorOp.Resolve<SelectionHandler>().SelectionChanged += OnSelectionChanged;
            }

            private void OnSelectionChanged(List<UnityEngine.GameObject> gameObjects)
            {
                if (gameObjects == null || gameObjects.Count == 0)
                {
                    OnDeselected();
                }
            }

            private void OnDeselected()
            {
                validateIndex++;
                if (validateIndex < VALIDATE_AUTOSAVE_AT_EVERY)
                {
                    EditorOp.Resolve<ToolbarView>().SetSaveMessage(true, SaveState.UnsavedChanges);
                    return;
                }
                validateIndex = 0;
                var currentURIndex = EditorOp.Resolve<IURCommand>().CurrentIndex;
                var delta = UnityEngine.Mathf.Abs(currentURIndex - lastURIndex);
                if (delta != VALIDATE_AUTOSAVE_AT_EVERY * 2)
                {
                    lastURIndex = currentURIndex;
                    EditorOp.Resolve<SceneDataHandler>().Save();
                }
            }

            public void Dispose()
            {
                EditorOp.Resolve<SelectionHandler>().SelectionChanged -= OnSelectionChanged;
            }
        }
    }
}