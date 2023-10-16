using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace Terra.Studio
{
    public class Broadcaster
    {
        public Action OnToBroadcastRequestReceived;
        private readonly string[] CORE_BROADCAST_KEYS = new[] { "Game Win", "Game Lose" };
        private Dictionary<string, List<Action<object>>> broadcastDict = new();

        public void SetBroadcastable(string broadcastData)
        {
            if (broadcastDict.ContainsKey(broadcastData))
            {
                return;
            }
            broadcastDict.Add(broadcastData, new List<Action<object>>());
        }

        public void ListenTo(string broadcastData, Action<object> onBroadcastListened)
        {
            if (!broadcastDict.ContainsKey(broadcastData))
            {
                SetBroadcastable(broadcastData);
            }
            broadcastDict[broadcastData].Add(onBroadcastListened);
        }

        public void StopListenTo(string broadcastData, Action<object> onBroadcastListened)
        {
            if (broadcastDict.ContainsKey(broadcastData))
            {
                broadcastDict[broadcastData].Remove(onBroadcastListened);
            }
        }

        public void Broadcast(string broadcastData, bool removeOnceBroadcasted = false)
        {
            var canBroadcast = SystemOp.Resolve<System>().CanInitiateSubsystemProcess?.Invoke() ?? true;
            if (!canBroadcast)
            {
                OnToBroadcastRequestReceived?.Invoke();
                return;
            }
            if (CORE_BROADCAST_KEYS.Any(x => x.Equals(broadcastData)))
            {
                PerformCoreAction(broadcastData);
                return;
            }
            var listeners = GetListeners(broadcastData);
            if (listeners == null || listeners.Count() == 0)
            {
                Debug.Log($"No one is listening to {broadcastData}");
                return;
            }
            var filteredListeners = listeners.ToList();
            if (removeOnceBroadcasted)
            {
                RemoveBroadcastable(broadcastData);
            }
            foreach (var listener in filteredListeners)
            {
                listener?.Invoke(null);
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

        private IEnumerable<Action<object>> GetListeners(string broadcastData)
        {
            if (!broadcastDict.ContainsKey(broadcastData))
            {
                return null;
            }
            return broadcastDict[broadcastData];
        }

        private void PerformCoreAction(string data)
        {
            CoroutineService.RunCoroutine(() =>
            {
                RuntimeOp.Resolve<GameData>().SetEndState(data);
                RuntimeOp.Resolve<GameStateHandler>().SwitchToNextState();
            },
            CoroutineService.DelayType.WaitForFrame);
        }
    }
}
