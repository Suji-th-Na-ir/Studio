using UnityEngine;
using PlayShifu.Terra;

namespace Terra.Studio
{
    public interface IBaseComponent
    {
        public bool CanExecute { get; set; }
        public bool IsExecuted { get; set; }
        public bool IsConditionAvailable { get; set; }
        public string ConditionType { get; set; }
        public string ConditionData { get; set; }
        public bool IsBroadcastable { get; set; }
        public string Broadcast { get; set; }
        public bool IsTargeted { get; set; }
        public int TargetId { get; set; }
        public EventContext EventContext { get; set; }
        public GameObject RefObj { get; set; }

        public virtual void Clone<T>(T actualData, ref T targetData, GameObject go) where T : struct, IBaseComponent
        {
            Helper.CopyStructFieldValues(actualData, ref targetData);
            targetData.IsConditionAvailable = actualData.IsConditionAvailable;
            targetData.ConditionType = actualData.ConditionType;
            targetData.ConditionData = actualData.ConditionData;
            targetData.IsBroadcastable = actualData.IsBroadcastable;
            targetData.Broadcast = actualData.Broadcast;
            targetData.IsTargeted = actualData.IsTargeted;
            targetData.TargetId = actualData.TargetId;
            targetData.EventContext = actualData.EventContext;
            targetData.RefObj = go;
            var conditionalCheckData = new EventConditionalCheckData()
            {
                conditionType = targetData.ConditionType,
                conditionData = targetData.ConditionData,
                goRef = targetData.RefObj
            };
            targetData.EventContext = new()
            {
                data = conditionalCheckData
            };
        }
    }
}
