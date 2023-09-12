using Newtonsoft.Json;
using PlayShifu.Terra;
using Terra.Studio;
using UnityEngine;


namespace RuntimeInspectorNamespace
{
    [EditorDrawComponent("Terra.Studio.Respawn")]
    public class Respawn : MonoBehaviour, IComponent
    {
        public Atom.PlaySfx PlaySFX = new();
        public Atom.PlayVfx PlayVFX = new();
        public string Broadcast = null;

        private void Awake()
        {
            PlaySFX.Setup<Respawn>(gameObject);
            PlayVFX.Setup<Respawn>(gameObject);
        }

        public (string type, string data) Export()
        {
            RespawnComponent comp = new();
            {
                comp.IsConditionAvailable = true;
                comp.ConditionType = EditorOp.Resolve<DataProvider>().GetEnumValue(StartOn.OnPlayerCollide);
                comp.ConditionData = "Player";
                comp.IsBroadcastable = !string.IsNullOrEmpty(Broadcast);
                comp.Broadcast = Broadcast;
                comp.canPlaySFX = PlaySFX.data.CanPlay;
                comp.canPlayVFX = PlayVFX.data.CanPlay;
                comp.sfxName = PlaySFX.data.ClipName;
                comp.vfxName = PlayVFX.data.ClipName;
                comp.sfxIndex = PlaySFX.data.ClipIndex;
                comp.vfxIndex = PlayVFX.data.ClipIndex;
            }
            gameObject.TrySetTrigger(false, true);
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var data = JsonConvert.SerializeObject(comp);
            return (type, data);
        }

        public void Import(EntityBasedComponent data)
        {
            RespawnComponent comp = JsonConvert.DeserializeObject<RespawnComponent>(data.data);
            Broadcast = comp.Broadcast;
            PlaySFX.data.CanPlay = comp.canPlaySFX;
            PlaySFX.data.ClipIndex = comp.sfxIndex;
            PlaySFX.data.ClipName = comp.sfxName;
            PlayVFX.data.CanPlay = comp.canPlayVFX;
            PlayVFX.data.ClipIndex = comp.vfxIndex;
            PlayVFX.data.ClipName = comp.vfxName;
            EditorOp.Resolve<UILogicDisplayProcessor>().ImportVisualisation(gameObject, GetType().Name, Broadcast, null);
        }
    }
}
