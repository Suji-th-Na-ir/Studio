using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

namespace Terra.Studio.RTEditor
{
    public class SystemGenerater : EditorWindow
    {
        private readonly GUIContent SYSTEM_NAME_CONTENT = EditorGUIUtility.TrTextContent("System Name", "Enter your system name, the necessary components and authors to be generated");
        private readonly GUIContent GENERATE_BUTTON_CONTENT = EditorGUIUtility.TrTextContent("Generate", "Generates system, the necessary components and authors");
        private const string SAVE_PATH = "Assets/_Studio/_Runtime/Scripts/Systems/";
        private const string COMPONENT_TEMPLATE_PATH = "Assets/_Studio/Editor/Templates/ComponentTemplate.txt";
        private const string SYSTEM_TEMPLATE_PATH = "Assets/_Studio/Editor/Templates/SystemTemplate.txt";
        private const string REPLACABLE_ID = "REPLACEME";
        private const string SUFFIX_COMPONENT_FILE = "Component";
        private const string SUFFIX_SYSTEM_FILE = "System";

        private string systemName;

        [MenuItem("Terra/Generate System %#.")]
        public static void Init()
        {
            var window = GetWindow<SystemGenerater>();
            window.titleContent = new GUIContent("Generate System")
            {
                image = EditorGUIUtility.IconContent("cs Script Icon").image
            };
            window.Show();
        }

        private void OnGUI()
        {
            AddSpace(5);
            RenderCenter(() =>
            {
                EditorGUILayout.LabelField("System and Component Generator", EditorStyles.boldLabel);
            },
            10);
            RenderTextField(SYSTEM_NAME_CONTENT, ref systemName, 5);
            RenderButton(GENERATE_BUTTON_CONTENT, Generate, 5);
        }

        private void RenderCenter(Action render, int? pixels = null)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            render?.Invoke();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            AddSpace(pixels);
        }

        private void RenderTextField(GUIContent content, ref string textRef, int? pixels = null)
        {
            textRef = EditorGUILayout.TextField(content, textRef);
            AddSpace(pixels);
        }

        private void RenderButton(GUIContent content, Action onClicked, int? pixels = null)
        {
            if (GUILayout.Button(content))
            {
                onClicked?.Invoke();
            }
            AddSpace(pixels);
        }

        private void AddSpace(int? pixels)
        {
            if (pixels != null)
            {
                GUILayout.Space(pixels.Value);
            }
        }

        private void Generate()
        {
            var isPreprocessSuccessful = PreprocessData();
            if (!isPreprocessSuccessful)
            {
                return;
            }

            #region Creation of directory

            var newSavePath = Path.Combine(SAVE_PATH, $"{systemName}{Path.DirectorySeparatorChar}");
            if (!Directory.Exists(newSavePath))
            {
                Directory.CreateDirectory(newSavePath);
            }

            #endregion
            #region Creation of component

            var componentTemplateData = AssetDatabase.LoadAssetAtPath<TextAsset>(COMPONENT_TEMPLATE_PATH).text;
            componentTemplateData = componentTemplateData.Replace(REPLACABLE_ID, systemName);
            var componentSavePath = Path.Combine(newSavePath, $"{systemName}{SUFFIX_COMPONENT_FILE}.cs");
            File.WriteAllText(componentSavePath, componentTemplateData);

            #endregion
            #region Creation of system

            var systemTemplateData = AssetDatabase.LoadAssetAtPath<TextAsset>(SYSTEM_TEMPLATE_PATH).text;
            systemTemplateData = systemTemplateData.Replace(REPLACABLE_ID, systemName);
            var systemSavePath = Path.Combine(newSavePath, $"{systemName}{SUFFIX_SYSTEM_FILE}.cs");
            File.WriteAllText(systemSavePath, systemTemplateData);

            #endregion

            AssetDatabase.Refresh();
        }

        private bool PreprocessData()
        {
            if (string.IsNullOrEmpty(systemName))
            {
                return false;
            }
            systemName = RemoveSpacesAndSpecialCharacters(systemName);
            systemName = FirstCharToUpperStringCreate(systemName);
            return true;
        }

        public string FirstCharToUpperStringCreate(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }
            return string.Create(input.Length, input, static (Span<char> chars, string str) =>
            {
                chars[0] = char.ToUpperInvariant(str[0]);
                str.AsSpan(1).CopyTo(chars[1..]);
            });
        }

        public string RemoveSpacesAndSpecialCharacters(string input)
        {
            string pattern = @"[\s!@#$%^&*()_+{}\[\]:;" + "\"\'<>,.?/|\\-]";
            return Regex.Replace(input, pattern, string.Empty);
        }
    }
}
