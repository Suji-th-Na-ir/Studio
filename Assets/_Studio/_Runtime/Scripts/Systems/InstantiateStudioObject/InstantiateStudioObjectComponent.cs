using System;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
using Random = UnityEngine.Random;

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
        public Vector3[] rangeTRS;

        [JsonIgnore] public uint currentRound;

        public readonly IEnumerable<Vector3> GetRandomPoints(int count)
        {
            var halfExtents = rangeTRS[2] / 2f;
            var rotation = Quaternion.Euler(rangeTRS[1]);
            var rotationMatrix = Matrix4x4.Rotate(rotation);
            var xTransform = Vector3.Dot(rotationMatrix.GetColumn(0), halfExtents);
            var yTransform = Vector3.Dot(rotationMatrix.GetColumn(1), halfExtents);
            var zTransform = Vector3.Dot(rotationMatrix.GetColumn(2), halfExtents);
            var transformedHalfExtents = new Vector3(xTransform, yTransform, zTransform);
            var minBounds = rangeTRS[0] - transformedHalfExtents;
            var maxBounds = rangeTRS[0] + transformedHalfExtents;
            var points = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                var randomX = Random.Range(minBounds.x, maxBounds.x);
                var randomY = Random.Range(minBounds.y, maxBounds.y);
                var randomZ = Random.Range(minBounds.z, maxBounds.z);
                points[i] = new Vector3(randomX, randomY, randomZ);
            }
            return points;
        }
    }
}