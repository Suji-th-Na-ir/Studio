using System;
using UnityEngine;
using Newtonsoft.Json;
using RuntimeInspectorNamespace;
using UnityEngine.Serialization;

namespace Terra.Studio
{
    [EditorDrawComponent("Terra.Studio.Push")]
    public class Push : MonoBehaviour, IComponent
    {
        public float resistance = 0;
        public bool showResetButton = true;
        public Atom.PlaySfx PlaySFX = new();
        public Atom.PlayVfx PlayVFX = new();
        public Atom.Broadcast Broadcast = new();

        private string guid;

        public void Awake()
        {
            guid = GetInstanceID() + "_push";//Guid.NewGuid().ToString("N");
            Broadcast.Setup(gameObject, this.GetType().Name, guid);
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
                drag = resistance,
                showResetButton = showResetButton,
                listen = Listen.Always,
                
                IsBroadcastable = !string.IsNullOrEmpty(Broadcast.data.broadcastName),
                Broadcast = string.IsNullOrEmpty(Broadcast.data.broadcastName) ? "None" : Broadcast.data.broadcastName,
                broadcastTypeIndex = Broadcast.data.broadcastTypeIndex,
            };
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var data = JsonConvert.SerializeObject(component);
            return (type, data);
        }

        public void Import(EntityBasedComponent data)
        {
            var comp = JsonConvert.DeserializeObject<PushComponent>(data.data);
            resistance = comp.drag;
            showResetButton = comp.showResetButton;
            PlaySFX.data.canPlay = comp.canPlaySFX;
            PlaySFX.data.clipIndex = comp.sfxIndex;
            PlaySFX.data.clipName = comp.sfxName;
            PlayVFX.data.canPlay = comp.canPlayVFX;
            PlayVFX.data.clipIndex = comp.vfxIndex;
            PlayVFX.data.clipName = comp.vfxName;
            
            EditorOp.Resolve<DataProvider>().UpdateToListenList(guid,comp.Broadcast);
            
            Broadcast.data.broadcastName = string.IsNullOrEmpty(comp.Broadcast) ? "None" : comp.Broadcast;
            Broadcast.data.broadcastTypeIndex = comp.broadcastTypeIndex;
        }
    }
}
