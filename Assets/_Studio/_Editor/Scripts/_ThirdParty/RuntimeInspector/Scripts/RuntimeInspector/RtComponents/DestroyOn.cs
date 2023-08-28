using System.Collections.Generic;
using Newtonsoft.Json;
using PlayShifu.Terra;
using Terra.Studio;
using UnityEngine;
using UnityEngine.Serialization;

namespace RuntimeInspectorNamespace
{
    public enum DestroyOnEnum
    {
        [EditorEnumField("Terra.Studio.TriggerAction", "Player")]
        OnPlayerCollide,
        [EditorEnumField("Terra.Studio.MouseAction", "OnClick")]
        OnClick,
        [EditorEnumField("Terra.Studio.Listener")]
        BroadcastListen
    }
    
    [EditorDrawComponent("Terra.Studio.DestroyOn")]
    public class DestroyOn : MonoBehaviour, IComponent
    {
        public Atom.StartOn startOn = new Atom.StartOn();
        public Atom.PlaySfx PlaySFX = new Atom.PlaySfx();
        public Atom.PlayVfx PlayVFX = new Atom.PlayVfx();
        public string Broadcast = null;

        public void Start()
        {
            startOn.Setup(gameObject);
            PlaySFX.Setup(gameObject);
            PlayVFX.Setup(gameObject);
        }

        public (string type, string data) Export()
        {
            DestroyOnComponent destroyOn = new();
            {
                destroyOn.IsConditionAvailable = true;
                destroyOn.ConditionType = startOn.data.startName;
                destroyOn.ConditionData = startOn.data.listenName;
                destroyOn.IsBroadcastable = !string.IsNullOrEmpty(Broadcast);
                destroyOn.Broadcast = string.IsNullOrEmpty(Broadcast) ? null : Broadcast;
                destroyOn.BroadcastListen = string.IsNullOrEmpty(startOn.data.listenName) ? null : startOn.data.listenName;

                destroyOn.canPlaySFX = PlaySFX.data.canPlay;
                destroyOn.canPlayVFX = PlayVFX.data.canPlay;

                destroyOn.sfxName = Helper.GetSfxClipNameByIndex(PlaySFX.data.clipIndex);
                destroyOn.vfxName = Helper.GetVfxClipNameByIndex(PlayVFX.data.clipIndex);

                destroyOn.sfxIndex = PlaySFX.data.clipIndex;
                destroyOn.vfxIndex = PlayVFX.data.clipIndex;
            }
            gameObject.TrySetTrigger(false, true);
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var data = JsonConvert.SerializeObject(destroyOn, Formatting.Indented);
            
            Debug.Log("export "+data);
            
            return (type, data);
        }

        public void Import(EntityBasedComponent cdata)
        {
            DestroyOnComponent cc = JsonConvert.DeserializeObject<DestroyOnComponent>($"{cdata.data}");
            
            startOn.data.startIndex = Helper.GetEnumIndexByString<DestroyOnEnum>(cc.ConditionType) ;
            startOn.data.listenIndex = Helper.GetListenIndex(cc.ConditionData);
            
            startOn.data.startName = cc.ConditionType;
            startOn.data.listenName = cc.ConditionData;

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
