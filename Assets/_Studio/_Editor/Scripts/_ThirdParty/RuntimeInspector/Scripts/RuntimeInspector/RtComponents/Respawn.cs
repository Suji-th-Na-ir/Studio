using Newtonsoft.Json;
using PlayShifu.Terra;
using Terra.Studio;
using UnityEngine;


namespace RuntimeInspectorNamespace
{
    [EditorDrawComponent("Terra.Studio.Respawn"), AliasDrawer("Kill Player")]

    public class Respawn : MonoBehaviour, IComponent
    {
        public Atom.PlaySfx PlaySFX = new();
        public Atom.PlayVfx PlayVFX = new();
        [AliasDrawer("Broadcast")]
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
                comp.canPlaySFX = PlaySFX.data.canPlay;
                comp.canPlayVFX = PlayVFX.data.canPlay;
                comp.sfxName = PlaySFX.data.clipName;
                comp.vfxName = PlayVFX.data.clipName;
                comp.sfxIndex = PlaySFX.data.clipIndex;
                comp.vfxIndex = PlayVFX.data.clipIndex;
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
            PlaySFX.data.canPlay = comp.canPlaySFX;
            PlaySFX.data.clipIndex = comp.sfxIndex;
            PlaySFX.data.clipName = comp.sfxName;
            PlayVFX.data.canPlay = comp.canPlayVFX;
            PlayVFX.data.clipIndex = comp.vfxIndex;
            PlayVFX.data.clipName = comp.vfxName;
            EditorOp.Resolve<UILogicDisplayProcessor>().ImportVisualisation(gameObject, GetType().Name, Broadcast, null);
        }
    }
}
