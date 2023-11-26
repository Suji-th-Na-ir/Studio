using UnityEngine;
using PlayShifu.Terra;

namespace Terra.Studio
{
    public class StopTranslateSystem : BaseSystem<StopTranslateComponent>
    {
        public override void Init(int entity)
        {
            base.Init(entity);
            ref var entityRef = ref entity.GetComponent<StopTranslateComponent>();
            var rb = entityRef.RefObj.AddRigidbody();
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        protected override void OnConditionalCheck(int entity, object data)
        {
            ref var entityRef = ref entity.GetComponent<StopTranslateComponent>();
            var isTranslateFound = CheckIfRotateComponentExistsOnEntity(entity);
            if (!isTranslateFound)
            {
                Debug.Log($"Translate system not found on entity: {entity} for stop translate to act on");
                return;
            }
            var compsData = RuntimeOp.Resolve<ComponentsData>();
            ref var translateRef = ref entity.GetComponent<TranslateComponent>();
            if (translateRef.ConditionType.Equals("Terra.Studio.GameStart"))
            {
                translateRef.IsExecuted = true;
            }
            else if (!translateRef.isHaltedByEvent && translateRef.CanExecute)
            {
                translateRef.CanExecute = false;
                compsData.ProvideEventContext(true, translateRef.EventContext);
            }
            translateRef.isHaltedByEvent = true;
            PlayFXIfExists(entityRef, 0);
            Broadcast(entityRef);
        }

        private bool CheckIfRotateComponentExistsOnEntity(int entity)
        {
            var currentWorld = RuntimeOp.Resolve<RuntimeSystem>().World;
            var filter = currentWorld.Filter<TranslateComponent>().End();
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
