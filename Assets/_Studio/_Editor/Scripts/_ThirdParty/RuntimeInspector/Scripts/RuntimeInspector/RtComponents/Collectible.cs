using System;
using Newtonsoft.Json;
using Terra.Studio;
using UnityEngine;
using UnityEngine.Serialization;

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

        public StartOnCollectible start = StartOnCollectible.OnPlayerCollide;
        public Atom.PlaySfx PlaySFX = new Atom.PlaySfx();
        public Atom.PlayVfx PlayVFX = new Atom.PlayVfx();
        public bool ShowScoreUI = false;
        public bool CanUpdateScore = false;
        public float ScoreValue = 0;
        public string Broadcast = null;
        
        public void Start()
        {

        }

        public (string type, string data) Export()
        {
            CollectableComponent collectable = new();
            {
                collectable.IsConditionAvailable = true;
                collectable.ConditionType = GetStartEvent();
                collectable.ConditionData = EditorOp.Resolve<DataProvider>().GetEnumConditionDataValue(start);
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
                collectable.showScoreUI = ShowScoreUI;
            }
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var data = JsonConvert.SerializeObject(collectable);
            return (type, data);
        }

        public void Import(EntityBasedComponent cdata)
        {
            CollectableComponent cc = JsonConvert.DeserializeObject<CollectableComponent>($"{cdata.data}");
            CanUpdateScore = cc.canUpdateScore;
            ShowScoreUI = cc.showScoreUI;
            ScoreValue = cc.scoreValue;

            if (EditorOp.Resolve<DataProvider>().TryGetEnum(cc.ConditionType, typeof(StartOnCollectible), out object result))
            {
                start = (StartOnCollectible)result;
            }

            Broadcast = cc.Broadcast;

            PlaySFX.data.canPlay = cc.canPlaySFX;
            PlaySFX.data.clipIndex = cc.sfxIndex;
            PlaySFX.data.clipName = cc.sfxName;
            PlayVFX.data.canPlay = cc.canPlayVFX;
            PlayVFX.data.clipIndex = cc.vfxIndex;
            PlayVFX.data.clipName = cc.vfxName;
        }

        public string GetStartEvent()
        {
            var eventName = EditorOp.Resolve<DataProvider>().GetEnumValue(start);
            return eventName;
        }

    }
}
