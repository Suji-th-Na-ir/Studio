using UnityEngine;
using Terra.Studio;
using Newtonsoft.Json;

namespace RuntimeInspectorNamespace
{
    [EditorDrawComponent("Terra.Studio.Checkpoint")]
    public class Checkpoint : MonoBehaviour, IComponent
    {
        public Atom.PlaySfx PlaySFX = new();
        public Atom.PlayVfx PlayVFX = new();
        [DisplayName("Broadcast")]
        public string Broadcast = null;

        public void Start()
        {
            PlaySFX.Setup<Checkpoint>(gameObject);
            PlayVFX.Setup<Checkpoint>(gameObject);
        }

        public (string type, string data) Export()
        {
            var component = new CheckpointComponent()
            {
                canPlaySFX = PlaySFX.data.canPlay,
                sfxName = PlaySFX.data.clipName,
                sfxIndex = PlaySFX.data.clipIndex,
                canPlayVFX = PlayVFX.data.canPlay,
                vfxName = PlayVFX.data.clipName,
                vfxIndex = PlayVFX.data.clipIndex,
                IsBroadcastable = !string.IsNullOrEmpty(Broadcast),
                respawnPoint = transform.position,
                IsConditionAvailable = true,
                ConditionType = "Terra.Studio.TriggerAction",
                ConditionData = "Player",
                Broadcast = Broadcast
            };
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var data = JsonConvert.SerializeObject(component);
            return (type, data);
        }

        public void Import(EntityBasedComponent data)
        {
            var json = data.data;
            var component = JsonConvert.DeserializeObject<CheckpointComponent>(json);
            PlaySFX.data.canPlay = component.canPlaySFX;
            PlaySFX.data.clipName = component.sfxName;
            PlaySFX.data.clipIndex = component.sfxIndex;
            PlayVFX.data.canPlay = component.canPlayVFX;
            PlayVFX.data.clipName = component.vfxName;
            PlayVFX.data.clipIndex = component.vfxIndex;
            Broadcast = component.Broadcast;
            EditorOp.Resolve<UILogicDisplayProcessor>().ImportVisualisation(gameObject, GetType().Name, Broadcast, null);
        }
    }
}
