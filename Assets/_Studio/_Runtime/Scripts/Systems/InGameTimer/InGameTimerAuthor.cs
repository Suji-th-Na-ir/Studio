using Newtonsoft.Json;

namespace Terra.Studio
{
    [Author("Terra.Studio.InGameTimer")]
    public class InGameTimerAuthor : BaseAuthor
    {
        public override void Generate(object data)
        {
            var authorData = (ComponentAuthorData)data;
            var compData = JsonConvert.DeserializeObject<InGameTimerComponent>(authorData.compData);
            ref var compRef = ref ComponentAuthorOp.AddEntityToComponent<InGameTimerComponent>(authorData.entity);
            ((IBaseComponent)compRef).Clone(compData, ref compRef, authorData.obj);
            var instance = RuntimeOp.Resolve<RuntimeSystem>().AddRunningInstance<InGameTimerSystem>();
            instance.Init<InGameTimerComponent>(authorData.entity);
        }
    }
}