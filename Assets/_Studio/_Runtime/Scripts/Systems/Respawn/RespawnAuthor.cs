using Newtonsoft.Json;

namespace Terra.Studio
{
    [Author("Terra.Studio.Respawn")]
    public class RespawnAuthor : BaseAuthor
    {
        public override void Generate(object data)
        {
            var authorData = (ComponentAuthorData)data;
            var compData = JsonConvert.DeserializeObject<RespawnComponent>(authorData.compData);
            ref var compRef = ref ComponentAuthorOp.AddEntityToComponent<RespawnComponent>(authorData.entity);
            ((IBaseComponent)compRef).Clone(compData, ref compRef, authorData.obj);
            var instance = RuntimeOp.Resolve<RuntimeSystem>().AddRunningInstance<RespawnSystem>();
            instance.Init<RespawnComponent>(authorData.entity);
        }
    }
}