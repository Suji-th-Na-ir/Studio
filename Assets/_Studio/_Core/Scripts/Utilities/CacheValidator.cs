using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Terra.Studio
{
    [Serializable]
    public class CachedData
    {
        public Dictionary<string, string> CloudUrlToLocalMap;
    }

    public class CacheValidator
    {
        public readonly string cacheFileName = "SavedData.json";
        public string CachedFilePath => Path.Combine(basePath, cacheFileName);

        private string basePath =>
#if UNITY_EDITOR
            Application.dataPath.Replace("Assets", "Dummy");
#else
            Application.persistentDataPath;
#endif

        private CachedData _cachedData;

        public CacheValidator()
        {
            Init();
        }

        private void Init()
        {
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }

            if (!File.Exists(CachedFilePath))
            {
                File.WriteAllText(CachedFilePath, JsonConvert.SerializeObject(new CachedData()));
            }

            var json = File.ReadAllText(CachedFilePath);
            _cachedData = JsonConvert.DeserializeObject<CachedData>(json);
            _cachedData ??= new CachedData();
            _cachedData.CloudUrlToLocalMap ??= new();
        }


        public void IsFileInCache(string key, Action<bool, string> invoked)
        {
            string localPath = "";
            if (!_cachedData.CloudUrlToLocalMap.ContainsKey(key))
            {
                invoked?.Invoke(false, localPath);
                return;
            }

            var path = Path.Combine(basePath, _cachedData.CloudUrlToLocalMap[key]);
            if (File.Exists(path))
            {
                localPath = path;
                invoked?.Invoke(true, localPath);
                return;
            }

            Debug.LogError($"THIS IS FUCKING WRONG. FILE EXISTED IN MAP BUT IN IN FS");
            invoked?.Invoke(false, localPath);
        }

        public void Save(string localPath, string content, string uniqueName)
        {
            var path = Path.Combine(basePath, localPath);
            var fileName = Path.GetFileName(localPath);

            var directoryPath = path.Replace($"{Path.DirectorySeparatorChar}{fileName}", "");
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }


            File.WriteAllText(path, content);

            _cachedData.CloudUrlToLocalMap.Add(uniqueName, localPath);

            var json = JsonConvert.SerializeObject(_cachedData);
            File.WriteAllText(CachedFilePath, json);
            SaveCacheFile();
        }

        public void Save(string localPath, byte[] content, string uniqueName)
        {
            var path = Path.Combine(basePath, localPath);
            var fileName = Path.GetFileName(localPath);

            var directoryPath = path.Replace($"{Path.DirectorySeparatorChar}{fileName}", "");
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            File.WriteAllBytes(path, content);

            _cachedData.CloudUrlToLocalMap.Add(uniqueName, localPath);

            var json = JsonConvert.SerializeObject(_cachedData);
            File.WriteAllText(CachedFilePath, json);
            SaveCacheFile();
        }

        private void Validate()
        {
            _cachedData ??= new CachedData();
            _cachedData.CloudUrlToLocalMap ??= new();
        }

        public void SaveCacheFile()
        {
#if !UNITY_WEBGL
            return;
#endif
            Debug.Log($"Saving cache file!");
            var bla = SystemOp.Resolve<FileService>();
            Validate();
            var fp = FileService.GetSavedFilePath(Path.GetFileNameWithoutExtension(cacheFileName));
            bla.WriteFile(JsonConvert.SerializeObject(_cachedData), fp, false, b =>
            {
                if (!b)
                {
                    return;
                }
            });
        }
    }
}