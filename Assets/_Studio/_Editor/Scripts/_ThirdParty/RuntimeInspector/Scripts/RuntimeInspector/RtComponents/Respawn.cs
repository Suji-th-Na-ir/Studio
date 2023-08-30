using System;
using Newtonsoft.Json;
using PlayShifu.Terra;
using Terra.Studio;
using UnityEngine;


namespace RuntimeInspectorNamespace
{
    [EditorDrawComponent("Terra.Studio.Respawn")]
    public class Respawn : MonoBehaviour, IComponent
    {
        public Atom.PlaySfx PlaySFX = new Atom.PlaySfx();
        public Atom.PlayVfx PlayVFX = new Atom.PlayVfx();
        public string Broadcast = null;
        
        public void Start()
        {
            PlaySFX.Setup(gameObject);
            PlayVFX.Setup(gameObject);
        }
        
        public void Update()
        {
            if (!String.IsNullOrEmpty(Broadcast))
            {
                EditorOp.Resolve<DataProvider>().UpdateListenToTypes(this.GetInstanceID() + "_respawn", Broadcast);
            }
        }
        
        public (string type, string data) Export()
        {
            RespawnComponent respawnComp = new();
            {
                respawnComp.IsConditionAvailable = true;
                respawnComp.ConditionType = EditorOp.Resolve<DataProvider>().GetEnumValue(StartOn.OnPlayerCollide);
                respawnComp.ConditionData = "Player";

                respawnComp.IsBroadcastable = !string.IsNullOrEmpty(Broadcast);
                respawnComp.Broadcast = string.IsNullOrEmpty(Broadcast) ? null : Broadcast;

                respawnComp.canPlaySFX = PlaySFX.data.canPlay;
                respawnComp.canPlayVFX = PlayVFX.data.canPlay;

                respawnComp.sfxName = string.IsNullOrEmpty(PlaySFX.data.clipName) ? null : PlaySFX.data.clipName;
                respawnComp.vfxName = string.IsNullOrEmpty(PlayVFX.data.clipName) ? null : PlayVFX.data.clipName;

                respawnComp.sfxIndex = PlaySFX.data.clipIndex;
                respawnComp.vfxIndex = PlayVFX.data.clipIndex;

            }
            gameObject.TrySetTrigger(false, true);
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var data = JsonConvert.SerializeObject(respawnComp);
            return (type, data);
        }

        public void Import(EntityBasedComponent data)
        {
            RespawnComponent cc = JsonConvert.DeserializeObject<RespawnComponent>($"{data.data}");
            Broadcast = cc.Broadcast;

            PlaySFX.data.canPlay = cc.canPlaySFX;
            PlaySFX.data.clipIndex = cc.sfxIndex;
            PlaySFX.data.clipName = cc.sfxName;
            PlayVFX.data.canPlay = cc.canPlayVFX;
            PlayVFX.data.clipIndex = cc.vfxIndex;
            PlayVFX.data.clipName = cc.vfxName;
        }

      
        private void OnDestroy()
        {
            if (gameObject.TryGetComponent(out Collider collider) && !gameObject.TryGetComponent(out MeshRenderer _))
            {
                Destroy(collider);
            }
        }
    }
}
