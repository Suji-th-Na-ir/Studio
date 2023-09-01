using UnityEngine;

namespace Terra.Studio
{
    public class EditorEssentialsLoader
    {
        private const string ESSENTIALS_SO_PATH = "SOs/EditorEssentialsLoaderSO";
        private readonly EssentialLoaderSO SO;

        public EditorEssentialsLoader()
        {
            SO = EditorOp.Load<EssentialLoaderSO>(ESSENTIALS_SO_PATH);
        }

        public void LoadEssentials()
        {
            foreach (var essential in SO.Essentials)
            {
                SpawnEssential(essential, out GameObject _);
            }
        }

        public void Load(EditorObjectType objectType, out GameObject reference)
        {
            var isAvailable = SO.TryGetData(objectType, out var data);
            if (isAvailable)
            {
                SpawnEssential(data, out reference);
            }
            else
            {
                reference = null;
            }
        }

        private void SpawnEssential(EssentialLoaderSO.EssentialsData essential, out GameObject spawnedObj)
        {
            var resourceObj = EditorOp.Load<GameObject>(essential.itemData.ResourcePath);
            spawnedObj = Object.Instantiate(resourceObj);
            Rulesets.ApplyRuleset(spawnedObj);
            spawnedObj.transform.position = essential.spawnPosition;
            spawnedObj.AddComponent<IgnoreToPackObject>();
            var studioGO = spawnedObj.AddComponent<StudioGameObject>();
            studioGO.itemData = essential.itemData;
            studioGO.type = essential.type;
            HandleLoadedObj(spawnedObj, essential.type);
            spawnedObj.name = spawnedObj.name.Replace("(Clone)", null);
        }

        private void HandleLoadedObj(GameObject go, EditorObjectType type)
        {
            if (type == EditorObjectType.SpawnPoint)
            {
                var pos = EditorOp.Resolve<SceneDataHandler>().PlayerSpawnPoint;
                go.transform.position = pos;
            }
            if (type == EditorObjectType.Score)
            {
                go.AddComponent<GameScore>();
            }
        }
    }
}