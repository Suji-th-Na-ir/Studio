using Newtonsoft.Json;

namespace Terra.Studio
{
    [Author("Terra.Studio.Checkpoint")]
    public class CheckpointAuthor : BaseAuthor
    {
        public override void Generate(object data)
        {
            var authorData = (ComponentAuthorData)data;
            var component = JsonConvert.DeserializeObject<CheckpointComponent>(authorData.compData);
            var ecsWorld = RuntimeOp.Resolve<RuntimeSystem>().World;
            var compPool = ecsWorld.GetPool<CheckpointComponent>();
            compPool.Add(authorData.entity);
            ref var compRef = ref compPool.Get(authorData.entity);
            ((IBaseComponent)component).Clone(component, ref compRef);
            var instance = RuntimeOp.Resolve<RuntimeSystem>().AddRunningInstance<CheckpointSystem>();
            instance.Init(authorData.entity);
        }
    }
}