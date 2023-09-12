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
        public Atom.PlaySfx PlaySFX = new();
        public Atom.PlayVfx PlayVFX = new();
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
                canPlaySFX = PlaySFX.data.CanPlay,
                sfxName = PlaySFX.data.ClipName,
                sfxIndex = PlaySFX.data.ClipIndex,
                canPlayVFX = PlayVFX.data.CanPlay,
                vfxName = PlayVFX.data.ClipName,
                vfxIndex = PlayVFX.data.ClipIndex,
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
            PlaySFX.data.CanPlay = comp.canPlaySFX;
            PlaySFX.data.ClipIndex = comp.sfxIndex;
            PlaySFX.data.ClipName = comp.sfxName;
            PlayVFX.data.CanPlay = comp.canPlayVFX;
            PlayVFX.data.ClipIndex = comp.vfxIndex;
            PlayVFX.data.ClipName = comp.vfxName;
            Broadcast = comp.Broadcast;
            EditorOp.Resolve<UILogicDisplayProcessor>().ImportVisualisation(gameObject, GetType().Name, Broadcast, null);
        }
    }
}
