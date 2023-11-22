using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using PlayShifu.Terra;
using Leopotam.EcsLite;

namespace Terra.Studio
{
    public class PlayerHealthSystem : BaseSystem, IEcsRunSystem
    {
        private const string PLAYER_HEALTH_UI = "DynamicUI/HealthBar";
        private const string SLIDER_LOC = "Slider";
        private const int DEFAULT_MAX_PLAYER_HEALTH = 100;
        private const int BUFFER_BEFORE_REGEN_STARTS = 5;
        private const float HEALTH_REGENERATION_TIME = 1f;
        private const float HEALTH_MODIFICATION_ANIM_TIME = 2f;

        private int entity;
        private Camera camera;
        private Transform player;
        private CoroutineService healthModificationCoroutine;
        private CoroutineService healthBarAnimationCoroutine;

        public override void Init<T>(int entity)
        {
            this.entity = entity;
            base.Init<T>(entity);
        }

        public override void OnConditionalCheck(int entity, object _)
        {
            ref var entityRef = ref entity.GetComponent<PlayerHealthComponent>();
            entityRef.playerHealth = DEFAULT_MAX_PLAYER_HEALTH;
            entityRef.CanExecute = true;
            SpawnUI(ref entityRef, entity);
        }

        private void SpawnUI(ref PlayerHealthComponent component, int entity)
        {
            var go = RuntimeOp.Load<GameObject>(PLAYER_HEALTH_UI);
            var ui = RuntimeOp.Resolve<View>().AttachDynamicUI(nameof(PlayerHealthComponent), go);
            component.slider = Helper.FindDeepChild<Image>(ui.transform, SLIDER_LOC);
            component.healthUI = ui;
            component.trackUI = true;
            RuntimeOp.Resolve<PlayerData>().OnPlayerHealthChangeRequested += OnHealthModifyRequested;
            player = RuntimeOp.Resolve<PlayerData>().PlayerRef;
            camera = RuntimeOp.Resolve<PlayerData>().PlayerCamera;
        }

        public void Run(IEcsSystems systems)
        {
            var component = entity.GetComponent<PlayerHealthComponent>();
            if (!component.trackUI || !component.healthUI) return;
            var playerPos = player.transform.position;
            playerPos.y += 2f;
            var screenPos = camera.WorldToScreenPoint(playerPos);
            component.healthUI.transform.position = new Vector3(screenPos.x, screenPos.y, 0);
        }

        public override void OnHaltRequested(EcsWorld currentWorld)
        {
            StopAllCoroutines();
            ref var entityRef = ref entity.GetComponent<PlayerHealthComponent>();
            if (!entityRef.CanExecute)
            {
                var compsData = RuntimeOp.Resolve<ComponentsData>();
                compsData.ProvideEventContext(false, entityRef.EventContext);
            }
            if (entityRef.trackUI && entityRef.healthUI)
            {
                entityRef.trackUI = false;
                Object.Destroy(entityRef.healthUI);
                RuntimeOp.Resolve<PlayerData>().OnPlayerHealthChangeRequested -= OnHealthModifyRequested;
            }
        }

        private void OnHealthModifyRequested(int modifier)
        {
            StopAllCoroutines();
            ChangeHealthWithClamp(modifier, false);
            ref var entityRef = ref entity.GetComponent<PlayerHealthComponent>();
            if (entityRef.playerHealth != 0)
            {
                InitiateRecovery();
            }
            else
            {
                var canRespawn = true;
                if (entityRef.IsBroadcastable)
                {
                    var broadcast = entityRef.Broadcast;
                    RuntimeOp.Resolve<Broadcaster>().Broadcast(broadcast);
                    canRespawn = !RuntimeOp.Resolve<Broadcaster>().CORE_BROADCAST_KEYS.Any(x => x.Equals(broadcast));
                }
                if (canRespawn)
                {
                    var respawnPoint = RuntimeOp.Resolve<GameData>().RespawnPoint;
                    RuntimeOp.Resolve<PlayerData>().SetPlayerPosition(respawnPoint);
                    ForceSetSliderValue();
                    ChangeHealthWithClamp(DEFAULT_MAX_PLAYER_HEALTH, false);
                }
                else
                {
                    entityRef.IsExecuted = true;
                    OnHaltRequested(RuntimeOp.Resolve<RuntimeSystem>().World);
                }
            }
        }

        private void InitiateRecovery()
        {
            healthModificationCoroutine = CoroutineService.RunCoroutine(() =>
            {
                InitiateRegeneration();
            },
            CoroutineService.DelayType.WaitForXSeconds, BUFFER_BEFORE_REGEN_STARTS);
        }

        private void InitiateRegeneration()
        {
            healthModificationCoroutine = CoroutineService.RunCoroutine(() =>
            {
                ChangeHealthWithClamp(-1, true);
            },
            CoroutineService.DelayType.UntilPredicateFailed, delay: HEALTH_REGENERATION_TIME, predicate: CanRegenerateMore);
        }

        private void ChangeHealthWithClamp(int modifier, bool isRegeneration)
        {
            ref var component = ref entity.GetComponent<PlayerHealthComponent>();
            if (!isRegeneration)
            {
                component.playerHealth += modifier;
            }
            else
            {
                component.playerHealth += component.regenerationPerSec;
            }
            component.playerHealth = Mathf.Clamp(component.playerHealth, 0, DEFAULT_MAX_PLAYER_HEALTH);
            AnimateHealthBar(component.slider, component.playerHealth);
        }

        private void AnimateHealthBar(Image slider, float playerHealth)
        {
            healthBarAnimationCoroutine = CoroutineService.LerpBetweenTwoFloats(slider.fillAmount, playerHealth / 100, HEALTH_MODIFICATION_ANIM_TIME, (newValue) =>
            {
                slider.fillAmount = newValue;
            });
        }

        private void StopAllCoroutines()
        {
            if (healthModificationCoroutine)
            {
                healthModificationCoroutine.Stop();
                healthModificationCoroutine = null;
            }
            if (healthBarAnimationCoroutine)
            {
                ForceSetSliderValue();
                healthBarAnimationCoroutine.Stop();
                healthBarAnimationCoroutine = null;
            }
        }

        private void ForceSetSliderValue()
        {
            ref var component = ref entity.GetComponent<PlayerHealthComponent>();
            component.slider.fillAmount = component.playerHealth / 100;
        }

        private bool CanRegenerateMore()
        {
            var playerHealth = entity.GetComponent<PlayerHealthComponent>().playerHealth;
            var canRegenerate = playerHealth != DEFAULT_MAX_PLAYER_HEALTH;
            return canRegenerate;
        }
    }
}