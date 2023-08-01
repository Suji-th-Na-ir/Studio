using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using System.Reflection;
using System.Collections.Generic;

namespace Terra.Studio.RTEditor
{
    public class GenerateVariants
    {
        private const string AUTHORS_FILE_PATH = "Assets/_Studio/Resources/AuthorsVariants.txt";
        private const string EVENT_FILE_PATH = "Assets/_Studio/Resources/EventsVariants.txt";

        [InitializeOnLoadMethod]
        private static void Generate()
        {
            if (!EditorPrefs.GetBool("HasSetCodeChecker", false))
            {
                return;
            }
            EditorPrefs.SetBool("HasSetCodeChecker", true);
            AssemblyReloadEvents.afterAssemblyReload += AfterAssemblyReload;
        }

        private static void AfterAssemblyReload()
        {
            GetAllAuthors();
            GetAllEvents();
        }

        private static void GetAllAuthors()
        {
            var dict = GetFileData(AUTHORS_FILE_PATH);
            var assembly = Assembly.GetAssembly(typeof(BaseAuthor));
            var derivedTypes = assembly.GetTypes()
                .Where(type => type.IsSubclassOf(typeof(BaseAuthor)))
                .ToArray();
            foreach (var derivedType in derivedTypes)
            {
                var authorAttribute = derivedType.GetCustomAttribute<AuthorAttribute>();
                if (authorAttribute != null)
                {
                    if (!dict.ContainsKey(authorAttribute.AuthorTarget))
                    {
                        dict.Add(authorAttribute.AuthorTarget, derivedType.FullName);
                    }
                }
            }
            CreateFile(AUTHORS_FILE_PATH, dict);
        }

        private static void GetAllEvents()
        {
            var dict = GetFileData(EVENT_FILE_PATH);
            var assembly = Assembly.GetAssembly(typeof(IEventExecutor));
            var derivedTypes = assembly.GetTypes()
                .Where(type => type.IsValueType && !type.IsEnum)
                .Where(type => type.GetInterfaces().Contains(typeof(IEventExecutor)))
                .Where(type => type.GetCustomAttribute<EventExecutorAttribute>() != null)
                .ToArray();
            foreach (var derivedType in derivedTypes)
            {
                var eventAttribute = derivedType.GetCustomAttribute<EventExecutorAttribute>();
                if (eventAttribute != null)
                {
                    if (!dict.ContainsKey(eventAttribute.EventTarget))
                    {
                        dict.Add(eventAttribute.EventTarget, derivedType.FullName);
                    }
                }
            }
            CreateFile(EVENT_FILE_PATH, dict);
        }

        private static void CreateFile(string filePath, Dictionary<string, string> data = null)
        {
            if (data == null)
            {
                data = new();
            }
            File.WriteAllText(filePath, JsonConvert.SerializeObject(data, Formatting.Indented));
        }

        private static Dictionary<string, string> GetFileData(string filePath)
        {
            if (!File.Exists(filePath))
            {
                CreateFile(filePath);
            }
            var text = File.ReadAllText(filePath);
            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(text);
            return dict;
        }
    }
}
