using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace Terra.Studio
{
    public class Broadcaster
    {
        public Action OnToBroadcastRequestReceived;
        public readonly string[] CORE_BROADCAST_KEYS = new[] { "Game Win", "Game Lose" };
        private Dictionary<string, List<Action<object>>> broadcastDict = new();

        public void ListenTo(string broadcastData, Action<object> onBroadcastListened)
        {
            if (!broadcastDict.ContainsKey(broadcastData))
            {
                broadcastDict.Add(broadcastData, new List<Action<object>>());
            }
            broadcastDict[broadcastData].Add(onBroadcastListened);
        }

        public void StopListenTo(string broadcastData, Action<object> onBroadcastListened)
        {
            if (broadcastDict.ContainsKey(broadcastData))
            {
                broadcastDict[broadcastData].Remove(onBroadcastListened);
                if (broadcastDict[broadcastData].Count == 0)
                {
                    broadcastDict.Remove(broadcastData);
                }
            }
        }

        public void Broadcast(string broadcastData)
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
            foreach (var listener in filteredListeners)
            {
                listener?.Invoke(null);
            }
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
