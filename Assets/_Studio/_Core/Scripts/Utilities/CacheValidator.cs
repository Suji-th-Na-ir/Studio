using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace Terra.Studio
{
    [Serializable]
    public class CachedData
    {
        public Dictionary<string, string> cloudUrlToLocalMap;
    }
    public class CacheValidator
    {
        public  readonly string cacheFileName = "SavedData.json";
        public string cachedFilePath => Path.Combine(basePath, cacheFileName);

        private string basePath =>
#if UNITY_EDITOR
            Application.dataPath.Replace("Assets","Dummy");
        #else
            Application.persistentDataPath;
#endif

        private CachedData cachedData;

        public CacheValidator()
        {
            Init();
        }

        public void Init()
        {
            Debug.Log($"Cache Validator Initialized");
// #if UNITY_EDITOR || !UNITY_WEBGL
            if (!Directory.Exists(basePath))
            {
                Debug.Log($"Directory did not exist, creating");
                Directory.CreateDirectory(basePath);
            }
            
            if (!File.Exists(cachedFilePath))
            {
                File.WriteAllText(cachedFilePath, JsonConvert.SerializeObject(new CachedData()));
            }
            
            var json = File.ReadAllText(cachedFilePath);
            cachedData = JsonConvert.DeserializeObject<CachedData>(json);
            cachedData??= new CachedData();
            cachedData.cloudUrlToLocalMap ??= new();
// #else
//             Validate();
//             SaveCacheFile();
//             
// #endif
        }

        
        public void IsFileInCache(string key, Action<bool,string> invoked)
        {
            string localPath = "";
            if (!cachedData.cloudUrlToLocalMap.ContainsKey(key))
            {
                invoked?.Invoke(false, localPath);
                return;
            }
// #if !UNITY_EDITOR || !UNITY_WEBGL

            var path = Path.Combine(basePath, cachedData.cloudUrlToLocalMap[key]);
            if (File.Exists(path))
            {
                localPath = path;
                invoked?.Invoke(true, localPath);
                return;
            }
            Debug.LogError($"THIS IS FUCKING WRONG. FILE EXISTED IN MAP BUT IN IN FS");
            invoked?.Invoke(false, localPath);
// #else
//             
//             
//             Validate();
//             var bla = SystemOp.Resolve<FileService>();
//             var fp = FileService.GetSavedFilePath(cachedData.cloudUrlToLocalMap[key]);
//             Debug.Log($"Checking for file {fp}");
//             localPath = fp;
//
//             var exists = File.Exists(fp);
//             if (!exists)
//             {
//                 Debug.LogError($"THIS IS FUCKING WRONG. FILE EXISTED IN MAP BUT IN IN FS");
//             }
//             invoked?.Invoke(exists, localPath);
// #endif
        }

        public void Save(string localPath, string content, string uniqueName)
        {
// #if !UNITY_EDITOR || !UNITY_WEBGL
            var path = Path.Combine(basePath, localPath);
            var fileName = Path.GetFileName(localPath);

            var directoryPath = path.Replace($"{Path.DirectorySeparatorChar}{fileName}", "");
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            
            
            File.WriteAllText(path, content);
            
            cachedData.cloudUrlToLocalMap.Add(uniqueName, localPath);

            var json = JsonConvert.SerializeObject(cachedData);
            File.WriteAllText(cachedFilePath, json);
            SaveCacheFile();
// #else
//             Debug.Log($"Saving {localPath}.....{uniqueName}....{content.Length}");
//             Validate();
//             var bla = SystemOp.Resolve<FileService>();
//             var fp = FileService.GetSavedFilePath(localPath);
//             Debug.Log($"Saving file {fp}");
//             File.WriteAllText(fp, content);
//             // bla.WriteFile(content, fp, false, (x)=>
//             // {
//             //     Debug.Log($"File with {uniqueName} and {filePath} saved!......{x}");
//             //        
//             // });
//             cachedData.cloudUrlToLocalMap.Add(uniqueName, localPath);
//             SaveCacheFile();
// #endif
        }

        public void Save(string localPath, byte[] content, string uniqueName)
        {
// #if !UNITY_EDITOR || !UNITY_WEBGL
            var path = Path.Combine(basePath, localPath);
            var fileName = Path.GetFileName(localPath);

            var directoryPath = path.Replace($"{Path.DirectorySeparatorChar}{fileName}", "");
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            
            File.WriteAllBytes(path, content);
            
            cachedData.cloudUrlToLocalMap.Add(uniqueName, localPath);

            var json = JsonConvert.SerializeObject(cachedData);
            File.WriteAllText(cachedFilePath, json);
            SaveCacheFile();
// #else
//             Validate();
//             Debug.Log($"Saving {localPath}.....{uniqueName}....{content.Length}");
//             var bla = SystemOp.Resolve<FileService>();
//             var fp = FileService.GetSavedFilePath(localPath);
//             var toWrite = Encoding.UTF8.GetString(content, 0, content.Length);
//             Debug.Log($"Saving file {fp}");
//             File.WriteAllBytes(fp, content);
//             // bla.WriteFile(toWrite, fp, false, (x)=>
//             // {
//             //     Debug.Log($"File with {uniqueName} and {filePath} saved!......{x}");
//             // });
//             cachedData.cloudUrlToLocalMap.Add(uniqueName, localPath);
//             SaveCacheFile();
// #endif
        }

        private void Validate()
        {
            cachedData??= new CachedData();
            cachedData.cloudUrlToLocalMap ??= new();
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
            bla.WriteFile(JsonConvert.SerializeObject(cachedData),fp, false, b =>
            {
                if (!b)
                {
                    return;
                }
            });
        }
        
    }
}