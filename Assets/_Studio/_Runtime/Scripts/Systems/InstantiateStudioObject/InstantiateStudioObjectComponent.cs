using System;
using UnityEngine;
using Newtonsoft.Json;

namespace Terra.Studio
{
    [Serializable]
    public struct InstantiateStudioObjectComponent : IBaseComponent
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

        public bool canPlaySFX;
        public string sfxName;
        public int sfxIndex;
        public bool canPlayVFX;
        public string vfxName;
        public int vfxIndex;
        public float interval;
        public uint rounds;
        public bool canRepeatForver;
        public uint duplicatesToSpawn;
        public InstantiateOn instantiateOn;
        public SpawnWhere spawnWhere;
        public EntityBasedComponent[] componentsOnSelf;
        public VirtualEntity[] childrenEntities;

        [JsonIgnore] public uint currentRound;
    }
}