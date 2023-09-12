using UnityEngine;
using Newtonsoft.Json;
using RuntimeInspectorNamespace;

namespace Terra.Studio
{
    [EditorDrawComponent("Terra.Studio.Teleport")]
    public class Teleport : MonoBehaviour, IComponent
    {
        public Vector3 teleportTo;
        public Atom.PlaySfx playSFX = new();
        public Atom.PlayVfx playVFX = new();
        public string broadcast = null;
        public bool executeMultipleTimes = true;

        private void Start()
        {
            if (teleportTo == Vector3.zero)
            {
                var currentPos = transform.localPosition;
                currentPos.y += 2f;
                currentPos.z += 2f;
                teleportTo = currentPos;
            }
        }

        public (string type, string data) Export()
        {
            var data = new TeleportComponent()
            {
                teleportTo = teleportTo,
                canPlaySFX = playSFX.data.CanPlay,
                sfxName = playSFX.data.ClipName,
                sfxIndex = playSFX.data.ClipIndex,
                canPlayVFX = playVFX.data.CanPlay,
                vfxName = playVFX.data.ClipName,
                vfxIndex = playVFX.data.ClipIndex,
                IsBroadcastable = !string.IsNullOrEmpty(broadcast),
                Broadcast = broadcast,
                IsConditionAvailable = true,
                ConditionType = "Terra.Studio.TriggerAction",
                ConditionData = "Player",
                listen = executeMultipleTimes ? Listen.Always : Listen.Once
            };
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var json = JsonConvert.SerializeObject(data);
            return (type, json);
        }

        public void Import(EntityBasedComponent data)
        {
            var obj = JsonConvert.DeserializeObject<TeleportComponent>(data.data);
            teleportTo = obj.teleportTo;
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
