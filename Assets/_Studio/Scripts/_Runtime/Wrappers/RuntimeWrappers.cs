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
    }
}
