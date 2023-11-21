using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Terra.Studio
{
    public class PlayerData
    {
        private const int DEFAULT_MAX_PLAYER_HEALTH = 100;
        private const int BUFFER_BEFORE_REGEN_STARTS = 3;

        private Transform playerRef;
        private float playerHealth;
        private float regenerationValue;
        private string broadcastOnPlayerDied;
        private CoroutineService currentActiveCoroutine;

        public float PlayerHealth => playerHealth;
        public event Action<float> OnPlayerHealthModified;

        public void SpawnPlayer()
        {
            var playerObj = (GameObject)RuntimeOp.Load(ResourceTag.Player);
            var reference = Object.Instantiate(playerObj);
            reference.transform.position = RuntimeOp.Resolve<GameData>().RespawnPoint;
            playerRef = reference.transform;
            playerHealth = DEFAULT_MAX_PLAYER_HEALTH;
        }

        public void SetPlayerPosition(Vector3 position)
        {
            if (playerRef)
            {
                playerRef.position = position;
            }
        }

        public void UpdatePlayerRegenerationTime(float regenerationValue)
        {
            this.regenerationValue = regenerationValue;
        }

        public void ModifyPlayerHealth(int modifier)
        {
            StopRegeneration();
            ChangeHealthWithClamp(modifier);
            if (playerHealth != 0)
            {
                InitiateRecovery();
            }
            else
            {
                var canBroadcast = !string.IsNullOrEmpty(broadcastOnPlayerDied);
                if (canBroadcast)
                {
                    RuntimeOp.Resolve<Broadcaster>().Broadcast(broadcastOnPlayerDied);
                }
            }
        }

        private void ChangeHealthWithClamp(float modifier)
        {
            playerHealth += modifier;
            playerHealth = (int)Mathf.Clamp(playerHealth, 0f, DEFAULT_MAX_PLAYER_HEALTH);
            OnPlayerHealthModified?.Invoke(playerHealth);
        }

        private void InitiateRecovery()
        {
            currentActiveCoroutine = CoroutineService.RunCoroutine(() =>
            {
                InitiateRegeneration();
            },
            CoroutineService.DelayType.WaitForXSeconds, BUFFER_BEFORE_REGEN_STARTS);
        }

        private void InitiateRegeneration()
        {
            currentActiveCoroutine = CoroutineService.RunCoroutine(() =>
            {
                ChangeHealthWithClamp(regenerationValue);
            },
            CoroutineService.DelayType.UntilPredicateFailed, predicate: CanRegenerateMore);
        }

        private void StopRegeneration()
        {
            if (currentActiveCoroutine)
            {
                currentActiveCoroutine.Stop();
                currentActiveCoroutine = null;
            }
        }

        private bool CanRegenerateMore()
        {
            var canRegenerate = playerHealth == DEFAULT_MAX_PLAYER_HEALTH;
            return canRegenerate;
        }

        public void UpdateBroadcastValueOnPlayerDied(string broadcast)
        {
            broadcastOnPlayerDied = broadcast;
        }
    }
}