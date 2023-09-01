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
            ref var compRef = ref ComponentAuthorOp.AddEntityToComponent<CheckpointComponent>(authorData.entity);
            ((IBaseComponent)component).Clone(component, ref compRef);
            var instance = RuntimeOp.Resolve<RuntimeSystem>().AddRunningInstance<CheckpointSystem>();
            instance.Init<CheckpointComponent>(authorData.entity);
        }
    }
}