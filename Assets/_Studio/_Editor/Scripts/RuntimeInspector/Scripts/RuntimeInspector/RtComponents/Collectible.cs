using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using RuntimeInspectorNamespace;
using Terra.Studio;
using Terra.Studio.RTEditor;
using UnityEngine;

namespace RuntimeInspectorNamespace
{
    public enum CollectableEventType
    {
        OnPlayerCollide,
        OnClick
    }

    public class Collectible : MonoBehaviour, IComponent
    {
        public CollectableEventType Start = CollectableEventType.OnPlayerCollide;
        public Atom.PlaySFX PlaySfx = Atom.PlaySFX.Off;
        public Atom.PlayVFX PlayVFX = Atom.PlayVFX.Off;
        public bool ShowScoreUI = false;
        public bool CanUpdateScore = false;
        public float ScoreValue = 0;
        public string Broadcast = "";

        public void ExportData()
        {
            Debug.Log("name " + gameObject.name);
            Debug.Log("start type " + Start.ToString());
            Debug.Log("Broadcast " + Broadcast.ToString());
        }

        public (string type, string data) Export()
        {
            var state = GetComponent<InspectorStateManager>();
            CollectableComponent collectable = new()
            {
                IsConditionAvailable = true,
                ConditionType = GetStartEvent(),
                ConditionData = GetStartCondition(),
                IsBroadcastable = Broadcast != "",
                Broadcast = Broadcast == "" ? null : Broadcast.ToString(),
                canPlaySFX = state.GetItem<bool>("sfx_toggle"),
                canPlayVFX = state.GetItem<bool>("vfx_toggle"),
                sfxName = !state.GetItem<bool>("sfx_toggle") ? null : PlaySFXField.GetSfxClipName(state.GetItem<int>("sfx_dropdown")),
                vfxName = !state.GetItem<bool>("vfx_toggle") ? null : PlayVFXField.GetVfxClipName(state.GetItem<int>("vfx_dropdown")),
                canUpdateScore = CanUpdateScore,
                scoreValue = ScoreValue,
                showScoreUI = ShowScoreUI
            };
            var type = "Terra.Studio.Collectable";
            var data = JsonConvert.SerializeObject(collectable);
            return (type, data);
        }

        public string GetStartEvent()
        {
            if (Start == CollectableEventType.OnPlayerCollide)
                return "Terra.Studio.TriggerAction";

            if (Start == CollectableEventType.OnClick)
                return "Terra.Studio.MouseAction";

            return "";
        }

        public string GetStartCondition()
        {
            if (Start == CollectableEventType.OnPlayerCollide)
                return "Player";

            if (Start == CollectableEventType.OnClick)
                return "OnClick";

            return "";
        }
    }
}
