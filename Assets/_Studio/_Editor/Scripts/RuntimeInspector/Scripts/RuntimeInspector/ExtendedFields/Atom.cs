using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Terra.Studio.RTEditor
{
    public static class Atom
    {
        public enum PlaySFX
        {
            On,
            Off
        }
        
        public enum  PlayVFX
        {
            On,
            Off
        }

        public enum BroadCast
        {
            None,
            CoinCollected,
            CollidedWithPlayer,
            OnClick
        }

        public enum BroadCastListen
        {
            None,
            CoinCollected,
            CollidedWithPlayer,
            OnClick
        }
    }
}
