using Newtonsoft.Json;
using PlayShifu.Terra;
using Terra.Studio;
using UnityEngine;
using System;

namespace RuntimeInspectorNamespace
{
    [EditorDrawComponent("Terra.Studio.Collectable")]
    public class Collectible : MonoBehaviour, IComponent
    {
        public enum StartOnCollectible
        {     
            [EditorEnumField("Terra.Studio.TriggerAction", "Player")]
            OnPlayerCollide,
            [EditorEnumField("Terra.Studio.MouseAction", "OnClick")]
            OnClick
        }

        public Atom.StartOn startOn = new Atom.StartOn();
        public Atom.PlaySfx PlaySFX = new Atom.PlaySfx();
        public Atom.PlayVfx PlayVFX = new Atom.PlayVfx();
        // public bool ShowScoreUI = false;
        public bool CanUpdateScore = false;
        public float ScoreValue = 0;
        public string Broadcast = null;
        
        public void Awake()
        {
            startOn.Setup(gameObject, Helper.GetEnumValuesAsStrings<StartOnCollectible>());
            PlaySFX.Setup(gameObject);
            PlayVFX.Setup(gameObject);
        }
        
        public void Update()
        {
            if (!String.IsNullOrEmpty(Broadcast))
            {
                Helper.UpdateListenToTypes(this.GetInstanceID()+"_collectible", Broadcast);
            }
        }

        public (string type, string data) Export()
        {
            CollectableComponent collectable = new();
            {
                collectable.IsConditionAvailable = true;
                collectable.ConditionType = GetStartEvent();
                collectable.ConditionData = GetStartCondition();
                collectable.listenIndex = startOn.data.listenIndex;
                collectable.IsBroadcastable = !string.IsNullOrEmpty(Broadcast);
                collectable.Broadcast = string.IsNullOrEmpty(Broadcast) ? null : Broadcast;

                collectable.canPlaySFX = PlaySFX.data.canPlay;
                collectable.canPlayVFX = PlayVFX.data.canPlay;

                collectable.sfxName = string.IsNullOrEmpty(PlaySFX.data.clipName) ? null : PlaySFX.data.clipName;
                collectable.vfxName = string.IsNullOrEmpty(PlayVFX.data.clipName) ? null : PlayVFX.data.clipName;

                collectable.sfxIndex = PlaySFX.data.clipIndex;
                collectable.vfxIndex = PlayVFX.data.clipIndex;

                collectable.canUpdateScore = CanUpdateScore;
                collectable.scoreValue = ScoreValue;
                collectable.showScoreUI = true;
            }
            
            gameObject.TrySetTrigger(false, true);
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var data = JsonConvert.SerializeObject(collectable);
            return (type, data);
        }
        
        public string GetStartEvent(string _input = null)
        {
            int index = startOn.data.startIndex;
            string inputString = ((StartOnCollectible)index).ToString();
            if (!string.IsNullOrEmpty(_input))
                inputString = _input;
            
            if (Enum.TryParse(inputString, out StartOnCollectible enumValue))
            {
                var eventName = EditorOp.Resolve<DataProvider>().GetEnumValue(enumValue);
                return eventName;
            }
            return EditorOp.Resolve<DataProvider>().GetEnumValue(StartOnCollectible.OnClick);
        }


        public string GetStartCondition(string _input = null)
        {
            int index = startOn.data.startIndex;
            string inputString = ((StartOnCollectible)index).ToString();
            if (!string.IsNullOrEmpty(_input))
                inputString = _input;
            
            if (inputString.ToLower().Contains("listen"))
            {
                return Helper.GetListenString(startOn.data.listenIndex);
            }
            else
            {
                if (Enum.TryParse(inputString, out StartOnCollectible enumValue))
                {
                    return EditorOp.Resolve<DataProvider>().GetEnumConditionDataValue(enumValue);
                }
                return EditorOp.Resolve<DataProvider>().GetEnumConditionDataValue(StartOnCollectible.OnClick);
            }
        }

        public void Import(EntityBasedComponent cdata)
        {
            CollectableComponent cc = JsonConvert.DeserializeObject<CollectableComponent>($"{cdata.data}");
            CanUpdateScore = cc.canUpdateScore;
            // ShowScoreUI = cc.showScoreUI;
            ScoreValue = cc.scoreValue;
            Broadcast = cc.Broadcast;

            PlaySFX.data.canPlay = cc.canPlaySFX;
            PlaySFX.data.clipIndex = cc.sfxIndex;
            PlaySFX.data.clipName = cc.sfxName;
            PlayVFX.data.canPlay = cc.canPlayVFX;
            PlayVFX.data.clipIndex = cc.vfxIndex;
            PlayVFX.data.clipName = cc.vfxName;
            
            if (EditorOp.Resolve<DataProvider>().TryGetEnum(cc.ConditionType, typeof(StartOnCollectible), out object result))
            {
                startOn.data.startIndex = (int)(StartOnCollectible)result;
            }

            if (cc.ConditionType.ToLower().Contains("listen"))
            {
                Helper.AddToListenList(cc.ConditionData);
            }
            startOn.data.listenIndex = cc.listenIndex;
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
