using Newtonsoft.Json;

namespace Terra.Studio
{
    [Author("Terra.Studio.Oscillate")]
    public class OscillateAuthor : BaseAuthor
    {
        public override void Generate(object data)
        {
            var authorData = (ComponentAuthorData)data;
            var oscillateCompData = JsonConvert.DeserializeObject<OscillateComponent>(authorData.compData);
            ref var compRef = ref ComponentAuthorOp.AddEntityToComponent<OscillateComponent>(authorData.entity);
            ((IBaseComponent)compRef).Clone(oscillateCompData, ref compRef, authorData.obj);
            var instance = RuntimeOp.Resolve<RuntimeSystem>().AddRunningInstance<OscillateSystem>();
            instance.Init<OscillateComponent>(authorData.entity);
        }
    }
}