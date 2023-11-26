namespace Terra.Studio
{
    public class OscillateSystem : BaseSystem<OscillateComponent>
    {
        //public override void Init<T>(int entity)
        //{
        //    base.Init<T>(entity);
        //    ref var oscillatable = ref entity.GetComponent<OscillateComponent>();
        //    if (oscillatable.RefObj.transform.parent != null)
        //    {
        //        oscillatable.fromPoint = oscillatable.RefObj.transform.TransformPoint(oscillatable.fromPoint);
        //        oscillatable.toPoint = oscillatable.RefObj.transform.TransformPoint(oscillatable.toPoint);
        //    }
        //}

        //public override void OnConditionalCheck(int entity, object data)
        //{
        //    ref var entityRef = ref entity.GetComponent<OscillateComponent>();
        //    var compsData = RuntimeOp.Resolve<ComponentsData>();
        //    compsData.ProvideEventContext(false, entityRef.EventContext);
        //    entityRef.RefObj.transform.position = entityRef.fromPoint;
        //    entityRef.CanExecute = true;
        //    entityRef.EventContext = default;
        //}

        //public virtual void Run(IEcsSystems systems)
        //{
        //    var filter = systems.GetWorld().Filter<OscillateComponent>().End();
        //    var oscillatorPool = systems.GetWorld().GetPool<OscillateComponent>();
        //    var totalEntitiesFinishedJob = 0;
        //    foreach (var entity in filter)
        //    {
        //        ref var oscillatable = ref oscillatorPool.Get(entity);
        //        if (oscillatable.IsConditionAvailable && !oscillatable.CanExecute)
        //        {
        //            return;
        //        }
        //        if (oscillatable.IsExecuted)
        //        {
        //            totalEntitiesFinishedJob++;
        //            continue;
        //        }
        //        var targetTr = oscillatable.RefObj.transform;
        //        var delta = targetTr.position - oscillatable.toPoint;
        //        if (Mathf.Approximately(delta.sqrMagnitude, float.Epsilon))
        //        {
        //            if (!oscillatable.loop)
        //            {
        //                oscillatable.IsExecuted = true;
        //                if (oscillatable.IsBroadcastable)
        //                {
        //                    RuntimeOp.Resolve<Broadcaster>().Broadcast(oscillatable.Broadcast);
        //                }
        //                continue;
        //            }
        //            else
        //            {
        //                (oscillatable.fromPoint, oscillatable.toPoint) = (oscillatable.toPoint, oscillatable.fromPoint);
        //            }
        //        }
        //        targetTr.position = Vector3.MoveTowards(targetTr.position, oscillatable.toPoint, oscillatable.speed * Time.deltaTime);
        //    }
        //    if (totalEntitiesFinishedJob == filter.GetEntitiesCount())
        //    {
        //        RuntimeOp.Resolve<RuntimeSystem>().RemoveRunningInstance(this);
        //    }
        //}
    }
}
