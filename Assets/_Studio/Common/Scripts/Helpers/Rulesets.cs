using System.Collections;
using System.Collections.Generic;
using RTG;
using UnityEngine;

namespace Terra.Studio
{
    public class Rulesets 
    {
        public static void ApplyRuleset(Object obj)
        {
            if (!obj)
                return;

            var type = obj.GetType();
            if(type==typeof( GameObject))
            {
                new GameObjectRulesets().Apply(obj as GameObject);
            }

        }

        private class GameObjectRulesets
        {
            public void Apply(GameObject gm)
            {
                var allMesh = gm.transform.GetComponentsInChildren<MeshRenderer>();
                foreach (var mesh in allMesh)
                {
                    mesh.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }
            }

        }
    }
}
