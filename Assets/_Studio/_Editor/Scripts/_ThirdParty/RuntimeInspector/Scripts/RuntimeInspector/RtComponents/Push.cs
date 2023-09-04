using UnityEngine;
using Newtonsoft.Json;
using RuntimeInspectorNamespace;

namespace Terra.Studio
{
    [EditorDrawComponent("Terra.Studio.Push")]
    public class Push : MonoBehaviour, IComponent
    {
        public float resistance = 0;
        public bool showResetButton = true;
        public Atom.PlaySfx playSFX = new();
        public Atom.PlayVfx playVFX = new();
        public Atom.Broadcast broadcast = new();

        public (string type, string data) Export()
        {
            var component = new PushComponent()
            {
                IsConditionAvailable = true,
                ConditionType = "Terra.Studio.TriggerAction",
                ConditionData = "Any",
                canPlaySFX = playSFX.data.canPlay,
                sfxName = playSFX.data.clipName,
                sfxIndex = playSFX.data.clipIndex,
                canPlayVFX = playVFX.data.canPlay,
                vfxName = playVFX.data.clipName,
                vfxIndex = playVFX.data.clipIndex,
                drag = resistance,
                showResetButton = showResetButton,
                listen = Listen.Always
            };
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var data = JsonConvert.SerializeObject(component);
            return (type, data);
        }

        public void Import(EntityBasedComponent data)
        {
            var component = JsonConvert.DeserializeObject<PushComponent>(data.data);
            resistance = component.drag;
            showResetButton = component.showResetButton;
            playSFX.data.canPlay = component.canPlaySFX;
            playSFX.data.clipIndex = component.sfxIndex;
            playSFX.data.clipName = component.sfxName;
            playVFX.data.canPlay = component.canPlayVFX;
            playVFX.data.clipIndex = component.vfxIndex;
            playVFX.data.clipName = component.vfxName;
        }
    }
}
