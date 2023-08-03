using System;
using UnityEngine;

namespace Terra.Studio
{
    public class RuntimeWrappers
    {
        public static PrimitiveType GetPrimitiveType(string value)
        {
            if (Enum.TryParse(typeof(PrimitiveType), value, false, out object result))
            {
                return (PrimitiveType)result;
            }
            return default;
        }

        public static GameObject SpawnPrimitive(PrimitiveType type)
        {
            var gameObject = GameObject.CreatePrimitive(type);
            return gameObject;
        }

        public static GameObject SpawnPrimitive(PrimitiveType type, params Vector3[] trs)
        {
            var go = SpawnPrimitive(type);
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

        public static GameObject SpawnPrimitive(string primitiveType, params Vector3[] trs)
        {
            var primitive = GetPrimitiveType(primitiveType);
            var go = SpawnPrimitive(primitive, trs);
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
            var vfx = UnityEngine.Object.Instantiate(vfxObj);
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
        public Action onRotated;
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
        public Action onTranslated;
        public BroadcastAt broadcastAt;
    }
}
