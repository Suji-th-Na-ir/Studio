using System;
using System.IO;
using UnityEngine;

namespace Terra.Studio
{
    public class FileService
    {
        private const string SAVED_FILE_TEXT_NAME = "SavedFile.json";

        public void WriteFile(string data, string fullFilePath, bool ignoreIfFileExists)
        {
            if (ignoreIfFileExists)
            {
                if (File.Exists(fullFilePath))
                {
                    return;
                }
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

        public static string GetSavedFilePath()
        {
            return Path.Combine(Application.persistentDataPath, SAVED_FILE_TEXT_NAME);
        }
    }
}