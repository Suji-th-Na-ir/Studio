using UnityEngine;
using PlayShifu.Terra;
using System;

namespace Terra.Studio
{
    public class GameData
    {
        public Vector3 RespawnPoint;
        public Transform PlayerRef;
        public GameEndState EndState;
        Action OnNewWeaponEquip;

        private Animator playerAnimator;
        public Animator PlayerAnimator
        {
            get
            {
                if (!playerAnimator)
                    playerAnimator = PlayerRef.GetComponentInChildren<Animator>();

                return playerAnimator;
            }
        }

        public void SetEndState(string state)
        {
            if (state.Equals("Game Win"))
            {
                EndState = GameEndState.Win;
            }
            else
            {
                EndState = GameEndState.Lose;
            }
        }

        public void SetPlayerPosition(Vector3 position)
        {
            if (PlayerRef)
            {
                PlayerRef.position = position;
            }
        }

        public void SetMeleeWeaponParentTransform(Transform weapon, Action onWeaponEquip)
        {    
           var parent= Helper.FindDeepChild(PlayerRef, "MeleeWeaponParent");
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                child.gameObject.tag = "Untagged";
                child.transform.position = RuntimeOp.Resolve<GameData>().PlayerRef.position + Vector3.up * 2f + Vector3.right * 2f;
               
                child.transform.SetParent(null);
            }
            OnNewWeaponEquip?.Invoke();
            OnNewWeaponEquip = onWeaponEquip;

            weapon.SetParent(parent);
            weapon.position = parent.position + parent.forward;
            weapon.localRotation = Quaternion.identity;
            weapon.gameObject.tag = "Damager";
           
        }

        public void ExecutePlayerMeleeAttack(GameObject weapon)
        {
            weapon.GetComponent<Collider>().isTrigger = true;
            PlayerAnimator.SetTrigger("Attack");
            PlayerAnimator.GetBehaviour<MeleeStateBehaviour>().OnExit = () =>
            {
                weapon.GetComponent<Collider>().isTrigger = false;
            };
        }
    }
}
