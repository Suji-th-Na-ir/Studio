using System;
using System.Collections;
using System.Collections.Generic;
using RuntimeInspectorNamespace;
using Terra.Studio.RTEditor;
using UnityEngine;

namespace Terra.Studio.RTEditor
{
    public enum CollectableEventType
    {
        None,
        OnPlayerCollide,
        OnClick
    }

    [Serializable]
    public class SFX
    {
        public string name;
    }
    
    public class Collectible : MonoBehaviour, IComponent
    {
        public CollectableEventType Start = CollectableEventType.None;
        public Atom.PlaySFX PlaySfx = Atom.PlaySFX.Off;
        public Atom.PlayVFX PlayVFX = Atom.PlayVFX.Off;
        public Atom.BroadCast Broadcast = Atom.BroadCast.None;

        public void ExportData()
        {
            Debug.Log("name "+gameObject.name);
            Debug.Log("start type "+Start.ToString());
            Debug.Log("Broadcast "+Broadcast.ToString());
        }
    }
}
