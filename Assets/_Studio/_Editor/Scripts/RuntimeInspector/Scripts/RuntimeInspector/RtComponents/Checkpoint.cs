using UnityEngine;
using Newtonsoft.Json;
using Terra.Studio.RTEditor;
using RuntimeInspectorNamespace;

namespace Terra.Studio
{
    [EditorDrawComponent("Terra.Studio.Checkpoint")]
    public class Checkpoint : MonoBehaviour, IComponent
    {
        public Atom.PlaySfx PlaySFX = new();
        public Atom.PlayVfx PlayVFX = new();
        public string Broadcast = null;

        public (string type, string data) Export()
        {
            var component = new CheckpointComponent()
            {
                canPlaySFX = PlaySFX.canPlay,
                sfxName = PlaySFX.clipName,
                sfxIndex = PlaySFX.clipIndex,
                canPlayVFX = PlayVFX.canPlay,
                vfxName = PlayVFX.clipName,
                vfxIndex = PlayVFX.clipIndex,
                IsBroadcastable = !string.IsNullOrEmpty(Broadcast),
                Broadcast = Broadcast,
                respawnPoint = transform.position,
                IsConditionAvailable = true,
                ConditionType = "Terra.Studio.TriggerAction",
                ConditionData = "Player"
            };
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var data = JsonConvert.SerializeObject(component);
            return (type, data);
        }

        public void Import(EntityBasedComponent data)
        {
            var json = (string)data.data;
            var component = JsonConvert.DeserializeObject<CheckpointComponent>(json);
            PlaySFX.canPlay = component.canPlaySFX;
            PlaySFX.clipName = component.sfxName;
            PlaySFX.clipIndex = component.sfxIndex;
            PlayVFX.canPlay = component.canPlayVFX;
            PlayVFX.clipName = component.vfxName;
            PlayVFX.clipIndex = component.vfxIndex;
            Broadcast = component.Broadcast;
        }
    }
}
