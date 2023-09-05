using System;
using UnityEngine;
using Newtonsoft.Json;

namespace Terra.Studio
{
    [Serializable]
    public struct SwitchComponent : IBaseComponent
    {
        public bool IsConditionAvailable { get; set; }
        public string ConditionType { get; set; }
        public string ConditionData { get; set; }
        public bool IsBroadcastable { get; set; }
        public string Broadcast { get; set; }
        public bool IsTargeted { get; set; }
        public int TargetId { get; set; }
        [JsonIgnore] public bool CanExecute { get; set; }
        [JsonIgnore] public bool IsExecuted { get; set; }
        [JsonIgnore] public EventContext EventContext { get; set; }
        [JsonIgnore] public GameObject RefObj { get; set; }
        public SwitchState currentState;
        public SwitchComponentData onStateData;
        public SwitchComponentData offStateData;
        public Listen listen;

        public void UpdateState()
        {
            if (currentState == SwitchState.On)
            {
                currentState = SwitchState.Off;
            }
            else
            {
                currentState = SwitchState.On;
            }
        }

        public readonly SwitchComponentData GetData()
        {
            if (currentState == SwitchState.On)
            {
                return onStateData;
            }
            else
            {
                return offStateData;
            }
        }
    }

    public struct SwitchComponentData
    {
        public SwitchState state;
        public bool canPlaySFX;
        public string sfxName;
        public int sfxIndex;
        public bool canPlayVFX;
        public string vfxName;
        public int vfxIndex;
        public bool isBroadcastable;
        public string broadcast;
        public int broadcastIndex;
    }
}