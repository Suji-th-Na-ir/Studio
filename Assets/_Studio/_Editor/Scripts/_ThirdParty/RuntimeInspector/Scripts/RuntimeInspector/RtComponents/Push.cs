using UnityEngine;
using Newtonsoft.Json;
using RuntimeInspectorNamespace;

namespace Terra.Studio
{
    [EditorDrawComponent("Terra.Studio.Push")]
    public class Push : MonoBehaviour, IComponent
    {
        public float resistance = 0;
        [DisplayName("Reset\nButton")]
        public bool showResetButton = true;
        public Atom.PlaySfx PlaySFX = new();
        public Atom.PlayVfx PlayVFX = new();
        [DisplayName("Broadcast")]
        public string Broadcast = null;

        public void Awake()
        {
            PlaySFX.Setup<DestroyOn>(gameObject);
            PlayVFX.Setup<DestroyOn>(gameObject);
        }

        public (string type, string data) Export()
        {
            var component = new PushComponent()
            {
                IsConditionAvailable = true,
                ConditionType = "Terra.Studio.TriggerAction",
                ConditionData = "Any",
                canPlaySFX = PlaySFX.data.canPlay,
                sfxName = PlaySFX.data.clipName,
                sfxIndex = PlaySFX.data.clipIndex,
                canPlayVFX = PlayVFX.data.canPlay,
                vfxName = PlayVFX.data.clipName,
                vfxIndex = PlayVFX.data.clipIndex,
                mass = resistance,
                showResetButton = showResetButton,
                listen = Listen.Always,
                Broadcast = Broadcast,
                IsBroadcastable = !string.IsNullOrEmpty(Broadcast)
            };
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var data = JsonConvert.SerializeObject(component);
            return (type, data);
        }

        public void Import(EntityBasedComponent data)
        {
            var comp = JsonConvert.DeserializeObject<PushComponent>(data.data);
            resistance = comp.mass;
            showResetButton = comp.showResetButton;
            PlaySFX.data.canPlay = comp.canPlaySFX;
            PlaySFX.data.clipIndex = comp.sfxIndex;
            PlaySFX.data.clipName = comp.sfxName;
            PlayVFX.data.canPlay = comp.canPlayVFX;
            PlayVFX.data.clipIndex = comp.vfxIndex;
            PlayVFX.data.clipName = comp.vfxName;
            Broadcast = comp.Broadcast;
            EditorOp.Resolve<UILogicDisplayProcessor>().ImportVisualisation(gameObject, GetType().Name, Broadcast, null);
        }
    }
}
