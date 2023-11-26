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
        public FXData FXData { get; set; }
        public GameObject RefObj { get; set; }
        public Listen Listen { get; set; }

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
            targetData.FXData = actualData.FXData;
            targetData.Listen = actualData.Listen;
            targetData.RefObj = go;
            targetData.EventContext = new()
            {
                componentName = typeof(T).FullName,
                conditionType = targetData.ConditionType,
                conditionData = targetData.ConditionData,
                goRef = targetData.RefObj
            };
        }

        public virtual PlayFXData? GetSFXData(int index)
        {
            var sfxArray = FXData.SFXData;
            if (sfxArray == null || sfxArray.Length == 0 || sfxArray.Length < index)
            {
                return null;
            }
            return sfxArray[index];
        }

        public virtual PlayFXData? GetVFXData(int index)
        {
            var vfxArray = FXData.VFXData;
            if (vfxArray == null || vfxArray.Length == 0 || vfxArray.Length < index)
            {
                return null;
            }
            return vfxArray[index];
        }
    }
}
