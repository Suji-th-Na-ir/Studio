using UnityEngine;
using Newtonsoft.Json;

namespace Terra.Studio
{
    public class WorldAuthorOp
    {
        public static IAuthor Author => Author<WorldAuthor>.Current;

        public static void Generate()
        {
            Author.Generate();
        }

        public static void Generate(object data)
        {
            Author.Generate(data);
        }

        public static void Degenerate(int entityID)
        {
            Author.Degenerate(entityID);
        }

        public static void Flush()
        {
            Author<WorldAuthor>.Flush();
        }

        private class WorldAuthor : BaseAuthor
        {
            public WorldData? GetWorldData()
            {
                //var resourceData = Resources.Load<TextAsset>("OscillationLogic").text;
                var resourceData = SystemOp.Resolve<CrossSceneDataHolder>().Get();
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
                    EntityAuthorOp.Generate(virtualEntity);
                }
            }
        }
    }
}
