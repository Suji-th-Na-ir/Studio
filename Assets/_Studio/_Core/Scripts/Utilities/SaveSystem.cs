namespace Terra.Studio
{
    public class SaveSystem
    {
        private const string RECOVERY_KEY_PREFIX = "RECOVERY";
        private const char DELIMITER = '_';
        private readonly string backwardCompatibilityFileName;
        private readonly string recoveryFileName;
        private readonly string savedFileName;

        public SaveSystem()
        {
            var projectName = SystemOp.Resolve<User>().ProjectName;
            var userName = SystemOp.Resolve<User>().UserName;
            backwardCompatibilityFileName = projectName;
            savedFileName = string.Concat(userName, DELIMITER, projectName);
            recoveryFileName = string.Concat(RECOVERY_KEY_PREFIX, DELIMITER, savedFileName);
            SystemOp.Resolve<FileService>().DoesFileExist(backwardCompatibilityFileName, PrepareSavedDataForBackwardCompatibility);
        }

        private void PrepareSavedDataForBackwardCompatibility(bool doFileExist)
        {
            if (!doFileExist) return;
            var saveFilePath = FileService.GetSavedFilePath(backwardCompatibilityFileName);
            var newSaveFilePath = FileService.GetSavedFilePath(savedFileName);
            SystemOp.Resolve<FileService>().ReadFileFromLocal?.Invoke(saveFilePath, (response) =>
            {
                SystemOp.Resolve<FileService>().WriteFile(response, newSaveFilePath, false);
                SystemOp.Resolve<FileService>().RemoveFileFromLocal?.Invoke(saveFilePath, null);
            });
        }
    }
}