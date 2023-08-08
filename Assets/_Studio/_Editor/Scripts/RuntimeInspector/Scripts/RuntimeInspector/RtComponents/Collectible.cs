using System;
using Newtonsoft.Json;
using Terra.Studio;
using Terra.Studio.RTEditor;
using UnityEngine;

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

        public StartOnCollectible Start = StartOnCollectible.OnPlayerCollide;
        public Atom.PlaySfx PlaySFX = new Atom.PlaySfx();
        public Atom.PlayVfx PlayVFX = new Atom.PlayVfx();
        public bool ShowScoreUI = false;
        public bool CanUpdateScore = false;
        public float ScoreValue = 0;
        public string Broadcast = null;

        public (string type, string data) Export()
        {
            CollectableComponent collectable = new();
            {
                collectable.IsConditionAvailable = true;
                collectable.ConditionType = GetStartEvent();
                collectable.ConditionData = EditorOp.Resolve<DataProvider>().GetEnumConditionDataValue(Start);
                collectable.IsBroadcastable = !string.IsNullOrEmpty(Broadcast);
                collectable.Broadcast = string.IsNullOrEmpty(Broadcast) ? null : Broadcast;

                collectable.canPlaySFX = PlaySFX.canPlay;
                collectable.canPlayVFX = PlayVFX.canPlay;

                collectable.sfxName = string.IsNullOrEmpty(PlaySFX.clipName) ? null : PlaySFX.clipName;
                collectable.vfxName = string.IsNullOrEmpty(PlayVFX.clipName) ? null : PlayVFX.clipName;

                collectable.sfxIndex = PlaySFX.clipIndex;
                collectable.vfxIndex = PlayVFX.clipIndex;

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
                Debug.Log(result);
                Start = (StartOnCollectible)result;
            }

            Broadcast = cc.Broadcast;

            PlaySFX.canPlay = cc.canPlaySFX;
            PlaySFX.clipIndex = cc.sfxIndex;
            PlaySFX.clipName = cc.sfxName;
            PlayVFX.canPlay = cc.canPlayVFX;
            PlayVFX.clipIndex = cc.vfxIndex;
            PlayVFX.clipName = cc.vfxName;
        }

        public string GetStartEvent()
        {
            var eventName = EditorOp.Resolve<DataProvider>().GetEnumValue(Start);
            return eventName;
        }

    }
}
