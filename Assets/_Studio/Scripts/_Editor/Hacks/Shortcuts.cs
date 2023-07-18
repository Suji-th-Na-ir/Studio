using UnityEngine;
using UnityEngine.EventSystems;

namespace Terra.Studio
{
    public class Shortcuts : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                if (Input.GetKeyDown(KeyCode.N))
                {
                    var go = StudioGameObject.CreateGameObject(PrimitiveType.Cube);
                    Interop<EditorInterop>.Current.Resolve<SelectionManager>().OnSelected(go);
                }

                if (Input.GetKeyDown(KeyCode.P))
                {
                    Interop<SystemInterop>.Current.Resolve<System>().SwitchState();
                }
            }

            if (Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                //if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 100f))
                //{
                //    var gameObject = Interop<EditorInterop>.Current.Resolve<StudioGameObjectsHolder>().Resolve(hit.transform.gameObject);
                //    Interop<EditorInterop>.Current.Resolve<SelectionManager>().OnSelected(gameObject);
                //}
                //else
                //{
                //    Interop<EditorInterop>.Current.Resolve<SelectionManager>().OnSelected(null);
                //}
            }
        }
    }
}
