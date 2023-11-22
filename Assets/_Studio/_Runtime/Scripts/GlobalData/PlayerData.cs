using System;
using UnityEngine;
using PlayShifu.Terra;
using Object = UnityEngine.Object;

namespace Terra.Studio
{
    public class PlayerData
    {
        private Transform playerRef;
        private Camera playerCamera;
        private Animator playerAnimator;
        private Action unequipOlderWeapon;

        public Camera PlayerCamera => playerCamera;
        public Transform PlayerRef => playerRef;
        public Action<int> OnPlayerHealthChangeRequested;

        public void SpawnPlayer()
        {
            var playerObj = (GameObject)RuntimeOp.Load(ResourceTag.Player);
            var reference = Object.Instantiate(playerObj);
            reference.transform.position = RuntimeOp.Resolve<GameData>().RespawnPoint;
            playerRef = reference.transform;
            playerAnimator = playerRef.GetComponentInChildren<Animator>();
            playerCamera = playerRef.GetComponentInChildren<Camera>();
        }

        public void SetPlayerPosition(Vector3 position)
        {
            if (playerRef)
            {
                playerRef.position = position;
            }
        }

        public void SetMeleeWeaponParentTransform(Transform weapon, Action unequipHandle)
        {
            var parent = Helper.FindDeepChild(playerRef, "MeleeWeaponParent");
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                child.gameObject.tag = "Untagged";
                child.transform.position = playerRef.position + Vector3.up * 2f + Vector3.right * 2f;
                child.transform.SetParent(null);
            }
            unequipOlderWeapon?.Invoke();
            unequipOlderWeapon = unequipHandle;
            weapon.SetParent(parent);
            weapon.position = parent.position + parent.forward;
            weapon.localRotation = Quaternion.identity;
            weapon.gameObject.tag = "Damager";
        }

        public void ExecutePlayerMeleeAttack(GameObject weapon)
        {
            weapon.GetComponent<Collider>().isTrigger = true;
            playerAnimator.SetTrigger("Attack");
            playerAnimator.GetBehaviour<MeleeStateBehaviour>().OnExit = () =>
            {
                weapon.GetComponent<Collider>().isTrigger = false;
            };
        }
    }
}