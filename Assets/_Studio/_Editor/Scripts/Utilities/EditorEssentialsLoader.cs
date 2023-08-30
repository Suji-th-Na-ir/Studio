using UnityEngine;

namespace Terra.Studio
{
    public class EditorEssentialsLoader
    {
        private const string ESSENTIALS_SO_PATH = "SOs/EditorEssentialsLoaderSO";

        public void LoadEssentials()
        {
            var so = EditorOp.Load<EssentialLoaderSO>(ESSENTIALS_SO_PATH);
            foreach (var essential in so.Essentials)
            {
                var resourceObj = EditorOp.Load<GameObject>(essential.itemData.ResourcePath);
                var spawnedObj = Object.Instantiate(resourceObj);
                Rulesets.ApplyRuleset(spawnedObj);
                spawnedObj.transform.position = essential.spawnPosition;
                spawnedObj.AddComponent<IgnoreToPackObject>();
                var studioGO = spawnedObj.AddComponent<StudioGameObject>();
                studioGO.itemData = essential.itemData;
                studioGO.type = essential.type;
                HandleLoadedObj(spawnedObj, essential.type);
            }
        }

        private void HandleLoadedObj(GameObject go, EditorObjectType type)
        {
            if (type == EditorObjectType.SpawnPoint)
            {
                var pos = EditorOp.Resolve<SceneDataHandler>().PlayerSpawnPoint;
                go.transform.position = pos;
            }
        }
    }
}