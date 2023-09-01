using Newtonsoft.Json;

namespace Terra.Studio
{
    [Author("Terra.Studio.DestroyOn")]
    public class DestroyOnAuthor : BaseAuthor
    {
        public override void Generate(object data)
        {
            var authorData = (ComponentAuthorData)data;
            var compData = JsonConvert.DeserializeObject<DestroyOnComponent>(authorData.compData);
            ref var compRef = ref ComponentAuthorOp.AddEntityToComponent<DestroyOnComponent>(authorData.entity);
            ((IBaseComponent)compRef).Clone(compData, ref compRef);
            var instance = RuntimeOp.Resolve<RuntimeSystem>().AddRunningInstance<DestroyOnSystem>();
            instance.Init<DestroyOnComponent>(authorData.entity);
        }
    }
}