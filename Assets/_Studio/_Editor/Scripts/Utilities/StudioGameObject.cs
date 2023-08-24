using UnityEngine;

namespace Terra.Studio
{
    public class StudioGameObject : MonoBehaviour
    {
        public ResourceDB.ResourceItemData itemData;
        public EditorObjectType type;

        private void Start()
        {
            if (type == EditorObjectType.SpawnPoint)
            {
                var pos = EditorOp.Resolve<EditorSystem>().PlayerSpawnPoint;
                transform.position = pos;
            }
        }
    }
}
