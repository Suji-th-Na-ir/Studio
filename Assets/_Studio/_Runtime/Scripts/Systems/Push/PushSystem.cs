using UnityEngine;
using UnityEngine.UI;
using PlayShifu.Terra;
using Leopotam.EcsLite;

namespace Terra.Studio
{
    public class PushSystem : BaseSystem
    {
        private const string RESET_GO_RESOURCE_NAME = "ResetParent";
        private GameObject resetPrefabObj;

        public override void Init<T>(int entity)
        {
            base.Init<T>(entity);
            ref var entityRef = ref entity.GetComponent<PushComponent>();
            var rb = entityRef.RefObj.AddRigidbody();
            rb.mass = entityRef.mass;
            rb.freezeRotation = true;
            entityRef.initialPosition = entityRef.RefObj.transform.position;
            InitializeUI();
        }

        private void InitializeUI()
        {
            if (resetPrefabObj)
            {
                return;
            }
            var resourceDB = (ResourceDB)SystemOp.Load(ResourceTag.ResourceDB);
            var itemData = resourceDB.GetItemDataForNearestName(RESET_GO_RESOURCE_NAME);
            resetPrefabObj = RuntimeOp.Load<GameObject>(itemData.ResourcePath);
        }

        public override void OnConditionalCheck(int entity, object data)
        {
            ref var entityRef = ref entity.GetComponent<PushComponent>();
            if (data == null)
            {
                return;
            }
            var otherGO = data as GameObject;
            var isValid = IsGOContextValid(otherGO, entity);
            if (!isValid)
            {
                return;
            }
            if (entityRef.listen != Listen.Always)
            {
                var compsData = RuntimeOp.Resolve<ComponentsData>();
                compsData.ProvideEventContext(false, entityRef.EventContext);
                entityRef.IsExecuted = true;
            }
            OnDemandRun(in entityRef, entity);
        }

        private bool IsGOContextValid(GameObject go, int currentEntity)
        {
            if (go.transform.CompareTag("Player"))
            {
                return true;
            }
            var currentWorld = RuntimeOp.Resolve<RuntimeSystem>().World;
            var filter = currentWorld.Filter<PushComponent>().End();
            var compPool = currentWorld.GetPool<PushComponent>();
            foreach (var entity in filter)
            {
                if (entity == currentEntity)
                {
                    continue;
                }
                var componentToCheck = compPool.Get(entity);
                if (go == componentToCheck.RefObj)
                {
                    return true;
                }
            }
            return false;
        }

        public void OnDemandRun(in PushComponent component, int entity)
        {
            if (component.canPlaySFX)
            {
                RuntimeWrappers.PlaySFX(component.sfxName);
            }
            if (component.canPlayVFX)
            {
                RuntimeWrappers.PlayVFX(component.vfxName, component.RefObj.transform.position);
            }
            var listenMultipleTimes = component.listen == Listen.Always;
            if (component.IsBroadcastable)
            {
                RuntimeOp.Resolve<Broadcaster>().Broadcast(component.Broadcast, !listenMultipleTimes);
            }
            LoadUI(component, entity);
        }

        private void LoadUI(in PushComponent component, int entity)
        {
            if (!component.showResetButton)
            {
                return;
            }
            var go = RuntimeOp.Resolve<View>().AttachDynamicUI(nameof(PushComponent), resetPrefabObj);
            var btn = go.GetComponent<Button>();
            var refTr = component.RefObj.transform;
            var newPos = component.initialPosition;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => { refTr.position = newPos; });
        }

        public override void OnHaltRequested(EcsWorld currentWorld)
        {
            var filter = currentWorld.Filter<PushComponent>().End();
            var compPool = currentWorld.GetPool<PushComponent>();
            foreach (var entity in filter)
            {
                var component = compPool.Get(entity);
                if (component.IsExecuted)
                {
                    continue;
                }
                var compsData = RuntimeOp.Resolve<ComponentsData>();
                compsData.ProvideEventContext(false, component.EventContext);
            }
        }
    }
}
