using Newtonsoft.Json;

namespace Terra.Studio
{
    [Author("Terra.Studio.Rotate")]
    public class RotateAuthor : BaseAuthor
    {
        public override void Generate(object data)
        {
            var authorData = (ComponentAuthorData)data;
            var compData = JsonConvert.DeserializeObject<RotateComponent>(authorData.compData);
            ref var compRef = ref ComponentAuthorOp.AddEntityToComponent<RotateComponent>(authorData.entity);
            ((IBaseComponent)compRef).Clone(compData, ref compRef);
            var instance = RuntimeOp.Resolve<RuntimeSystem>().AddRunningInstance<RotateSystem>();
            instance.Init<RotateComponent>(authorData.entity);
        }
    }
}