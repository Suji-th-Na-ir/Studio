using Newtonsoft.Json;

namespace Terra.Studio
{
    [Author("Terra.Studio.Translate")]
    public class TranslateAuthor : BaseAuthor
    {
        public override void Generate(object data)
        {
            var authorData = (ComponentAuthorData)data;
            var compData = JsonConvert.DeserializeObject<TranslateComponent>(authorData.compData);
            ref var compRef = ref ComponentAuthorOp.AddEntityToComponent<TranslateComponent>(authorData.entity);
            ((IBaseComponent)compRef).Clone(compData, ref compRef);
            var instance = RuntimeOp.Resolve<RuntimeSystem>().AddRunningInstance<TranslateSystem>();
            instance.Init<TranslateComponent>(authorData.entity);
        }
    }
}