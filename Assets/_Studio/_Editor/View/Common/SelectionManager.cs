using System;
using UnityEngine;

namespace Terra.Studio
{
    public class SelectionManager : MonoBehaviour
    {
        public event Action<StudioGameObject> onSelectionOccured;
        private string currentSelectedObject;

        private void Awake()
        {
            Interop<EditorInterop>.Current.Register(this);
        }

        public void OnSelected(in StudioGameObject gameObject)
        {
            if (!string.IsNullOrEmpty(currentSelectedObject))
            {
                if (gameObject != null && currentSelectedObject.Equals(gameObject.Id))
                {
                    return;
                }
            }
            onSelectionOccured?.Invoke(gameObject);
            currentSelectedObject = gameObject?.Id ?? null;
        }
    }
}