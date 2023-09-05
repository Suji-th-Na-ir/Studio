using UnityEngine;
using RuntimeInspectorNamespace;

namespace Terra.Studio
{
    public class Switch : MonoBehaviour, IComponent
    {
        public enum StartOn
        {
            OnClick,
            OnPlayerCollide,
            OnObjectCollide
        }

        public StartOn switchOn;
        [Header("When Switch is \"On\"")]
        [AliasDrawer("Broadcast")] public Atom.Broadcast broadcastWhenOn = new();
        [AliasDrawer("Play SFX")] public Atom.PlaySfx playSFXWhenOn = new();
        [AliasDrawer("Play VFX")] public Atom.PlayVfx playVFXWhenOn = new();
        [Header("When Switch is \"Off\"")]
        [AliasDrawer("Broadcast")] public Atom.Broadcast broadcastWhenOff = new();
        [AliasDrawer("Play SFX")] public Atom.PlaySfx playSFXWhenOff = new();
        [AliasDrawer("Play VFX")] public Atom.PlayVfx playVFXWhenOff = new();

        public (string type, string data) Export()
        {
            return default;
        }

        public void Import(EntityBasedComponent data)
        {

        }
    }
}
