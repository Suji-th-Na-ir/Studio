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

        public (string type, string data) Export()
        {
            var data = new ClickComponent()
            {
                canPlaySFX = playSFX.data.canPlay,
                sfxName = playSFX.data.clipName,
                sfxIndex = playSFX.data.clipIndex,
                canPlayVFX = playVFX.data.canPlay,
                vfxName = playVFX.data.clipName,
                vfxIndex = playVFX.data.clipIndex,
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
            playSFX.data.canPlay = obj.canPlaySFX;
            playSFX.data.clipName = obj.sfxName;
            playSFX.data.clipIndex = obj.sfxIndex;
            playVFX.data.canPlay = obj.canPlayVFX;
            playVFX.data.clipName = obj.vfxName;
            playVFX.data.clipIndex = obj.vfxIndex;
            broadcast = obj.Broadcast;
            executeMultipleTimes = obj.listen == Listen.Always;
            EditorOp.Resolve<UILogicDisplayProcessor>().UpdateBroadcastString(broadcast, ""
                    , new ComponentDisplayDock() { componentGameObject = gameObject, componentType = GetType().Name });
        }
    }
}
