using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using static Terra.Studio.Atom;

namespace Terra.Studio
{
    [EditorDrawComponent("Terra.Studio.UpdateScore")]
    public class UpdateScore : BaseBehaviour
    {
        public enum StartOn
        {
            [EditorEnumField("Terra.Studio.TriggerAction", "Player"), AliasDrawer("Player Touches")]
            OnPlayerCollide,
            [EditorEnumField("Terra.Studio.TriggerAction", "Other"), AliasDrawer("Other Object Touches")]
            OnObjectCollide,
            [EditorEnumField("Terra.Studio.MouseAction", "OnClick"), AliasDrawer("Clicked")]
            OnClick,
            [EditorEnumField("Terra.Studio.Listener"), AliasDrawer("Broadcast Listened")]
            BroadcastListen

        }
        [AliasDrawer("Update When")]
        public Atom.StartOn startOn = new();
        public Atom.ScoreData Score = new();

        public override string ComponentName => nameof(UpdateScore);

        public override bool CanPreview => false;
        protected override bool CanBroadcast => false;
        protected override bool CanListen => true;

        protected override void Awake()
        {
            base.Awake();
            Score.Setup(gameObject,this);
            startOn.Setup<StartOn>(gameObject, ComponentName, OnListenerUpdated, startOn.data.startIndex == 1);
        }

        public override (string type, string data) Export()
        {
            var data = new UpdateScoreComponent
            {
                IsConditionAvailable = true,
                ConditionType = EditorOp.Resolve<DataProvider>().GetEnumValue(GetEnum(startOn.data.startName)),
                IsBroadcastable = false,
                Broadcast = string.Empty,
                ConditionData = GetConditionValue(),
                startIndex = startOn.data.startIndex,
                AddScoreValue = Score.score
            };
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var json = JsonConvert.SerializeObject(data);
            return (type, json);
        }

        public override void Import(EntityBasedComponent data)
        {
            var obj = JsonConvert.DeserializeObject<UpdateScoreComponent>(data.data);
            startOn.data.startName = GetStart(obj).ToString();
            startOn.data.listenName = GetListenValues(obj);
            startOn.data.startIndex = obj.startIndex;
            var listenString = "";
            if (startOn.data.startIndex == 1)
                listenString = startOn.data.listenName;
            Score.score = obj.AddScoreValue;
            if (Score.score != 0)
            {
                EditorOp.Resolve<SceneDataHandler>()?.UpdateScoreModifiersCount(true, Score.instanceId, false);
            }
            ImportVisualisation(string.Empty, listenString);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (Score.score != 0)
            {
                EditorOp.Resolve<SceneDataHandler>()?.UpdateScoreModifiersCount(true, Score.instanceId);
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            EditorOp.Resolve<SceneDataHandler>()?.UpdateScoreModifiersCount(false, Score.instanceId);
        }

        private StartOn GetEnum(string data)
        {
            if (Enum.TryParse(data, out StartOn startOn))
            {
                return startOn;
            }
            return StartOn.OnClick;
        }

        private string GetConditionValue()
        {
            if (string.IsNullOrEmpty(startOn.data.startName) ||
                startOn.data.startName.Equals(StartOn.OnClick))
            {
                return EditorOp.Resolve<DataProvider>().GetEnumConditionDataValue(StartOn.OnClick);
            }
            else if (startOn.data.startName.Equals(StartOn.OnObjectCollide.ToString()))
            {
                return EditorOp.Resolve<DataProvider>().GetEnumConditionDataValue(StartOn.OnObjectCollide);
            }
            else
            {
                return startOn.data.listenName;
            }
        }

        private StartOn GetStart(UpdateScoreComponent comp)
        {
            if (EditorOp.Resolve<DataProvider>().TryGetEnum(comp.ConditionType, typeof(StartOn), out object result))
            {
                return (StartOn)result;
            }
            return StartOn.OnClick;
        }

        private string GetListenValues(UpdateScoreComponent comp)
        {
            if (comp.ConditionType.Equals(EditorOp.Resolve<DataProvider>().GetEnumValue(StartOn.BroadcastListen)))
            {
                return comp.ConditionData;
            }
            return null;
        }
    }
}
