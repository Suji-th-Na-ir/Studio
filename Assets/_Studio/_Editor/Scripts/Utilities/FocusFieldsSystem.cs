using System;
using System.Collections.Generic;
using RuntimeInspectorNamespace;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Terra.Studio
{
    public struct FocusedGameObject
    {
        public GameObject fGameObject;
        public Action OnSelect;
        public Action OnDeSelect;
    }
    public class FocusFieldsSystem : IDisposable
    {
        public readonly List<FocusedGameObject> focusedGameObjects = new List<FocusedGameObject>();

        private FocusedGameObject currentFocusedGameobject;
        public GameObject CurrentFocuedGameobject { get { return currentFocusedGameobject.fGameObject; } }

        public GameObject LastFocusedGameObject
        {
            get
            {
                if (currentFocusedGameobject.fGameObject == null)
                    return null;
                var index = focusedGameObjects.IndexOf(currentFocusedGameobject) - 1;
                if (index >= 0 && index < focusedGameObjects.Count)
                {
                    return focusedGameObjects[index].fGameObject;
                }
                else if (index > focusedGameObjects.Count)
                {
                    return focusedGameObjects[0].fGameObject;
                }
                else
                {
                    return focusedGameObjects[focusedGameObjects.Count - 1].fGameObject;
                }

            }
        }

        public GameObject NextFocusedGameObject
        {
            get
            {
                if (currentFocusedGameobject.fGameObject == null)
                    return null;
                var index = focusedGameObjects.IndexOf(currentFocusedGameobject) + 1;
                if (index < focusedGameObjects.Count)
                {
                    return focusedGameObjects[index].fGameObject;
                }
                return focusedGameObjects[0].fGameObject;

            }
        }
        public void AddFocusedGameobjects(GameObject gm, Action onSelect = null, Action onDeselect = null)
        {
            var selectable = gm.GetComponent<Selectable>();
            if (selectable)
            {
                FocusedGameObject fgo = new FocusedGameObject() { fGameObject = gm, OnSelect = onSelect, OnDeSelect = onDeselect };
                if (!focusedGameObjects.Contains(fgo))
                    focusedGameObjects.Add(fgo);
                else
                    return;
                var pointer = selectable.GetComponent<PointerEventListener>();
                if (!pointer)
                    pointer = selectable.gameObject.AddComponent<PointerEventListener>();
                pointer.PointerDown += SetCurrentFocusedGameobject;
            }
        }

        public void RemoveCurrentSelected()
        {
            currentFocusedGameobject.OnDeSelect?.Invoke();
            currentFocusedGameobject = default;
        }

        public void RemoveFocusedGameObjects(GameObject gm)
        {
            focusedGameObjects.Remove(GetFocusedObjectWithSameGO(gm));
        }

        public void SelectFocusedGameObject(GameObject gm)
        {
            if (CurrentFocuedGameobject != null)
            {
                var dropDown = CurrentFocuedGameobject.GetComponent<Dropdown>();
                if (dropDown != null)
                {
                    dropDown.Hide();
                }
            }
            var pointerData = new PointerEventData(EventSystem.current);
            pointerData.selectedObject = gm;
            SetCurrentFocusedGameobject(pointerData);
        }

        private void SetCurrentFocusedGameobject(PointerEventData data)
        {
            if (CurrentFocuedGameobject == data.selectedObject)
                return;

            ExecuteEvents.Execute(CurrentFocuedGameobject, data, ExecuteEvents.deselectHandler);
            currentFocusedGameobject.OnDeSelect?.Invoke();

            currentFocusedGameobject = GetFocusedObjectWithSameGO(data.selectedObject);
            ExecuteEvents.Execute(CurrentFocuedGameobject, data, ExecuteEvents.selectHandler);
            currentFocusedGameobject.OnSelect?.Invoke();
        }

        private FocusedGameObject GetFocusedObjectWithSameGO(GameObject go)
        {
            for (int i = 0; i < focusedGameObjects.Count; i++)
            {
                if (focusedGameObjects[i].fGameObject == go)
                {
                    return focusedGameObjects[i];
                }
            }
            return default;
        }

        public void AddAfterGameObject(GameObject lastGm, GameObject newGm, Action onSelect = null, Action onDeselect = null)
        {
            var selectable = lastGm.GetComponent<Selectable>();
            if (selectable)
            {
                FocusedGameObject fgo = new FocusedGameObject() { fGameObject = newGm, OnSelect = onSelect, OnDeSelect = onDeselect };
                int insertIndex = focusedGameObjects.IndexOf(GetFocusedObjectWithSameGO(lastGm));

                if (insertIndex != -1) // Check if the element was found in the list.
                {
                    if (!focusedGameObjects.Contains(fgo))
                        focusedGameObjects.Insert(insertIndex + 1, fgo);
                    else
                        return;
                }
                var pointer = selectable.GetComponent<PointerEventListener>();
                if (!pointer)
                    pointer = selectable.gameObject.AddComponent<PointerEventListener>();
                selectable.GetComponent<PointerEventListener>().PointerDown += SetCurrentFocusedGameobject;
            }
        }

        public void Dispose()
        {
            focusedGameObjects.Clear();
        }
    }
}
