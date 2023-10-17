using UnityEngine;
using System.Collections.Generic;

namespace Terra.Studio
{
    public class EntitiesGraphics
    {
        private Dictionary<int, GameObject> trackEntitiesGraphics;

        public void TrackEntityVisual(int entity, GameObject visual)
        {
            var shouldTrack = !SystemOp.Resolve<System>().CanInitiateSubsystemProcess?.Invoke() ?? false;
            if (!shouldTrack) return;
            trackEntitiesGraphics ??= new();
            trackEntitiesGraphics.Add(entity, visual);
        }

        public void FlushAllTrackedVisuals()
        {
            if (trackEntitiesGraphics == null) return;
            foreach (var entityGraphics in trackEntitiesGraphics)
            {
                var go = entityGraphics.Value;
                if (go)
                {
                    Object.Destroy(entityGraphics.Value);
                }
            }
            trackEntitiesGraphics = null;
        }
    }
}
