using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeStateBehaviour : StateMachineBehaviour
{
   public Action OnExit;
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        OnExit?.Invoke();
    }
}
