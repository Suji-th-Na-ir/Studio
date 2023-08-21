using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Terra.Studio
{
    public class RuntimeWrappers
    {
        public static GameObject SpawnGameObject(string path, ResourceDB.ResourceItemData itemData, params Vector3[] trs)
        {
            var go = RuntimeOp.Load<GameObject>(path);
            if (go == null)
            {
                return go;
            }
            go = Object.Instantiate(go);
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

        private static GameObject ResolveTRS(GameObject go, ResourceDB.ResourceItemData itemData, params Vector3[] trs)
        {
            AttachPrerequisities(go, itemData);
            if (trs == null || trs.Length == 0)
            {
                return go;
            }
            for (int i = 0; i < 3; i++)
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
            if (SystemOp.Resolve<System>() && SystemOp.Resolve<System>().CurrentStudioState == StudioState.Editor)
            {
                if (!go.TryGetComponent(out StudioGameObject studioGO))
                {
                    studioGO = go.AddComponent<StudioGameObject>();
                    studioGO.itemData = itemData;
                }
            }
        }

        public static void PlaySFX(string sfxName)
        {
            var go = new GameObject("SFX_Holder");
            var audioSource = go.AddComponent<AudioSource>();
            var sfx = RuntimeOp.Load<AudioClip>($"sfx/{sfxName}");
            audioSource.clip = sfx;
            audioSource.loop = false;
            audioSource.Play();
            var destroyAfter = go.AddComponent<DestroyAfter>();
            destroyAfter.seconds = 2f;
        }

        public static void PlayVFX(string vfxName, Vector3 position)
        {
            var vfxObj = RuntimeOp.Load<GameObject>($"vfx/{vfxName}");
            var vfx = Object.Instantiate(vfxObj);
            vfx.transform.position = position;
            var destroyAfter = vfx.AddComponent<DestroyAfter>();
            destroyAfter.seconds = 2f;
        }

        public static void AddScore(float addBy)
        {
            RuntimeOp.Resolve<ScoreHandler>().AddScore((int)addBy);
        }

        public static void RotateObject(RotateByParams rotateParams)
        {
            var rotateBy = rotateParams.targetObj.AddComponent<RotateByHelper>();
            rotateBy.Rotate(rotateParams);
        }

        public static void TranslateObject(TranslateParams translateParams)
        {
            var translate = translateParams.targetObj.AddComponent<TranslateHelper>();
            translate.Translate(translateParams);
        }

        public static void RespawnPlayer(Vector3 position)
        {
            RuntimeOp.Resolve<GameData>().PlayerRef.position = position;
        }
    }

    public struct RotateByParams
    {
        public float rotateBy;
        public float rotationSpeed;
        public int rotationTimes;
        public bool shouldPingPong;
        public bool shouldPause;
        public float pauseForTime;
        public GameObject targetObj;
        public Action<bool> onRotated;
        public Axis axis;
        public Direction direction;
        public BroadcastAt broadcastAt;
    }

    public struct TranslateParams
    {
        public Vector3 translateFrom;
        public Vector3 translateTo;
        public float speed;
        public int translateTimes;
        public bool shouldPingPong;
        public bool shouldPause;
        public float pauseForTime;
        public float pauseDistance;
        public GameObject targetObj;
        public Action<bool> onTranslated;
        public BroadcastAt broadcastAt;
    }
}
