using System;
using UnityEngine;
using Newtonsoft.Json;
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
        public FXData FXData { get; set; }
        public Listen Listen { get; set; }
        [JsonIgnore] public bool CanExecute { get; set; }
        [JsonIgnore] public bool IsExecuted { get; set; }
        [JsonIgnore] public EventContext EventContext { get; set; }
        [JsonIgnore] public GameObject RefObj { get; set; }

        public float interval;
        public uint rounds;
        public bool canRepeatForver;
        public uint duplicatesToSpawn;
        public InstantiateOn instantiateOn;
        public SpawnWhere spawnWhere;
        public EntityBasedComponent[] componentsOnSelf;
        public VirtualEntity[] childrenEntities;
        public TRS trs;
        [JsonIgnore] public uint currentRound;

        public readonly Vector3[] GetRandomPoints(uint count)
        {
            var halfExtents = trs.Scale / 2f;
            var rotation = Quaternion.Euler(trs.Rotation);
            var rotationMatrix = Matrix4x4.Rotate(rotation);
            var xTransform = Vector3.Dot(rotationMatrix.GetColumn(0), halfExtents);
            var yTransform = Vector3.Dot(rotationMatrix.GetColumn(1), halfExtents);
            var zTransform = Vector3.Dot(rotationMatrix.GetColumn(2), halfExtents);
            var transformedHalfExtents = new Vector3(xTransform, yTransform, zTransform);
            var minBounds = trs.Position - transformedHalfExtents;
            var maxBounds = trs.Position + transformedHalfExtents;
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
        [Serializable]
        public struct TRS
        {
            [JsonConverter(typeof(Vector3Converter))] public Vector3 Position;
            [JsonConverter(typeof(Vector3Converter))] public Vector3 Rotation;
            [JsonConverter(typeof(Vector3Converter))] public Vector3 Scale;

            public static TRS GetTRS(Vector3[] trs)
            {
                var trsData = new TRS()
                {
                    Position = trs[0],
                    Rotation = trs[1],
                    Scale = trs[2],
                };
                return trsData;
            }

            public static Vector3[] GetVector3Array(TRS trs)
            {
                var vector3s = new Vector3[]
                {
                    trs.Position,
                    trs.Rotation,
                    trs.Scale
                };
                return vector3s;
            }
        }
    }
}