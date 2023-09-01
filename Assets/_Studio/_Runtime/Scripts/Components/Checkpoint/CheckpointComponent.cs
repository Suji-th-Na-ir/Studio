using System;
using UnityEngine;
using Newtonsoft.Json;
using PlayShifu.Terra;

namespace Terra.Studio
{
    [Serializable]
    public struct CheckpointComponent : IBaseComponent
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
        [JsonIgnore] public EventExecutorData EventExecutorData { get; set; }

        [JsonIgnore] public GameObject refObj;
        [JsonConverter(typeof(Vector3Converter))] public Vector3 respawnPoint;

        public bool canPlaySFX;
        public bool canPlayVFX;
        public string sfxName;
        public string vfxName;
        public int sfxIndex;
        public int vfxIndex;

        public void Setup()
        {
            EventExecutorData = new()
            {
                data = ConditionData,
                goRef = refObj
            };
        }

        public void CloneFrom<T>(T data) where T : struct, IBaseComponent
        {
            Helper.CopyStructFieldValues<CheckpointComponent>(this, data);
            Setup();
        }
    }
}