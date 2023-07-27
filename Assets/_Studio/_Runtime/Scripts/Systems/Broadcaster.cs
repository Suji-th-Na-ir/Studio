using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace Terra.Studio
{
    public class Broadcaster
    {
        private Dictionary<string, List<Action>> broadcastDict = new();

        public void SetBroadcastable(string broadcastData)
        {
            if (broadcastDict.ContainsKey(broadcastData))
            {
                return;
            }
            broadcastDict.Add(broadcastData, new List<Action>());
        }

        public void ListenTo(string broadcastData, Action onBroadcastListened)
        {
            if (!broadcastDict.ContainsKey(broadcastData))
            {
                SetBroadcastable(broadcastData);
            }
            broadcastDict[broadcastData].Add(onBroadcastListened);
        }

        public void StopListenTo(string broadcastData, Action onBroadcastListened)
        {
            if (broadcastDict.ContainsKey(broadcastData))
            {
                broadcastDict[broadcastData].Remove(onBroadcastListened);
            }
        }

        public void Broadcast(string broadcastData, bool removeOnceBroadcasted = false)
        {
            var listeners = GetListeners(broadcastData);
            if (listeners == null || listeners.Count() == 0)
            {
                Debug.Log($"No one is listening to {broadcastData}");
                return;
            }
            foreach (var listener in listeners)
            {
                listener?.Invoke();
            }
            if (removeOnceBroadcasted)
            {
                RemoveBroadcastable(broadcastData);
            }
        }

        private void RemoveBroadcastable(string broadcastData)
        {
            if (!broadcastDict.ContainsKey(broadcastData))
            {
                return;
            }
            broadcastDict.Remove(broadcastData);
        }

        private IEnumerable<Action> GetListeners(string broadcastData)
        {
            if (!broadcastDict.ContainsKey(broadcastData))
            {
                return null;
            }
            return broadcastDict[broadcastData];
        }
    }
}
