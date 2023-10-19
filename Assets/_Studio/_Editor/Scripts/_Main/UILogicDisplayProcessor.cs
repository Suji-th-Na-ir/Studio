using System;
using UnityEngine;
using RuntimeInspectorNamespace;
using System.Collections.Generic;

namespace Terra.Studio
{
    [Serializable]
    public struct ComponentDisplayDock
    {
        public GameObject ComponentGameObject;
        public string ComponentType;

    }
    public class UILogicDisplayProcessor : MonoBehaviour
    {
        private Dictionary<string, List<ComponentDisplayDock>> m_Broadcasters; //key: BroadcastString Value: GameObject/Component
        private Dictionary<string, List<ComponentDisplayDock>> m_Listners;

        private Dictionary<GameObject, List<ComponentIconNode>> m_icons;

        private List<ComponentDisplayDock> GetListnersInSceneFor(string broadcastString) => m_Listners.TryGetValue(broadcastString, out var value) ? value : new List<ComponentDisplayDock>();
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
            EditorOp.Resolve<SelectionHandler>().SelectionChanged += OnSelectionChanged;
            EditorOp.Resolve<RuntimeInspector>().OnPageIndexChanged += OnPageChanged;
            m_Broadcasters = new Dictionary<string, List<ComponentDisplayDock>>();
            m_Listners = new Dictionary<string, List<ComponentDisplayDock>>();
            m_icons = new Dictionary<GameObject, List<ComponentIconNode>>();
        }

        private void OnPageChanged(int index)
        {
            OnSelectionChanged(EditorOp.Resolve<SelectionHandler>().GetSelectedObjects());
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
            lastKey ??= "";
            if (list.TryGetValue(lastKey, out List<ComponentDisplayDock> value))
            {
                if (!string.IsNullOrEmpty(newKey) && newKey.Equals(lastKey))
                    return;
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
                    if (m_icons.TryGetValue(obj.ComponentGameObject, out var compIcons))
                    {
                        for (int j = 0; j < compIcons.Count; j++)
                        {
                            if (compIcons[j].GetComponentDisplayDockTarget().Equals(obj))
                            {
                                compIcons[j].IsListning = false;
                                compIcons[j].BroadcastingStrings.Clear();
                                compIcons[j].ListenStrings.Clear();
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

        public void UpdateListenerString(string newString, string lastString, ComponentDisplayDock obj)
        {
            UpdateDictionaryWithNewStringKey(m_Listners, lastString, newString, obj);
            ValidateBroadcastListen();
        }


        private void ValidateBroadcastListen()
        {
            foreach (var icons in m_icons)
            {
                var comp = icons.Value;
                for (int i = 0; i < comp.Count; i++)
                {
                    comp[i]?.ListnerTargets?.Clear();
                    comp[i]?.BroadcastTargets?.Clear();
                    comp[i]?.BroadcastingStrings.Clear();
                    comp[i]?.ListenStrings.Clear();

                }
            }
            foreach (var broadcast in m_Broadcasters)
            {
                var listners = GetListnersInSceneFor(broadcast.Key);
                var allbroadCastObject = broadcast.Value;

                List<ComponentIconNode> broadcastNode = new List<ComponentIconNode>();
                for (int i = 0; i < allbroadCastObject.Count; i++)
                {
                    if (m_icons.TryGetValue(allbroadCastObject[i].ComponentGameObject, out var compIcons))
                    {
                        for (int j = 0; j < compIcons.Count; j++)
                        {
                            if (compIcons[j].GetComponentDisplayDockTarget().Equals(allbroadCastObject[i]))
                            {
                                broadcastNode.Add(compIcons[j]);
                                compIcons[j].ListnerTargets = GetTargetIconsForDisplayDock(listners);
                                if (broadcast.Key == "Game Win")
                                {
                                    compIcons[j].m_isBroadcatingGameWon = true;
                                    compIcons[j].m_isBroadcatingGameLoose = false;
                                }
                                else if (broadcast.Key == "Game Lose")
                                {
                                    compIcons[j].m_isBroadcatingGameWon = false;
                                    compIcons[j].m_isBroadcatingGameLoose = true;
                                }
                                else
                                {
                                    compIcons[j].m_isBroadcatingGameWon = false;
                                    compIcons[j].m_isBroadcatingGameLoose = false;
                                }
                                compIcons[j].BroadcastingStrings = new List<string> { broadcast.Key };
                            }
                        }
                    }
                }


            }

            foreach (var listner in m_Listners)
            {
                var broadcasters = GetBroadcastersInSceneFor(listner.Key);
                var allListnerObject = listner.Value;
                for (int i = 0; i < allListnerObject.Count; i++)
                {
                    if (m_icons.TryGetValue(allListnerObject[i].ComponentGameObject, out var compIcons))
                    {
                        for (int j = 0; j < compIcons.Count; j++)
                        {
                            if (compIcons[j].GetComponentDisplayDockTarget().Equals(allListnerObject[i]))
                            {
                                compIcons[j].BroadcastTargets = GetTargetIconsForDisplayDock(broadcasters);
                                compIcons[j].IsListning = true;
                                compIcons[j].ListenStrings = new List<string> { listner.Key };
                            }
                        }
                    }
                }
            }


            OnSelectionChanged(EditorOp.Resolve<SelectionHandler>().GetSelectedObjects());
        }

        private List<ComponentIconNode> GetTargetIconsForDisplayDock(List<ComponentDisplayDock> docks)
        {
            List<ComponentIconNode> result = new List<ComponentIconNode>();

            for (int i = 0; i < docks.Count; i++)
            {
                if (m_icons.ContainsKey(docks[i].ComponentGameObject))
                {
                    if (m_icons[docks[i].ComponentGameObject] != null)
                    {
                        var icons = m_icons[docks[i].ComponentGameObject];
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
            GameObject iconGameObject = new GameObject($"Icon{componentDisplay.ComponentGameObject.name}_{componentDisplay.ComponentType}");
            iconGameObject.transform.SetParent(EditorOp.Resolve<VisualisationView>().transform, false);
            var compIcon = iconGameObject.AddComponent<ComponentIconNode>();
            compIcon.Setup(componentDisplay);

            if (!m_icons.TryGetValue(componentDisplay.ComponentGameObject, out List<ComponentIconNode> value))
            {
                if (m_icons.ContainsKey(componentDisplay.ComponentGameObject))
                    m_icons[componentDisplay.ComponentGameObject].Add(compIcon);
                else
                {
                    List<ComponentIconNode> componentIconNodes = new List<ComponentIconNode>() { compIcon, };
                    m_icons.Add(componentDisplay.ComponentGameObject, componentIconNodes);

                }
            }
            else
            {
                m_icons[componentDisplay.ComponentGameObject].Add(compIcon);
            }

            for (int i = 0; i < m_icons[componentDisplay.ComponentGameObject].Count; i++)
            {
                m_icons[componentDisplay.ComponentGameObject][i].m_componentIndex = i;
            }
        }

        public void ImportVisualisation(GameObject gameObj, string component, string broadcast, string broadcastListen)
        {
            bool newcomponent = true;
            var compdock = new ComponentDisplayDock() { ComponentGameObject = gameObj, ComponentType = component };

            if (m_icons.ContainsKey(gameObj))
            {
                if (m_icons.TryGetValue(gameObj, out var value))
                {
                    foreach (var v in value)
                    {
                        if (v.GetComponentDisplayDockTarget().Equals(compdock))
                        {
                            newcomponent = false;
                            break;
                        }
                    }

                }
            }

            if (newcomponent)
            {
                AddComponentIcon(compdock);
            }

            UpdateBroadcastString(broadcast, "", compdock);
            UpdateListenerString(broadcastListen, "", compdock);
        }

        private void RemoveIcon(ComponentDisplayDock componentDisplay)
        {
            List<ComponentIconNode> value;
            List<ComponentIconNode> toRemove = new List<ComponentIconNode>();
            if (!m_icons.TryGetValue(componentDisplay.ComponentGameObject, out value))
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

            if (m_icons[componentDisplay.ComponentGameObject].Count == 0)
                m_icons.Remove(componentDisplay.ComponentGameObject);


            //Remove all broadcasters and listner
            foreach (var broadcast in m_Broadcasters)
            {
                if (broadcast.Value.Contains(componentDisplay))
                    broadcast.Value.Remove(componentDisplay);
            }

            foreach (var listner in m_Listners)
            {
                if (listner.Value.Contains(componentDisplay))
                    listner.Value.Remove(componentDisplay);
            }

        }

        private void OnSelectionChanged(List<GameObject> selection)
        {
            if (selection == null || selection.Count == 0)
            {
                foreach (var item in m_icons)
                {
                    for (int i = 0; i < item.Value.Count; i++)
                    {
                        item.Value[i].isTargetSelected = true;
                    }
                }
                return;
            }
            foreach (var item in m_icons)
            {
                for (int i = 0; i < item.Value.Count; i++)
                {
                    item.Value[i].isTargetSelected = false;
                }
            }



            foreach (var s in selection)
            {
                if (m_icons.TryGetValue(s.gameObject, out var value))
                {

                    for (int i = 0; i < value.Count; i++)
                    {
                        value[i].isTargetSelected = true;

                        if (value[i].ListnerTargets != null)
                            foreach (var l in value[i].ListnerTargets)
                            {
                                l.isTargetSelected = true;
                            }

                        if (value[i].BroadcastTargets != null)
                            foreach (var b in value[i].BroadcastTargets)
                            {
                                b.isTargetSelected = true;
                            }
                    }

                }
            }



        }
    }
}
