using UnityEngine;
using Newtonsoft.Json;

namespace Terra.Studio
{
    public class WorldAuthor : BaseAuthor
    {
        public WorldData? GetWorldData()
        {
            //var resourceData = Resources.Load<TextAsset>("OscillationLogic");
            var resourceData = Interop<SystemInterop>.Current.Resolve<CrossSceneDataHolder>().Get();
            if (string.IsNullOrEmpty(resourceData)) return null;
            var worldData = JsonConvert.DeserializeObject<WorldData>(resourceData);
            return worldData;
        }

        public override void Generate()
        {
            var worldData = GetWorldData();
            if (worldData == null || !worldData.HasValue) return;
            foreach (var virtualEntity in worldData.Value.entities)
            {
                Author<EntityAuthor>.Current.Generate(virtualEntity);
            }
        }
    }
}
