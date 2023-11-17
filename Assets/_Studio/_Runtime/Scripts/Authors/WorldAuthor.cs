using System;
using Newtonsoft.Json;

namespace Terra.Studio
{
    public class WorldAuthorOp
    {
        public static IAuthor Author => Author<WorldAuthor>.Current;

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
                var resourceData = SystemOp.Resolve<CrossSceneDataHolder>().Get();
                if (string.IsNullOrEmpty(resourceData)) return null;
                var worldData = JsonConvert.DeserializeObject<WorldData>(resourceData);
                return worldData;
            }

            public override void Generate(object data)
            {
                var action = (Action)data;
                var worldData = GetWorldData();
                if (worldData == null || !worldData.HasValue)
                {
                    action?.Invoke();
                    return;
                }
                var value = worldData.Value;
                SystemOp.Resolve<RequestValidator>().Prewarm(ref value, () =>
                {
                    foreach (var virtualEntity in value.entities)
                    {
                        EntityAuthorOp.Generate(virtualEntity);
                    }
                    var metaData = value.metaData;
                    if (metaData.Equals(default(WorldMetaData)))
                    {
                        OnGenerateDone(action);
                        return;
                    }
                    RuntimeOp.Resolve<GameData>().RespawnPoint = metaData.playerSpawnPoint;
                    OnGenerateDone(action);
                });
            }

            private void OnGenerateDone(Action action)
            {
                action?.Invoke();
#if UNITY_WEBGL && !UNITY_EDITOR
            WebGLWrapper.HideLoadingScreen();
#endif
            }
        }
    }
}
