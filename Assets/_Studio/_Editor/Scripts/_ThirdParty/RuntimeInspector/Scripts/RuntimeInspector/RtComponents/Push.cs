using UnityEngine;
using RuntimeInspectorNamespace;

namespace Terra.Studio
{
    [EditorDrawComponent("Terra.Studio.Push")]
    public class Push : MonoBehaviour, IComponent
    {
        public float resistance = 0;
        public bool showResetButton = true;
        public Atom.PlaySfx playSFX;
        public Atom.PlayVfx playVFX;
        public BroadcastAtForPushObjects broadcastAt;

        public (string type, string data) Export()
        {
            var component = new PushComponent()
            {

            };
            return default;
        }

        public void Import(EntityBasedComponent data)
        {

        }
    }
}
