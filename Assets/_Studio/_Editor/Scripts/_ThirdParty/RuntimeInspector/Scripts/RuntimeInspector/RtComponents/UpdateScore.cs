using System;
using Newtonsoft.Json;

namespace Terra.Studio
{
    [EditorDrawComponent("Terra.Studio.UpdateScore"), AliasDrawer("Update Score")]
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
        [AliasDrawer("Update By")]
        public Atom.ScoreData Score = new();
        public Atom.Broadcast broadcastData = new();

        public override string ComponentName => nameof(UpdateScore);
        public override bool CanPreview => false;
        protected override bool CanBroadcast => true;
        protected override bool CanListen => true;
        protected override string[] BroadcasterRefs => new string[]
        {
            broadcastData.broadcast
        };

        protected override void Awake()
        {
            base.Awake();
            Score.Setup(gameObject,this);
            startOn.Setup<StartOn>(gameObject, ComponentName, OnListenerUpdated,startOn.data.startIndex==3);
            broadcastData.Setup(gameObject, this);
        }

        public override (string type, string data) Export()
        {
            var data = new UpdateScoreComponent
            {
                IsConditionAvailable = true,
                ConditionType = EditorOp.Resolve<DataProvider>().GetEnumValue(GetEnum(startOn.data.startName)),
                ConditionData = GetStartCondition(),
                AddScoreValue = Score.score,
                IsBroadcastable = !string.IsNullOrEmpty(broadcastData.broadcast),
                Broadcast = broadcastData.broadcast,
                listen = Listen.Always
            };
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var json = JsonConvert.SerializeObject(data);
            return (type, json);
        }

        public override void Import(EntityBasedComponent data)
        {
            var comp = JsonConvert.DeserializeObject<UpdateScoreComponent>(data.data);
            startOn.data.startName = GetStart(comp).ToString();
            startOn.data.listenName = GetListenValues(comp);
            broadcastData.broadcast = comp.Broadcast;
            var listenString = "";
            if (EditorOp.Resolve<DataProvider>().TryGetEnum(comp.ConditionType, typeof(StartOn), out object result))
            {
                var res = (StartOn)result;
                if (res == StartOn.OnPlayerCollide)
                {
                    if (comp.ConditionData.Equals("Player"))
                    {
                        startOn.data.startIndex = (int)res;
                    }
                    else
                    {
                        startOn.data.startIndex = (int)StartOn.OnObjectCollide;
                    }
                }
                else
                {
                    startOn.data.startIndex = (int)(StartOn)result;
                }
            }
            if (startOn.data.startIndex == 3)
                listenString = startOn.data.listenName;
            Score.score = comp.AddScoreValue;
            if (Score.score != 0)
            {
                EditorOp.Resolve<SceneDataHandler>()?.UpdateScoreModifiersCount(true, Score.instanceId, false);
            }
            ImportVisualisation(broadcastData.broadcast, listenString);
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

        public string GetStartCondition()
        {
            int index = startOn.data.startIndex;
            var value = (StartOn)index;
            string inputString = value.ToString();
            if (inputString.ToLower().Contains("listen"))
            {
                return startOn.data.listenName;
            }
            var data = EditorOp.Resolve<DataProvider>().GetEnumConditionDataValue(value);
            return data;
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
