using System;
using Newtonsoft.Json;
using Terra.Studio;
using Terra.Studio.RTEditor;
using UnityEngine;


namespace RuntimeInspectorNamespace
{
    [EditorDrawComponent("Terra.Studio.Respawn")]
    public class Respawn : MonoBehaviour, IComponent
    {
        public Atom.PlaySfx PlaySFX = new Atom.PlaySfx();
        public Atom.PlayVfx PlayVFX = new Atom.PlayVfx();
        public string Broadcast = null;
        public Vector3 respawnPoint;

        public (string type, string data) Export()
        {
            RespawnComponent respawnComp = new();
            {
                respawnComp.IsBroadcastable = !string.IsNullOrEmpty(Broadcast);
                respawnComp.Broadcast = string.IsNullOrEmpty(Broadcast) ? null : Broadcast;

                respawnComp.canPlaySFX = PlaySFX.canPlay;
                respawnComp.canPlayVFX = PlayVFX.canPlay;

                respawnComp.sfxName = string.IsNullOrEmpty(PlaySFX.clipName) ? null : PlaySFX.clipName;
                respawnComp.vfxName = string.IsNullOrEmpty(PlayVFX.clipName) ? null : PlaySFX.clipName;

                respawnComp.sfxIndex = PlaySFX.clipIndex;
                respawnComp.vfxIndex = PlayVFX.clipIndex;

            }
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var data = JsonConvert.SerializeObject(respawnComp);
            return (type, data);
        }

        public void Import(EntityBasedComponent data)
        {
            RespawnComponent cc = JsonConvert.DeserializeObject<RespawnComponent>($"{data.data}");
           

            if (EditorOp.Resolve<DataProvider>().TryGetEnum(cc.ConditionType, typeof(StartOn), out object result))
            {
               // Start = (StartOn)result;
            }
            Broadcast = cc.Broadcast;

            PlaySFX.canPlay = cc.canPlaySFX;
            PlaySFX.clipIndex = cc.sfxIndex;
            PlaySFX.clipName = cc.sfxName;
            PlayVFX.canPlay = cc.canPlayVFX;
            PlayVFX.clipIndex = cc.vfxIndex;
            PlayVFX.clipName = cc.vfxName;
        }

       
    }
}
