using System;
using System.Collections;
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

        private FocusedGameObject lastFocusedGameObject;
        public GameObject LastFocusedGameObject
        {
            get
            {
                if (!lastFocusedGameObject.fGameObject)
                {
                    return currentFocusedGameobject.fGameObject;
                }
                return lastFocusedGameObject.fGameObject;
            }
        }

        public GameObject NextFocusedGameObject
        {
            get
            {
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
                selectable.GetComponent<PointerEventListener>().PointerDown += SetCurrentFocusedGameobject;
            }
        }

        public void RemoveFocusedGameObjects(GameObject gm)
        {
            focusedGameObjects.Remove(GetFocusedObjectWithSameGO(gm));
        }

        public void SelectFocusedGameObject(GameObject gm)
        {
            var pointerData = new PointerEventData(EventSystem.current);
            pointerData.selectedObject = gm;
            SetCurrentFocusedGameobject(pointerData);
        }

        private void SetCurrentFocusedGameobject(PointerEventData data)
        {
            if (CurrentFocuedGameobject == data.selectedObject)
                return;
            lastFocusedGameObject = currentFocusedGameobject;

            ExecuteEvents.Execute(CurrentFocuedGameobject, data, ExecuteEvents.deselectHandler);
            lastFocusedGameObject.OnDeSelect?.Invoke();

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
            return new FocusedGameObject();
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

        public void ClearFocusedGameobjects()
        {
            focusedGameObjects.Clear();
        }

        public void Dispose()
        {
            focusedGameObjects.Clear();
        }
    }
}
