using UnityEngine;
using PlayShifu.Terra;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Terra.Studio
{
    public class RuntimeWrappers
    {
        public static GameObject SpawnObject(AssetType assetType, string assetPath, PrimitiveType primitiveType, params Vector3[] trs)
        {
            GameObject go = null;
            switch (assetType)
            {
                case AssetType.Empty:
                    go = SpawnEmpty(null, trs);
                    break;
                case AssetType.Prefab:
                    go = SpawnGameObject(assetPath, ResourceDB.GetItemData(assetPath), trs);
                    break;
                case AssetType.Primitive:
                    go = SpawnPrimitive(primitiveType, ResourceDB.GetDummyItemData(primitiveType), trs);
                    break;
            }
            return go;
        }
        public static GameObject SpawnGameObject(string path, ResourceDB.ResourceItemData itemData, params Vector3[] trs)
        {
            var go = RuntimeOp.Load<GameObject>(path);
            if (go == null)
            {
                return go;
            }
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                go = UnityEditor.PrefabUtility.InstantiatePrefab(go) as GameObject;
            }
            else
#endif

            {
                go = Object.Instantiate(go);
                Rulesets.ApplyRuleset(go);
            }
            return ResolveTRS(go, itemData, trs);
        }

        public static GameObject SpawnPrimitive(PrimitiveType type, ResourceDB.ResourceItemData itemData, params Vector3[] trs)
        {
            var go = GameObject.CreatePrimitive(type);
            return ResolveTRS(go, itemData, trs);
        }

        public static GameObject SpawnEmpty(ResourceDB.ResourceItemData itemData, params Vector3[] trs)
        {
            var go = new GameObject();
            return ResolveTRS(go, itemData, trs);
        }

        public static GameObject DuplicateGameObject(GameObject actualGameObject, Transform parent, params Vector3[] trs)
        {
            var go = Object.Instantiate(actualGameObject, parent);
            return ResolveTRS(go, null, trs);
        }

        public static GameObject ResolveTRS(GameObject go, ResourceDB.ResourceItemData itemData, params Vector3[] trs)
        {
            AttachPrerequisities(go, itemData);
            MoveGameObjectToActiveScene(go);
            if (trs == null || trs.Length == 0)
            {
                return go;
            }
            for (int i = 0; i < trs.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        go.transform.position = trs[i];
                        break;
                    case 1:
                        go.transform.rotation = Quaternion.Euler(trs[i]);
                        break;
                    case 2:
                        go.transform.localScale = trs[i];
                        break;
                }
            }
            return go;
        }

        public static void AttachPrerequisities(GameObject go, ResourceDB.ResourceItemData itemData)
        {
            if (itemData == null) return;
            if (SystemOp.Resolve<System>() && SystemOp.Resolve<System>().CurrentStudioState == StudioState.Editor)
            {
                if (!go.TryGetComponent(out StudioGameObject studioGO))
                {
                    studioGO = go.AddComponent<StudioGameObject>();
                }
                studioGO.itemData = itemData;
            }
        }

        public static void PlaySFX(string sfxName)
        {
            var sfx = RuntimeOp.Load<AudioClip>($"sfx/{sfxName}");
            if (!sfx)
            {
                return;
            }
            var go = new GameObject("SFX_Holder");
            var audioSource = go.AddComponent<AudioSource>();
            audioSource.clip = sfx;
            audioSource.loop = false;
            audioSource.Play();
            var destroyAfter = go.AddComponent<DestroyAfter>();
            destroyAfter.seconds = 2f;
            MoveGameObjectToActiveScene(go);
        }

        public static void PlayVFX(string vfxName, Vector3 position)
        {
            var vfxObj = RuntimeOp.Load<GameObject>($"vfx/{vfxName}");
            if (!vfxObj)
            {
                return;
            }
            var vfx = Object.Instantiate(vfxObj);
            vfx.transform.position = position;
            var destroyAfter = vfx.AddComponent<DestroyAfter>();
            destroyAfter.seconds = 2f;
            MoveGameObjectToActiveScene(vfx);
        }

        public static void AddScore(float addBy)
        {
            RuntimeOp.Resolve<ScoreHandler>().AddScore((int)addBy);
        }

        public static void RespawnPlayer(Vector3 position)
        {
            RuntimeOp.Resolve<GameData>().PlayerRef.position = position;
        }

        public static void CleanBehaviour(Transform child)
        {
            if (child.TryGetComponent(out BaseBehaviour behaviour))
            {
                Object.Destroy(behaviour);
            }
            if (child.TryGetComponent(out Outline outline))
            {
                Object.Destroy(outline);
            }
            if (child.TryGetComponent(out StudioGameObject gameObject))
            {
                Object.Destroy(gameObject);
            }
        }

        public static void MoveGameObjectToActiveScene(GameObject go)
        {
            if (Helper.IsInUnityEditorMode())
            {
                return;
            }
            var scene = SystemOp.Resolve<ISubsystem>().GetScene();
            if (go.scene != scene)
            {
                SceneManager.MoveGameObjectToScene(go, scene);
            }
        }
    }
}
