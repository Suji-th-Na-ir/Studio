using UnityEngine;
using Leopotam.EcsLite;

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
                var selection = GetSelection();
                RuntimeOp.Resolve<RuntimeSystem>().OnClicked?.Invoke(selection);
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
    }
}