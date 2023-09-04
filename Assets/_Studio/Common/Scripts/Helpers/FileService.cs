using System;
using System.IO;
using UnityEngine;

namespace Terra.Studio
{
    public class FileService
    {
        public void WriteFile(string data, string fullFilePath, bool ignoreIfFileExists)
        {
            if (ignoreIfFileExists && File.Exists(fullFilePath))
            {
                return;
            }
            if (File.Exists(fullFilePath) && !ignoreIfFileExists)
            {
                var backupFile = Path.Combine(Path.GetDirectoryName(fullFilePath), $"{Path.GetFileNameWithoutExtension(fullFilePath)}_backup{Path.GetExtension(fullFilePath)}");
                File.Copy(fullFilePath, backupFile, true);
            }
            WriteFile(data, fullFilePath);
        }

        public void WriteFile(string data, string fullFilePath)
        {
            var dirPath = Path.GetDirectoryName(fullFilePath);
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            File.WriteAllText(fullFilePath, data);
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

        public string ReadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new Exception($"File does not exist at: {filePath}");
            }
            var data = File.ReadAllText(filePath);
            return data;
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
    }
}