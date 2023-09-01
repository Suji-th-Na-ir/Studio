using Newtonsoft.Json;

namespace Terra.Studio
{
    [Author("Terra.Studio.Collectable")]
    public class CollectableAuthor : BaseAuthor
    {
        public override void Generate(object data)
        {
            var authorData = (ComponentAuthorData)data;
            var collectableCompData = JsonConvert.DeserializeObject<CollectableComponent>(authorData.compData);
            ref var compRef = ref ComponentAuthorOp.AddEntityToComponent<CollectableComponent>(authorData.entity);
            ((IBaseComponent)compRef).Clone(collectableCompData, ref compRef, authorData.obj);
            var instance = RuntimeOp.Resolve<RuntimeSystem>().AddRunningInstance<CollectableSystem>();
            instance.Init<CollectableComponent>(authorData.entity);
        }
    }
}