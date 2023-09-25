using System;
using System.IO;
using UnityEngine;
using PlayShifu.Terra;

namespace Terra.Studio
{
    public class FileService
    {
        public Action<string, Action<bool>> DoesFileExist;
        public Action<string, string> WriteFileIntoLocal;
        public Action<string, Action<string>> ReadFileFromLocal;

        public FileService()
        {
            if (Helper.IsPlatformWebGL())
            {
                DoesFileExist = SystemOp.Resolve<WebGLHandler>().DoesStoreHasData;
                WriteFileIntoLocal = SystemOp.Resolve<WebGLHandler>().WriteDataIntoStore;
                ReadFileFromLocal = SystemOp.Resolve<WebGLHandler>().ReadDataFromStore;
            }
            else
            {
                DoesFileExist = CheckIfFileExists;
                WriteFileIntoLocal = WriteFile;
                ReadFileFromLocal = ReadFromFile;
            }
        }

        public void WriteFile(string data, string fullFilePath, bool ignoreIfFileExists)
        {
            DoesFileExist.Invoke(fullFilePath, (doesExistInLocal) =>
            {
                if (ignoreIfFileExists && doesExistInLocal)
                {
                    return;
                }
                else
                {
                    if (Helper.IsInUnityEditor())
                    {
                        BackupFile(fullFilePath);
                    }
                    WriteFileIntoLocal.Invoke(data, fullFilePath);
                }
            });
        }

        private void WriteFile(string data, string fullFilePath)
        {
            var dirPath = Path.GetDirectoryName(fullFilePath);
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            File.WriteAllText(fullFilePath, data);
        }

        private void ReadFromFile(string filePath, Action<string> callback)
        {
            if (!File.Exists(filePath))
            {
                throw new Exception($"File does not exist at: {filePath}");
            }
            var data = File.ReadAllText(filePath);
            callback?.Invoke(data);
        }

        public void BackupFile(string fullFilePath)
        {
            if (File.Exists(fullFilePath))
            {
                var backupFile = Path.Combine(Path.GetDirectoryName(fullFilePath), $"{Path.GetFileNameWithoutExtension(fullFilePath)}_backup{Path.GetExtension(fullFilePath)}");
                File.Copy(fullFilePath, backupFile, true);
            }
        }

        public void CopyFile(string actualPath, string targetPath)
        {
            if (!File.Exists(actualPath))
            {
                throw new Exception($"Actual file does not exist at: {actualPath}");
            }
            var targetDirName = Path.GetDirectoryName(targetPath);
            if (!Directory.Exists(targetDirName))
            {
                Directory.CreateDirectory(targetDirName);
            }
            File.Copy(actualPath, targetPath, true);
        }

        public static string GetSavedFilePath(string existingFilePath)
        {
            if (existingFilePath.Contains("/") || existingFilePath.Contains(@"\"))
            {
                return Path.Combine(Application.persistentDataPath, Path.GetFileName(existingFilePath));
            }
            else
            {
                return Path.Combine(Application.persistentDataPath, $"{existingFilePath}.json");
            }
        }

        private void CheckIfFileExists(string fullFile, Action<bool> callback)
        {
            var doesFileExist = File.Exists(fullFile);
            callback?.Invoke(doesFileExist);
        }
    }
}