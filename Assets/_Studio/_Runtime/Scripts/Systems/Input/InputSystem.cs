using UnityEngine;
using Leopotam.EcsLite;

namespace Terra.Studio
{
    public class InputSystem : IEcsRunSystem
    {
        private bool isPressedDown;
        private Camera mainCamera;

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
                var selection = GetSelection();
                RuntimeOp.Resolve<RuntimeSystem>().OnClicked?.Invoke(selection);
            }
        }

        private GameObject GetSelection()
        {
            if (Physics.Raycast(GetCamera().ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 100f))
            {
                return hit.transform.gameObject;
            }
            return null;
        }

        private Camera GetCamera()
        {
            if (!mainCamera)
            {
                mainCamera = Camera.main;
            }
            return mainCamera;
        }
    }
}