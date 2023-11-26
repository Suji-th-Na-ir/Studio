using UnityEngine;
using PlayShifu.Terra;

namespace Terra.Studio
{
    public class StopRotateSystem : BaseSystem<StopRotateComponent>
    {
        public override void Init(int entity)
        {
            base.Init(entity);
            ref var entityRef = ref entity.GetComponent<StopRotateComponent>();
            var rb = entityRef.RefObj.AddRigidbody();
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        protected override void OnConditionalCheck(int entity, object data)
        {
            ref var entityRef = ref entity.GetComponent<StopRotateComponent>();
            var isRotateFound = CheckIfRotateComponentExistsOnEntity(entity);
            if (!isRotateFound)
            {
                Debug.Log($"Rotate system not found on entity: {entity} for stop rotate to act on", entityRef.RefObj);
                return;
            }
            var compsData = RuntimeOp.Resolve<ComponentsData>();
            ref var rotateRef = ref entity.GetComponent<RotateComponent>();
            if (rotateRef.ConditionType.Equals("Terra.Studio.GameStart"))
            {
                rotateRef.IsExecuted = true;
            }
            else if (!rotateRef.isHaltedByEvent && rotateRef.CanExecute)
            {
                rotateRef.CanExecute = false;
                compsData.ProvideEventContext(true, rotateRef.EventContext);
            }
            rotateRef.isHaltedByEvent = true;
            OnDemandRun(in entityRef);
        }

        public void OnDemandRun(in StopRotateComponent component)
        {
            PlayFXIfExists(component, 0);
            Broadcast(component);
        }

        private bool CheckIfRotateComponentExistsOnEntity(int entity)
        {
            var currentWorld = RuntimeOp.Resolve<RuntimeSystem>().World;
            var filter = currentWorld.Filter<RotateComponent>().End();
            foreach (var otherEntity in filter)
            {
                if (otherEntity == entity)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
