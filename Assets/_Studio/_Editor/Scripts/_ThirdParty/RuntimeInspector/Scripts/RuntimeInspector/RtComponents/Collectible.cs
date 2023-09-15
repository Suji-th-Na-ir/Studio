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
            [EditorEnumField("Terra.Studio.TriggerAction", "Player"),DisplayName("Player Touches")]
            OnPlayerCollide,
            [EditorEnumField("Terra.Studio.MouseAction", "OnClick"),DisplayName("Clicked")]
            OnClick
        }
        [DisplayName("CollectWhen")]
        public Atom.StartOn startOn = new();
        public Atom.PlaySfx PlaySFX = new();
        public Atom.PlayVfx PlayVFX = new();
        public Atom.ScoreData Score = new();
        [DisplayName("Broadcast")]
        public string Broadcast = null;

        public void Awake()
        {
            Score.instanceId = Guid.NewGuid().ToString("N");
            startOn.Setup(gameObject, Helper.GetEnumWithDisplayNames<StartOnCollectible>(), GetType().Name,false);
            PlaySFX.Setup<Collectible>(gameObject);
            PlayVFX.Setup<Collectible>(gameObject);
        }

        public (string type, string data) Export()
        {
            CollectableComponent comp = new();
            {
                comp.IsConditionAvailable = true;
                comp.ConditionType = GetStartEvent();
                comp.ConditionData = GetStartCondition();
                comp.IsBroadcastable = !string.IsNullOrEmpty(Broadcast);
                comp.Broadcast = Broadcast;
                comp.canPlaySFX = PlaySFX.data.canPlay;
                comp.canPlayVFX = PlayVFX.data.canPlay;
                comp.sfxName = string.IsNullOrEmpty(PlaySFX.data.clipName) ? null : PlaySFX.data.clipName;
                comp.vfxName = string.IsNullOrEmpty(PlayVFX.data.clipName) ? null : PlayVFX.data.clipName;
                comp.sfxIndex = PlaySFX.data.clipIndex;
                comp.vfxIndex = PlayVFX.data.clipIndex;
                comp.canUpdateScore = Score.score != 0;
                comp.scoreValue = Score.score;
            }
            gameObject.TrySetTrigger(false, true);
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var data = JsonConvert.SerializeObject(comp);
            return (type, data);
        }

        public string GetStartEvent()
        {
            int index = startOn.data.startIndex;
            var value = (StartOnCollectible)index;
            var eventName = EditorOp.Resolve<DataProvider>().GetEnumValue(value);
            return eventName;
        }

        public string GetStartCondition()
        {
            int index = startOn.data.startIndex;
            var value = (StartOnCollectible)index;
            return EditorOp.Resolve<DataProvider>().GetEnumConditionDataValue(value);
        }

        public void Import(EntityBasedComponent cdata)
        {
            CollectableComponent comp = JsonConvert.DeserializeObject<CollectableComponent>($"{cdata.data}");
            Score.score = comp.scoreValue;
            EditorOp.Resolve<SceneDataHandler>()?.UpdateScoreModifiersCount(true, Score.instanceId);
            PlaySFX.data.canPlay = comp.canPlaySFX;
            PlaySFX.data.clipIndex = comp.sfxIndex;
            PlaySFX.data.clipName = comp.sfxName;
            PlayVFX.data.canPlay = comp.canPlayVFX;
            PlayVFX.data.clipIndex = comp.vfxIndex;
            PlayVFX.data.clipName = comp.vfxName;
            if (EditorOp.Resolve<DataProvider>().TryGetEnum(comp.ConditionType, typeof(StartOnCollectible), out object result))
            {
                startOn.data.startIndex = (int)(StartOnCollectible)result;
            }
            Broadcast = comp.Broadcast;
            startOn.data.startName = comp.ConditionType;
            startOn.data.listenName = comp.ConditionData;
            EditorOp.Resolve<UILogicDisplayProcessor>().ImportVisualisation(gameObject, GetType().Name, Broadcast, null);
        }

        private void OnDestroy()
        {
            EditorOp.Resolve<SceneDataHandler>()?.UpdateScoreModifiersCount(false, Score.instanceId);
        }
    }
}
