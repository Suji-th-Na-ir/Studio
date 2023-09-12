using UnityEngine;
using Newtonsoft.Json;
using RuntimeInspectorNamespace;

namespace Terra.Studio
{
    [EditorDrawComponent("Terra.Studio.Click")]
    public class Click : MonoBehaviour, IComponent
    {
        public string broadcast = null;
        public Atom.PlaySfx playSFX = new();
        public Atom.PlayVfx playVFX = new();
        public bool executeMultipleTimes = true;

        public void Awake()
        {
            playSFX.Setup<Click>(gameObject);
            playVFX.Setup<Click>(gameObject);
        }

        public (string type, string data) Export()
        {
            var data = new ClickComponent()
            {
                canPlaySFX = playSFX.data.CanPlay,
                sfxName = playSFX.data.ClipName,
                sfxIndex = playSFX.data.ClipIndex,
                canPlayVFX = playVFX.data.CanPlay,
                vfxName = playVFX.data.ClipName,
                vfxIndex = playVFX.data.ClipIndex,
                IsBroadcastable = !string.IsNullOrEmpty(broadcast),
                Broadcast = broadcast,
                IsConditionAvailable = true,
                ConditionType = "Terra.Studio.MouseAction",
                ConditionData = "OnClick",
                listen = executeMultipleTimes ? Listen.Always : Listen.Once
            };
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var json = JsonConvert.SerializeObject(data);
            return (type, json);
        }

        public void Import(EntityBasedComponent data)
        {
            var obj = JsonConvert.DeserializeObject<ClickComponent>(data.data);
            playSFX.data.CanPlay = obj.canPlaySFX;
            playSFX.data.ClipName = obj.sfxName;
            playSFX.data.ClipIndex = obj.sfxIndex;
            playVFX.data.CanPlay = obj.canPlayVFX;
            playVFX.data.ClipName = obj.vfxName;
            playVFX.data.ClipIndex = obj.vfxIndex;
            broadcast = obj.Broadcast;
            executeMultipleTimes = obj.listen == Listen.Always;
            EditorOp.Resolve<UILogicDisplayProcessor>().ImportVisualisation(gameObject, GetType().Name, broadcast, null);
        }
    }
}
