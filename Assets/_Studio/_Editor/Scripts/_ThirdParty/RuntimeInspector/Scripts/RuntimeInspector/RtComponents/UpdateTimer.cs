using Newtonsoft.Json;

namespace Terra.Studio
{
    [EditorDrawComponent("Terra.Studio.UpdateTimer")]
    public class UpdateTimer : BaseBehaviour
    {
        public Atom.StartOn updateWhen = new();
        public int updateBy = 0;

        public override string ComponentName => nameof(UpdateTimer);
        public override bool CanPreview => false;
        protected override bool CanBroadcast => false;
        protected override bool CanListen => true;
        protected override string[] ListenerRefs => new string[]
        {
            updateWhen.data.listenName
        };

        private void Start()
        {
            updateWhen.Setup<StartOn>(gameObject, ComponentName);
        }

        public override (string type, string data) Export()
        {
            var comp = new UpdateTimerComponent()
            {
                IsConditionAvailable = true,
                ConditionType = GetStartEvent(),
                ConditionData = GetStartCondition(),
                IsBroadcastable = false,
                canPlaySFX = false,
                canPlayVFX = false,
                updateBy = updateBy
            };
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var data = JsonConvert.SerializeObject(comp);
            return (type, data);
        }

        public override void Import(EntityBasedComponent data)
        {
            var comp = JsonConvert.DeserializeObject<UpdateTimerComponent>(data.data);
            updateBy = comp.updateBy;

        }

        private string GetStartEvent()
        {
            int index = updateWhen.data.startIndex;
            var value = (StartOn)index;
            var eventName = EditorOp.Resolve<DataProvider>().GetEnumValue(value);
            return eventName;
        }

        private string GetStartCondition()
        {
            int index = updateWhen.data.startIndex;
            var value = (StartOn)index;
            string inputString = value.ToString();
            if (inputString.ToLower().Contains("listen"))
            {
                return updateWhen.data.listenName;
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
                updateWhen.data.startIndex = (int)targetEnum;
                updateWhen.data.startName = targetEnum.ToString();
                return;
            }
            var listenType = EditorOp.Resolve<DataProvider>().GetEnumValue(StartOn.BroadcastListen);
            if (listenType.Equals(component.ConditionType))
            {
                updateWhen.data.startIndex = (int)StartOn.BroadcastListen;
                updateWhen.data.startName = StartOn.BroadcastListen.ToString();
                updateWhen.data.listenName = component.ConditionData;
                return;
            }
            var startOn = (StartOn)result;
            updateWhen.data.startIndex = (int)startOn;
            updateWhen.data.startName = startOn.ToString();
        }
    }
}
