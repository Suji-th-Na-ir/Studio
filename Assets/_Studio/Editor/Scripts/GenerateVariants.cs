using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using System.Reflection;
using System.Collections.Generic;
using RuntimeInspectorNamespace;

namespace Terra.Studio.RTEditor
{
    public class GenerateVariants
    {
        private const string AUTHORS_FILE_PATH = "Assets/_Studio/Resources/Runtime/AuthorsVariants.txt";
        private const string EVENT_FILE_PATH = "Assets/_Studio/Resources/Runtime/EventsVariants.txt";
        private const string COMPONENT_DRAWERS_FILE_PATH = "Assets/_Studio/Resources/Editortime/ComponentDrawersVariants.txt";
        private const string COMPONENT_FIELDS_FILE_PATH = "Assets/_Studio/Resources/Editortime/EnumFieldsVariants.txt";

        [InitializeOnLoadMethod]
        private static void Generate()
        {
            AfterAssemblyReload();
        }

        private static void AfterAssemblyReload()
        {
            CheckIfFolderExists();
            GetAllAuthors();
            GetAllEvents();
            GetAllDrawerComponents();
            GetAllEnumFieldComponents();
        }

        private static void GetAllAuthors()
        {
            var dict = new Dictionary<string, string>();
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
            var dict = new Dictionary<string, string>();
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

        private static void GetAllDrawerComponents()
        {
            var dict = new Dictionary<string, string>();
            var assembly = Assembly.GetAssembly(typeof(BaseAuthor));
            var derivedTypes = assembly.GetTypes()
                .Where(type => type.IsSubclassOf(typeof(MonoBehaviour)))
                .ToArray();
            foreach (var derivedType in derivedTypes)
            {
                var editorDrawAttribute = derivedType.GetCustomAttribute<EditorDrawComponentAttribute>();
                if (editorDrawAttribute != null)
                {
                    if (!dict.ContainsKey(editorDrawAttribute.ComponentTarget))
                    {
                        dict.Add(editorDrawAttribute.ComponentTarget, derivedType.FullName);
                    }
                }
            }
            CreateFile(COMPONENT_DRAWERS_FILE_PATH, dict);
        }

        private static void GetAllEnumFieldComponents()
        {
           
            var dict = new Dictionary<string, string[]>();
            var assembly = Assembly.GetAssembly(typeof(BaseAuthor));
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsEnum)
                {
                    var enumValues = Enum.GetValues(type);
                    foreach (var enumValue in enumValues)
                    {
                        var name = enumValue.ToString();
                        var enumType = enumValue.GetType();
                        var fieldInfo = type.GetField(name);
                        var attribute = fieldInfo.GetCustomAttribute<EditorEnumFieldAttribute>();
                        if (attribute != null)
                        { 
                            if (!dict.ContainsKey(attribute.ComponentTarget))
                            {
                                
                                dict.Add(enumType + "." + name, new string[]{ attribute.ComponentTarget,attribute.ComponentData});
                            }
                        }
                    }
                }
            }
            CreateFile(COMPONENT_FIELDS_FILE_PATH, dict);
        }

        private static void CheckIfFolderExists()
        {
            if (!Directory.Exists("Assets/_Studio/Resources/Runtime"))
            {
                Directory.CreateDirectory("Assets/_Studio/Resources/Runtime");
            }
            if (!Directory.Exists("Assets/_Studio/Resources/Editortime"))
            {
                Directory.CreateDirectory("Assets/_Studio/Resources/Editortime");
            }
        }

        private static void CreateFile(string filePath, Dictionary<string, string> data)
        {
            data ??= new();
            File.WriteAllText(filePath, JsonConvert.SerializeObject(data, Formatting.Indented));
        }

        private static void CreateFile(string filePath, Dictionary<string, string[]> data)
        {
            data ??= new();
            File.WriteAllText(filePath, JsonConvert.SerializeObject(data, Formatting.Indented));
        }
    }
}
