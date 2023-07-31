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
                    EditorOp.Resolve<SelectionManager>().OnSelected(go);
                }
            }
        }
    }
}
