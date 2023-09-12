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
                canPlaySFX = PlaySFX.data.CanPlay,
                sfxName = PlaySFX.data.ClipName,
                sfxIndex = PlaySFX.data.ClipIndex,
                canPlayVFX = PlayVFX.data.CanPlay,
                vfxName = PlayVFX.data.ClipName,
                vfxIndex = PlayVFX.data.ClipIndex,
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
            PlaySFX.data.CanPlay = component.canPlaySFX;
            PlaySFX.data.ClipName = component.sfxName;
            PlaySFX.data.ClipIndex = component.sfxIndex;
            PlayVFX.data.CanPlay = component.canPlayVFX;
            PlayVFX.data.ClipName = component.vfxName;
            PlayVFX.data.ClipIndex = component.vfxIndex;
            Broadcast = component.Broadcast;
            EditorOp.Resolve<UILogicDisplayProcessor>().ImportVisualisation(gameObject, GetType().Name, Broadcast, null);
        }
    }
}
