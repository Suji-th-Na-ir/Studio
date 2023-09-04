using UnityEngine;
using RuntimeInspectorNamespace;

namespace Terra.Studio
{
    [EditorDrawComponent("Terra.Studio.Push")]
    public class Push : MonoBehaviour, IComponent
    {
        public float drag = 0;
        public bool showResetButton = true;
        public Atom.PlaySfx playSFX;
        public Atom.PlayVfx playVFX;

        public (string type, string data) Export()
        {
            return default;
        }

        public void Import(EntityBasedComponent data)
        {

        }
    }
}
