using System;
using System.Collections;
using System.Collections.Generic;
using RuntimeInspectorNamespace;
using Terra.Studio.RTEditor;
using UnityEngine;

namespace RuntimeInspectorNamespace
{
    public enum CollectableEventType
    {
        None,
        OnPlayerCollide,
        OnClick
    }

    public class Collectible : MonoBehaviour, IComponent
    {
        public CollectableEventType Start = CollectableEventType.None;
        public Atom.PlaySFX PlaySfx = Atom.PlaySFX.Off;
        public Atom.PlayVFX PlayVFX = Atom.PlayVFX.Off;
        public bool ShowScoreUI = false;
        public Atom.BroadCast Broadcast = Atom.BroadCast.None;

        public void ExportData()
        {
            Debug.Log("name "+gameObject.name);
            Debug.Log("start type "+Start.ToString());
            Debug.Log("Broadcast "+Broadcast.ToString());
        }
    }
}
