using UnityEngine;
using Leopotam.EcsLite;
using UnityEngine.SceneManagement;

namespace Terra.Studio
{
    public class ClickSystem : IEcsRunSystem
    {
        private bool isPressedDown;

        public void Run(IEcsSystems systems)
        {
            if (Input.GetMouseButtonDown(0))
            {
                isPressedDown = true;
            }
            if (Input.GetMouseButtonUp(0))
            {
                if (!isPressedDown)
                {
                    return;
                }
                isPressedDown = false;
                Interop<RuntimeInterop>.Current.Resolve<RuntimeSystem>().OnClicked?.Invoke(GetSelectionPhysicsScene());
            }
        }

        private GameObject GetSelection()
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 100f))
            {
                return hit.transform.gameObject;
            }
            return null;
        }

        private GameObject GetSelectionPhysicsScene()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            PhysicsScene pScene = SceneManager.GetActiveScene().GetPhysicsScene();
            if (pScene.Raycast(worldPoint, ray.direction, out var hit, float.MaxValue))
            {
                return hit.collider.gameObject;
            }
            return null;
        }
    }
}