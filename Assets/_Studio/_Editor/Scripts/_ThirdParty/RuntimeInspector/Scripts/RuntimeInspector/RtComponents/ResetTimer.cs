using Newtonsoft.Json;

namespace Terra.Studio
{
    [EditorDrawComponent("Terra.Studio.ResetTimer"), AliasDrawer("Reset Timer")]
    public class ResetTimer : BaseBehaviour
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

        public Atom.StartOn resetWhen = new();
        public Atom.Broadcast broadcast = new();

        public override string ComponentName => nameof(ResetTimer);
        public override bool CanPreview => false;
        protected override bool CanBroadcast => false;
        protected override bool CanListen => true;

        protected override void Awake()
        {
            base.Awake();
            resetWhen.Setup<StartOn>(gameObject, ComponentName, OnListenerUpdated, resetWhen.data.startIndex == 3);
            broadcast.Setup(gameObject, this);
        }

        public override (string type, string data) Export()
        {
            var component = new ResetTimerComponent()
            {
                IsConditionAvailable = true,
                ConditionType = GetStartEvent(),
                ConditionData = GetStartCondition(),
                IsBroadcastable = !string.IsNullOrEmpty(broadcast.broadcast),
                Broadcast = broadcast.broadcast,
                Listen = Listen.Always
            };
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var data = JsonConvert.SerializeObject(component);
            return (type, data);
        }

        public override void Import(EntityBasedComponent data)
        {
            var comp = JsonConvert.DeserializeObject<UpdateTimerComponent>(data.data);
            broadcast.broadcast = comp.Broadcast;
            ApplyStartCondition(comp);
            ImportVisualisation(broadcast.broadcast, resetWhen.data.listenName);
        }

        private string GetStartEvent()
        {
            int index = resetWhen.data.startIndex;
            var value = (StartOn)index;
            var eventName = EditorOp.Resolve<DataProvider>().GetEnumValue(value);
            return eventName;
        }

        private string GetStartCondition()
        {
            int index = resetWhen.data.startIndex;
            var value = (StartOn)index;
            string inputString = value.ToString();
            if (inputString.ToLower().Contains("listen"))
            {
                return resetWhen.data.listenName;
            }
            var data = EditorOp.Resolve<DataProvider>().GetEnumConditionDataValue(value);
            return data;
        }

        private void ApplyStartCondition(UpdateTimerComponent component)
        {
            var isStartOnType = EditorOp.Resolve<DataProvider>().TryGetEnum(component.ConditionType, typeof(StartOn), out var result);
            if (!isStartOnType) return;
            var colliderType = EditorOp.Resolve<DataProvider>().GetEnumValue(StartOn.OnPlayerCollide);
            if (colliderType.Equals(component.ConditionType))
            {
                var playerCollideValue = EditorOp.Resolve<DataProvider>().GetEnumConditionDataValue(StartOn.OnPlayerCollide);
                var targetEnum = playerCollideValue.Equals(component.ConditionData) ?
                    StartOn.OnPlayerCollide :
                    StartOn.OnObjectCollide;
                resetWhen.data.startIndex = (int)targetEnum;
                resetWhen.data.startName = targetEnum.ToString();
                return;
            }
            var listenType = EditorOp.Resolve<DataProvider>().GetEnumValue(StartOn.BroadcastListen);
            if (listenType.Equals(component.ConditionType))
            {
                resetWhen.data.startIndex = (int)StartOn.BroadcastListen;
                resetWhen.data.startName = StartOn.BroadcastListen.ToString();
                resetWhen.data.listenName = component.ConditionData;
                return;
            }
            var startOn = (StartOn)result;
            resetWhen.data.startIndex = (int)startOn;
            resetWhen.data.startName = startOn.ToString();
        }

        private void Start() => EditorOp.Resolve<SceneDataHandler>().SetupTimerManager();
    }
}
