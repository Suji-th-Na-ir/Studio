using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.EventSystems;
using System.Collections;
using System.Reflection;

namespace PlayShifu.Terra
{
    public static class Helper
    {
        public static string GetCurrentAppPlatform()
        {
            string platform = "ios";
#if UNITY_IOS
            platform = "ios";
#elif UNITY_STANDALONE_LINUX
            platform = "linux";
#elif UNITY_STANDALONE_OSX
            platform = "macos";
#elif UNITY_STANDALONE_WIN
            platform = "windows";
#elif UNITY_ANDROID
            platform = "android";
#endif
            return platform;
        }

        public static bool IsPhone()
        {
            return (GetAspectRatio() > 1.65f);
        }

        public static bool ISaKindleDevice()
        {
            List<string> deviceNames = new List<string>{
            "KFMUWI","KFKAWI","KFONWI",
        };

            string deviceModel = SystemInfo.deviceModel;
            foreach (string name in deviceNames)
            {
                if (deviceModel.Contains(name))
                    return true;
            }

            return false;
        }

        public static bool IsRamLessThan2GB()
        {
            return SystemInfo.systemMemorySize < 2100;
        }
        public static bool IsIpad3or4()
        {
            if (SystemInfo.deviceModel.Contains("iPad3") || SystemInfo.deviceModel.Contains("iPad4"))
                return true;

            return false;
        }

        public static bool ISaCenterCamDevice()
        {
            List<string> deviceNames = new List<string>{
            "KFONWI",
        };

            string deviceModel = SystemInfo.deviceModel;
            foreach (string name in deviceNames)
            {
                if (deviceModel.Contains(name))
                    return true;
            }

            return false;

        }
        public static T FindDeepChild<T>(Transform aParent, string aName, bool exactMatch = true) where T : Component
        {
            var transform = FindDeepChild(aParent, aName, exactMatch);
            if (transform != null)
            {
                return transform.GetComponent<T>();
            }
            return null;
        }

        public static Transform FindDeepChild(Transform aParent, string aName, bool exactMatch = true)
        {
            if (aParent == null)
            {
                return null;
            }
            Queue<Transform> queue = new Queue<Transform>();
            queue.Enqueue(aParent);
            while (queue.Count > 0)
            {
                var c = queue.Dequeue();
                if (c == null) continue;
                if (exactMatch)
                {
                    if (c.name == aName)
                    {
                        return c;
                    }
                }
                else
                {
                    if (c.name.Contains(aName))
                    {
                        return c;
                    }
                }

                foreach (Transform t in c)
                    queue.Enqueue(t);
            }
            return null;
        }

        public static List<Transform> FindDeepChildsWithName(Transform aParent, string aName, bool exactMatch)
        {
            List<Transform> allT = new List<Transform>();

            Queue<Transform> queue = new Queue<Transform>();
            queue.Enqueue(aParent);
            while (queue.Count > 0)
            {
                var c = queue.Dequeue();
                if (exactMatch)
                {
                    if (c.name == aName)
                    {
                        allT.Add(c);
                    }
                }
                else
                {
                    if (c.name.Contains(aName))
                    {
                        allT.Add(c);
                    }
                }

                foreach (Transform t in c)
                    queue.Enqueue(t);
            }
            return allT;
        }

        public static float GetAspectRatio()
        {
            return (float)((float)Screen.width / (float)Screen.height);
        }

        public static Color GetColorFromHex(string a_strHexValue)
        {
            Color colorToReturn = Color.black;
            if (!a_strHexValue.StartsWith("#"))
                a_strHexValue = "#" + a_strHexValue;
            ColorUtility.TryParseHtmlString(a_strHexValue, out colorToReturn);
            return colorToReturn;
        }

        public static string GetColorHexFromColor(Color color)
        {
            return ColorUtility.ToHtmlStringRGB(color);
        }

        public static string GetColorHexFromColorWithAlpha(Color32 color)
        {
            return string.Format("{0}/{1}", ColorUtility.ToHtmlStringRGB(color), color.a);
        }

        public static Color GetColorFromHexWithAlpha(string a_strHexValueWithAlpha)
        {
            Color32 colorToReturn = Color.black;
            string[] cols = a_strHexValueWithAlpha.Split("/");
            if (cols.Length > 0)
            {
                colorToReturn = GetColorFromHex(cols[0]);
            }
            if (cols.Length == 2)
            {
                byte.TryParse(cols[1], out colorToReturn.a);
            }
            return colorToReturn;
        }

        public static float NormalizedValue(float value, float min, float max)
        {
            float normalized = (value - min) / (max - min);
            return normalized;
        }

        public static bool Approximately(this Quaternion quatA, Quaternion value, float precision)
        {
            if (quatA.Equals(value))
            {
                return true;
            }
            float dotVal = Mathf.Abs(Quaternion.Dot(quatA, value));
            return dotVal >= 1f - precision;
        }

        public static bool IsPointerOverUIElement(Vector2 mousePosition, out string name, bool checkUILayerOnly = true)
        {
            return IsPointerOverUIElement(GetEventSystemRaycastResults(mousePosition), out name, checkUILayerOnly);
        }

        ///Returns 'true' if we touched or hovering on Unity UI element.
        private static bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults, out string name, bool checkUILayerOnly = true)
        {
            name = string.Empty;
            for (int index = 0; index < eventSystemRaysastResults.Count; index++)
            {
                RaycastResult curRaysastResult = eventSystemRaysastResults[index];
                // Debug.LogFormat("Raycast result : {0} -- {1}", curRaysastResult.gameObject.name, curRaysastResult.gameObject.layer);
                if (checkUILayerOnly && curRaysastResult.gameObject.layer == LayerMask.NameToLayer("UI"))
                {
                    name = curRaysastResult.gameObject.name;
                    return true;
                }
                else if (!checkUILayerOnly)
                {
                    name = curRaysastResult.gameObject.name;
                    return true;
                }
            }
            return false;
        }

        ///Gets all event systen raycast results of current mouse or touch position.
        private static List<RaycastResult> GetEventSystemRaycastResults(Vector2 position)
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = position;
            List<RaycastResult> raysastResults = new List<RaycastResult>();
            if (EventSystem.current != null)
            {
                EventSystem.current.RaycastAll(eventData, raysastResults);
            }
            return raysastResults;
        }

        public static float troubleShootHelpDelayCount = 0;
        public static void ResetTroubleshootHelper()
        {
            troubleShootHelpDelayCount = 0;
        }
        public static bool IncreaseTroubleshootHelper(float _count = 1f)
        {
            troubleShootHelpDelayCount += _count;
            if (troubleShootHelpDelayCount > 5)
                return true;
            else
                return false;
        }


        public static GenericItemCount<T> FindMostPopular<T>(List<T> list)
        {
            GenericItemCount<T> itemCount = new GenericItemCount<T>();

            var query = from i in list
                        group i by i into g
                        select new { g.Key, Count = g.Count() };

            int frequency = query.Max(g => g.Count);

            // find the values with that frequency
            List<T> modes = query.Where(g => g.Count == frequency).Select(g => g.Key).ToList();
            if (modes.Count > 0)
            {
                itemCount.Item = modes.First();
                itemCount.Count = frequency;
            }
            return itemCount;
        }

        public class GenericItemCount<T>
        {
            public T Item;
            public int Count;
        }

        public static bool IsBetween(float value, float min, float max)
        {
            return (value >= min && value <= max);
        }

        public static bool IsDeviceRamHigherThan2GB()
        {
            Debug.Log($"System Memory = {SystemInfo.systemMemorySize}");
            if (SystemInfo.systemMemorySize >= 2048)
                return true;
            return false;
        }

        public static IEnumerator FadeUIElement(CanvasGroup canvasGroup,
            bool enabled, float duration = 0.5f, float delayBeforeFade = 0)
        {
            return FadeUIElements(new List<CanvasGroup>() { canvasGroup },
                enabled, duration, delayBeforeFade);
        }

        public static IEnumerator FadeUIElements(List<CanvasGroup> canvasGroups,
            bool enabled, float duration = 0.5f, float delayBeforeFade = 0)
        {
            float targetAlpha;
            if (enabled)
            {
                targetAlpha = 1;
            }
            else
            {
                targetAlpha = 0;
            }
            List<float> currAlphas = new List<float>();
            for (int i = 0; i < canvasGroups.Count; i++)
            {
                if (enabled)
                {
                    if (!canvasGroups[i].gameObject.activeSelf)
                    {
                        canvasGroups[i].gameObject.SetActive(true);
                        canvasGroups[i].alpha = 0;
                    }
                }
                currAlphas.Add(canvasGroups[i].alpha);
            }
            if (delayBeforeFade != 0)
            {
                yield return new WaitForSeconds(delayBeforeFade);
            }
            for (float t = 0; t <= duration; t += Time.deltaTime)
            {
                for (int i = 0; i < canvasGroups.Count; i++)
                {
                    canvasGroups[i].alpha = Mathf.Lerp(currAlphas[i],
                        targetAlpha, t / duration);
                }
                yield return new WaitForEndOfFrame();
            }
            for (int i = 0; i < canvasGroups.Count; i++)
            {
                canvasGroups[i].alpha = targetAlpha;
                if (!enabled)
                {
                    canvasGroups[i].gameObject.SetActive(false);
                }
            }
        }

#if UNITY_EDITOR
        public static List<T> FindAssetsByTypeInProjectWindow<T>() where T : UnityEngine.Object
        {
            List<T> assets = new List<T>();
            string[] guids = UnityEditor.AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T)));
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]);
                T asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset != null)
                {
                    assets.Add(asset);
                }
            }
            return assets;
        }
#endif

        public static string GetCoreDataSavePath()
        {
            return Application.persistentDataPath + "/studio_rt/";
        }

        public static void CopyStructFieldValues<T>(T source, ref T target) where T : struct
        {
            Type structType = typeof(T);
            var fields = structType.GetFields(BindingFlags.Instance | BindingFlags.Public);
            foreach (var field in fields)
            {
                object value = field.GetValue(source);
                field.SetValueDirect(__makeref(target), value);
            }
        }

        public static string[] GetSfxClipNames()
        {
            var sfxClips = ResourceDB.LoadAllStudioAssetWithStringInPath<AudioClip>("sfx").ToArray();
            string[] names = new String[sfxClips.Length];
            for (int i = 0; i < sfxClips.Length; i++)
            {
                names[i] = sfxClips[i].name;
            }
            return names;
        }

        public static string GetSfxClipNameByIndex(int _index)
        {
            string[] names = GetSfxClipNames();
            if (_index < names.Length)
                return names[_index];
            
            return null;
        }

        public static string[] GetVfxClipNames()
        {
            var vfxClips = ResourceDB.LoadAllStudioAssetWithStringInPath<GameObject>("vfx").ToArray();
            string[] names = new string[vfxClips.Length];
            for (int i = 0; i < vfxClips.Length; i++)
            {
                names[i] = vfxClips[i].name;
            }
            return names;
        }

        public static string GetVfxClipNameByIndex(int _index)
        {
            string[] names = GetVfxClipNames();
            if (_index < names.Length) 
                return names[_index];
            return null;
        }
        
        public static float StringToFloat(string _value)
        {
            float result = 0f;
            Single.TryParse(_value, out result);
            return result;
        }

        public static int StringInInt(string _val)
        {
            int result = 0;
            int.TryParse(_val, out result);
            return result;
        }
        public static T DeepCopy<T>(T other)
        {
            var serializedData = JsonUtility.ToJson(other);
            return JsonUtility.FromJson<T>(serializedData);
        }
    }
}