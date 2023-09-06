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

        public Atom.StartOn startOn = new();
        public Atom.PlaySfx PlaySFX = new();
        public Atom.PlayVfx PlayVFX = new();
        public Atom.ScoreData Score = new();
        public string Broadcast = null;

        public void Awake()
        {
            startOn.Setup(gameObject, Helper.GetEnumValuesAsStrings<StartOnCollectible>(), GetType().Name);
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
                return string.IsNullOrEmpty(startOn.data.listenName) ? null : startOn.data.listenName;
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
            CollectableComponent comp = JsonConvert.DeserializeObject<CollectableComponent>($"{cdata.data}");
            Score.score = comp.scoreValue;
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
    }
}
