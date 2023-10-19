using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Terra.Studio
{
    public class BroadcastListenStringValidator : IDisposable
    {
        
        private List<string> broadcastStrings = new List<string>() {"None", "Game Win", "Game Lose" };
        public List<string> BroadcastStrings { get { return broadcastStrings; } }
        public void Dispose()
        {
            broadcastStrings.Clear();
        }

        public void UpdateNewBroadcast(string newValue)
        {
            if(!broadcastStrings.Contains(newValue))
                broadcastStrings.Add(newValue); 
        }
    }
}
