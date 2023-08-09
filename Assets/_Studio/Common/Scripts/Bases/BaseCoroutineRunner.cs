using UnityEngine;

namespace Terra.Studio
{
    public class BaseCoroutineRunner : MonoBehaviour
    {
        protected virtual void Start()
        {
            RuntimeOp.Resolve<RuntimeSystem>().AddCoroutineRunner(true, this);
        }

        protected virtual void OnDestroy()
        {
            if (RuntimeOp.Resolve<RuntimeSystem>())
            {
                RuntimeOp.Resolve<RuntimeSystem>().AddCoroutineRunner(false, this);
            }
        }
    }
}
