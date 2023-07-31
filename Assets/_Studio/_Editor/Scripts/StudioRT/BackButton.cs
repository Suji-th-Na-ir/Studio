using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Terra.Studio
{
    public class BackButton : MonoBehaviour
    {
        public void BackButtonClicked()
        {
            RuntimeOp.Resolve<RuntimeSystem>().RequestSwitchState();
        }
    }
}
