using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEngine;
using static UnityEditor.Progress;

namespace Terra.Studio
{
    [Serializable]
    public struct ComponentDisplayDock
    {
        public GameObject componentGameObject;
        public string componentType;

    }
    public class UILogicDisplayProcessor : MonoBehaviour
    {
        private Dictionary<string, List<ComponentDisplayDock>> m_Broadcasters; //key: BroadcastString Value: GameObject/Component
        private Dictionary<string, List<ComponentDisplayDock>> m_Listners;
        [SerializeField] ComponentIconsPreset iconPresets;

        private Dictionary<GameObject, List<ComponentIconNode>> m_icons;

        private List<ComponentDisplayDock> GetListnersInSceneFor(string broadcastString) => m_Listners.TryGetValue(broadcastString, out var value) ? value :new List<ComponentDisplayDock>();
        private List<ComponentDisplayDock> GetBroadcastersInSceneFor(string listenString) => m_Broadcasters.TryGetValue(listenString, out var value) ? value : new List<ComponentDisplayDock>();


        private void Awake()
        {
            EditorOp.Register(this);
        }

        private void OnDestroy()
        {
            EditorOp.Unregister(this);
        }

        public void Init()
        {
            m_Broadcasters = new Dictionary<string, List<ComponentDisplayDock>>();
            m_Listners = new Dictionary<string, List<ComponentDisplayDock>>();
            m_icons = new Dictionary<GameObject, List<ComponentIconNode>>();
        }

        public void AddComponentIcon(ComponentDisplayDock obj)
        {
            AddIcon(obj);
            ValidateBroadcastListen();
        }

        public void RemoveComponentIcon(ComponentDisplayDock obj)
        {
            RemoveIcon(obj);
            ValidateBroadcastListen();
        }

        private void UpdateDictionaryWithNewStringKey(Dictionary<string, List<ComponentDisplayDock>> list, string lastKey, string newKey, ComponentDisplayDock obj)
        {
            if (lastKey == null)
                lastKey = "";
            if (list.TryGetValue(lastKey, out List<ComponentDisplayDock> value))
            {
                if (!string.IsNullOrEmpty(newKey))
                {
                    if (list.TryGetValue(newKey, out List<ComponentDisplayDock> newValueList))
                    {
                        if (!newValueList.Contains(obj))
                        {
                            newValueList.Add(obj);
                        }
                    }
                    else
                    {
                        newValueList = new List<ComponentDisplayDock> { obj };
                        list.Add(newKey, newValueList);
                    }

                    value.Remove(obj); // Remove from old key's list
                }
                else
                {
                    if (m_icons.TryGetValue(obj.componentGameObject, out var compIcons))
                    {
                        for (int j = 0; j < compIcons.Count; j++)
                        {
                            if (compIcons[j].GetComponentDisplayDockTarget().Equals(obj))
                            {
                                compIcons[j].m_isBroadcating = false;
                            }
                        }
                    }
                    value.Remove(obj);

                }

                if (value.Count == 0)
                {
                    list.Remove(lastKey); // Remove old key if its list is empty
                }
            }
            else
            {
                if (string.IsNullOrEmpty(newKey))
                    return;
                if (list.TryGetValue(newKey, out List<ComponentDisplayDock> newValueList))
                {
                    if (!newValueList.Contains(obj))
                    {
                        newValueList.Add(obj);
                    }
                }
                else
                {
                    newValueList = new List<ComponentDisplayDock> { obj };
                    list.Add(newKey, newValueList);
                }
            }
           
        }

        public void UpdateBroadcastString(string newString, string lastString, ComponentDisplayDock obj)
        {
        
            UpdateDictionaryWithNewStringKey(m_Broadcasters, lastString, newString, obj);
            ValidateBroadcastListen();
        }

        public void UpdateListnerString(string newString, string lastString, ComponentDisplayDock obj)
        {
            UpdateDictionaryWithNewStringKey(m_Listners, lastString, newString, obj);
            ValidateBroadcastListen();
        }


        private void ValidateBroadcastListen()
        {
            foreach (var broadcast in m_Broadcasters)
            {
                var listners = GetListnersInSceneFor(broadcast.Key);
                var allbroadCastObject = broadcast.Value;
                for (int i = 0; i < allbroadCastObject.Count; i++)
                {
                    if (m_icons.TryGetValue(allbroadCastObject[i].componentGameObject, out var compIcons))
                    {
                        for (int j = 0; j < compIcons.Count; j++)
                        {
                            if (compIcons[j].GetComponentDisplayDockTarget().Equals(allbroadCastObject[i]))
                            {
                                compIcons[j].ListnerTargets = GetTargetIconsForDisplayDock(listners);
                                compIcons[j].m_isBroadcating = true;
                                if (broadcast.Key == "Game Win")
                                    compIcons[j].m_isBroadcatingGameWon = true;
                                else
                                    compIcons[j].m_isBroadcatingGameWon = false;
                            }
                        }
                    }
                }
            }

        }

        private List<ComponentIconNode> GetTargetIconsForDisplayDock(List<ComponentDisplayDock> docks)
        {
            List<ComponentIconNode> result = new List<ComponentIconNode>();

            for (int i = 0; i < docks.Count; i++)
            {
                if (m_icons.ContainsKey(docks[i].componentGameObject))
                {
                    if (m_icons[docks[i].componentGameObject]!=null )
                    {
                        var icons=m_icons[docks[i].componentGameObject];
                        for (int j = 0; j < icons.Count; j++)
                        {
                            if (icons[j].GetComponentDisplayDockTarget().Equals(docks[i]))
                            {
                                result.Add(icons[j]);
                            }
                        }
                    }
                }
            }
            return result;
        }

        private void AddIcon(ComponentDisplayDock componentDisplay)
        {
            GameObject iconGameObject = new GameObject($"Icon{componentDisplay.componentGameObject.name}_{componentDisplay.componentType}");
         
            var iconSprite = iconPresets.GetIcon(componentDisplay.componentType);
            var broadcastSprite = iconPresets.GetIcon("Broadcast");
            var broadcastNoListnerSprite = iconPresets.GetIcon("BroadcastNoListner");
            var gameWonBroadcastSprite = iconPresets.GetIcon("GameWon");
            var compIcon = iconGameObject.AddComponent<ComponentIconNode>();
            compIcon.Setup(iconSprite,broadcastSprite,broadcastNoListnerSprite,gameWonBroadcastSprite, componentDisplay);
            if (!m_icons.TryGetValue(componentDisplay.componentGameObject, out List<ComponentIconNode> value))
            {
                if (m_icons.ContainsKey(componentDisplay.componentGameObject))
                    m_icons[componentDisplay.componentGameObject].Add(compIcon);
                else
                {
                    List<ComponentIconNode> componentIconNodes = new List<ComponentIconNode>() { compIcon, };
                    m_icons.Add(componentDisplay.componentGameObject, componentIconNodes);
                   
                }
            }
            else
            {
                m_icons[componentDisplay.componentGameObject].Add(compIcon);
               

            }

            for (int i = 0; i < m_icons[componentDisplay.componentGameObject].Count; i++)
            {
                m_icons[componentDisplay.componentGameObject][i].m_componentIndex = i;
            }
        }

        public void ImportVisualisation(GameObject gameObj, string component, string broadcast, string broadcastListen)
        {
            AddComponentIcon(new ComponentDisplayDock() { componentGameObject = gameObj, componentType = component });
            UpdateBroadcastString(broadcast, "", new ComponentDisplayDock() { componentGameObject = gameObj, componentType = component });
            UpdateListnerString(broadcastListen, "", new ComponentDisplayDock() { componentGameObject = gameObj, componentType = component });
        }

        private void RemoveIcon(ComponentDisplayDock componentDisplay)
        {
            List<ComponentIconNode> value;
            List<ComponentIconNode> toRemove=new List<ComponentIconNode>();
            if (!m_icons.TryGetValue(componentDisplay.componentGameObject, out value))
                return;

            for (int i = 0; i < value.Count; i++)
            {
                if (value[i].GetComponentDisplayDockTarget().Equals(componentDisplay))
                {
                    value[i].DestroyThisIcon();
                    toRemove.Add(value[i]); 
                }
            }
            foreach (var remove in toRemove)
            {
                value.Remove(remove);
            }
           
            if (m_icons[componentDisplay.componentGameObject].Count==0)
            m_icons.Remove(componentDisplay.componentGameObject);


            //Remove all broadcasters and listner
            foreach (var broadcast in m_Broadcasters)
            {
                if (broadcast.Value.Contains(componentDisplay))
                    broadcast.Value.Remove(componentDisplay);
            }

            foreach (var listner  in m_Listners)
            {
                if (listner.Value.Contains(componentDisplay))
                    listner.Value.Remove(componentDisplay);
            }

        }
    }
}
