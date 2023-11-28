using UnityEngine;
using UnityEngine.UI;
using PlayShifu.Terra;
using Leopotam.EcsLite;

namespace Terra.Studio
{
    public class MeleeDamageableSystem : BaseSystem<MeleeDamageableComponent>, IEcsRunSystem
    {
        private const string HEALTH_UI = "DynamicUI/HealthBar";
        private const string SLIDER_LOC = "Slider";
        private const float UPPER_LIMIT_MAGNITUDE = 35f;
        private const float LOWER_LIMIT_MAGNITUDE = 5f;
        private const float UPPER_LIMIT_HEALTH_SCALE = 0.3f;
        private const float LOWER_LIMIT_HEALTH_SCALE = 0.7f;
        private Camera camera;
        private Transform player;

        public override void Init(int entity)
        {
            ref var entityRef = ref entity.GetComponent<MeleeDamageableComponent>();
            entityRef.currentHealth = entityRef.health;
            var eventContext = entityRef.EventContext;
            eventContext.conditionType = "Terra.Studio.GameStart";
            eventContext.conditionData = "OnStart";
            entityRef.EventContext = eventContext;
            base.Init(entity);
        }

        private void SpawnHealthUI(ref MeleeDamageableComponent component, int entity)
        {
            var componentName = string.Concat(nameof(MeleeDamageableComponent), "_", entity);
            var go = RuntimeOp.Load<GameObject>(HEALTH_UI);
            var ui = RuntimeOp.Resolve<View>().AttachDynamicUI(componentName, go);
            component.healthBar = Helper.FindDeepChild<Image>(ui.transform, SLIDER_LOC);
            component.healthUI = ui;
            component.CanExecute = true;
            camera = RuntimeOp.Resolve<PlayerData>().PlayerCamera;
            player = RuntimeOp.Resolve<PlayerData>().PlayerRef;
        }

        protected override void OnConditionalCheck(int entity, object data)
        {
            ref var entityRef = ref EntityAuthorOp.GetComponent<MeleeDamageableComponent>(entity);
            if (entityRef.EventContext.conditionType.Equals("Terra.Studio.GameStart"))
            {
                MapOriginalData(ref entityRef);
                SpawnHealthUI(ref entityRef, entity);
                return;
            }
            if (data == null) return;
            var go = data as GameObject;
            if (go)
            {
                if (go.transform.CompareTag("Damager"))
                {
                    var currentWorld = RuntimeOp.Resolve<RuntimeSystem>().World;
                    var filter = currentWorld.Filter<MeleeWeaponComponent>().End();
                    var compPool = currentWorld.GetPool<MeleeWeaponComponent>();
                    foreach (var entity1 in filter)
                    {
                        if (entity1 == entity)
                        {
                            continue;
                        }
                        var componentToCheck = compPool.Get(entity1);
                        if (go == componentToCheck.RefObj)
                        {
                            OnDemandRun(ref entityRef, entity, componentToCheck.damage);
                            break;
                        }
                    }
                }
            }

            if (entityRef.currentHealth <= 0)
            {
                entityRef.IsExecuted = true;
                DeleteEntity(entity);
            }
        }

        public void OnDemandRun(ref MeleeDamageableComponent component, int entity, int damage)
        {
            component.currentHealth = Mathf.Clamp(component.currentHealth - damage, 0, int.MaxValue);
            component.healthBar.fillAmount = (float)component.currentHealth / component.health;
            if (component.currentHealth > 0)
            {
                PlayFXIfExists(component, 0);
                Broadcast(component);
            }
            else
            {
                OnObjectDestroyInvoked(component, entity);
            }
        }

        private void OnObjectDestroyInvoked(in MeleeDamageableComponent component, int entity, bool shouldExecuteEndState = true)
        {
            var componentName = string.Concat(nameof(MeleeDamageableComponent), "_", entity);
            RuntimeOp.Resolve<View>().RemoveDynamicUI(componentName);
            PlayFXIfExists(component, 1);
            if (component.IsBroadcastableDead)
            {
                RuntimeOp.Resolve<Broadcaster>().Broadcast(component.BroadcastDead);
            }
        }

        public void Run(IEcsSystems systems)
        {
            var filter = systems.GetWorld().Filter<MeleeDamageableComponent>().End();
            var pool = systems.GetWorld().GetPool<MeleeDamageableComponent>();
            var totalEntites = filter.GetEntitiesCount();
            if (totalEntites == 0)
            {
                RemoveRunningInstance();
                return;
            }
            foreach (var entity in filter)
            {
                var component = pool.Get(entity);
                if (!component.CanExecute || component.IsExecuted) continue;
                var ui = component.healthUI;
                var targetPos = component.RefObj.transform.position;
                var isInCameraView = IsInCameraView(targetPos);
                if (!isInCameraView)
                {
                    if (ui.activeSelf)
                    {
                        ui.SetActive(false);
                    }
                    continue;
                }
                else if (!ui.activeSelf)
                {
                    ui.SetActive(true);
                }
                var modifiedPlayerPos = new Vector3(player.position.x, 0f, player.position.z);
                var direction = targetPos - modifiedPlayerPos;
                var magnitude = Vector3.Magnitude(direction);
                magnitude = Mathf.Clamp(magnitude, LOWER_LIMIT_MAGNITUDE, UPPER_LIMIT_MAGNITUDE);
                var scale = GetRatioFittedValue(magnitude);
                ui.transform.localScale = new Vector3(scale, scale, scale);
                var worldPos = component.RefObj.transform.position;
                var screenPos = camera.WorldToScreenPoint(worldPos);
                ui.transform.position = new Vector3(screenPos.x, screenPos.y, 0);
            }
        }

        private void MapOriginalData(ref MeleeDamageableComponent component)
        {
            var compsData = RuntimeOp.Resolve<ComponentsData>();
            compsData.ProvideEventContext(false, component.EventContext);
            var eventContext = component.EventContext;
            eventContext.conditionData = component.ConditionData;
            eventContext.conditionType = component.ConditionType;
            component.EventContext = eventContext;
            compsData.ProvideEventContext(true, component.EventContext);
        }

        private float GetRatioFittedValue(float value)
        {
            var oldRange = UPPER_LIMIT_MAGNITUDE - LOWER_LIMIT_MAGNITUDE;
            var newRange = UPPER_LIMIT_HEALTH_SCALE - LOWER_LIMIT_HEALTH_SCALE;
            var newValue = ((value - LOWER_LIMIT_MAGNITUDE) * newRange / oldRange) + LOWER_LIMIT_HEALTH_SCALE;
            return newValue;
        }

        private bool IsInCameraView(Vector3 worldPoint)
        {
            var viewportPoint = camera.WorldToViewportPoint(worldPoint);
            if (viewportPoint.x > 0 && viewportPoint.x < 1 &&
               viewportPoint.y > 0 && viewportPoint.y < 1 &&
               viewportPoint.z > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void OnEntityQueuedToDestroy(int entity)
        {
            var world = RuntimeOp.Resolve<RuntimeSystem>().World;
            var pool = world.GetPool<MeleeDamageableComponent>();
            var isDamagable = pool.Has(entity);
            if (!isDamagable) return;
            ref var component = ref pool.Get(entity);
            component.IsExecuted = true;
            OnObjectDestroyInvoked(component, entity, false);
        }
    }
}
