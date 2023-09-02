using Newtonsoft.Json;

namespace Terra.Studio
{
    [Author("Terra.Studio.GameScore")]
    public class InGameScoreAuthor : BaseAuthor
    {
        public override void Generate(object data)
        {
            var authorData = (ComponentAuthorData)data;
            var compData = JsonConvert.DeserializeObject<InGameScoreComponent>(authorData.compData);
            ref var compRef = ref ComponentAuthorOp.AddEntityToComponent<InGameScoreComponent>(authorData.entity);
            ((IBaseComponent)compRef).Clone(compData, ref compRef, authorData.obj);
        }
    }
}