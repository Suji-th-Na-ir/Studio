using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
            var sfx = Resources.Load<AudioClip>($"sfx/{sfxName}") as AudioClip;
            audioSource.clip = sfx;
            audioSource.loop = false;
            audioSource.Play();
            var destroyAfter = go.AddComponent<DestroyAfter>();
            destroyAfter.seconds = 2f;
        }

        public static void PlayVFX(string vfxName, Vector3 position)
        {
            var vfxObj = Resources.Load<GameObject>($"vfx/{vfxName}") as GameObject;
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

        public struct RotateByParams
        {
            public float rotateBy;
            public float rotationSpeed;
            public int rotationTimes;
            public RotateComponent.Axis axis;
            public RotateComponent.Direction direction;
            public RotateComponent.BroadcastAt broadcastAt;
            public bool shouldPingPong;
            public Action onRotated;
            public bool shouldPause;
            public float pauseForTime;
            public GameObject targetObj;
        }
    }
}
