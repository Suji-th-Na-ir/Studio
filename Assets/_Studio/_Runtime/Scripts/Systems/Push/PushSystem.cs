using UnityEngine;
using UnityEngine.UI;
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
            ref var entityRef = ref EntityAuthorOp.GetComponent<PushComponent>(entity);
            if (!entityRef.RefObj.TryGetComponent(out Rigidbody rb))
            {
                rb = entityRef.RefObj.AddComponent<Rigidbody>();
            }
            rb.drag = entityRef.drag;
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
            ref var entityRef = ref EntityAuthorOp.GetComponent<PushComponent>(entity);
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

        public void OnDemandRun(in PushComponent component, int _)
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
            if (component.listen != Listen.Always)
            {
                var compsData = RuntimeOp.Resolve<ComponentsData>();
                compsData.ProvideEventContext(false, component.EventContext);
            }
            LoadUI(component);
        }

        private void LoadUI(in PushComponent component)
        {
            if (!component.showResetButton)
            {
                return;
            }
            var go = RuntimeOp.Resolve<View>().AttachDynamicUI(resetPrefabObj);
            var btn = go.GetComponent<Button>();
            var refTr = component.RefObj.transform;
            var newPos = component.initialPosition;
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
