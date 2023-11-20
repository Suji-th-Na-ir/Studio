using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Terra.Studio
{
    public class PlayerData
    {
        private const int DEFAULT_MAX_PLAYER_HEALTH = 100;
        private Transform playerRef;
        private int playerHealth;
        public int PlayerHealth => playerHealth;
        public event Action<int> OnPlayerHealthModified;

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

        public void ModifyPlayerHealth(int modifier)
        {
            playerHealth += modifier;
            playerHealth = (int)Mathf.Clamp(playerHealth, 0f, DEFAULT_MAX_PLAYER_HEALTH);
            OnPlayerHealthModified?.Invoke(playerHealth);
        }
    }
}