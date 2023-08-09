using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Terra.Studio
{
    public class RuntimeWrappers
    {
        public static GameObject SpawnGameObject(string path, params Vector3[] trs)
        {
            var go = Resources.Load<GameObject>(path);
            if (go == null)
                return go;
            go = Object.Instantiate(go);

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

        public static void PlaySFX(string sfxName)
        {
            var go = new GameObject("SFX_Holder");
            var audioSource = go.AddComponent<AudioSource>();
            var sfx = Resources.Load<AudioClip>($"sfx/{sfxName}");
            audioSource.clip = sfx;
            audioSource.loop = false;
            audioSource.Play();
            var destroyAfter = go.AddComponent<DestroyAfter>();
            destroyAfter.seconds = 2f;
        }

        public static void PlayVFX(string vfxName, Vector3 position)
        {
            var vfxObj = Resources.Load<GameObject>($"vfx/{vfxName}");
            var vfx = Object.Instantiate(vfxObj);
            vfx.transform.position = position;
            var destroyAfter = vfx.AddComponent<DestroyAfter>();
            destroyAfter.seconds = 2f;
        }

        public static void AddScore(float addBy)
        {
            Debug.Log($"Adding score by: {addBy}");
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
            RuntimeOp.Resolve<GameData>().PlayerRef.parent.position = position;
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
