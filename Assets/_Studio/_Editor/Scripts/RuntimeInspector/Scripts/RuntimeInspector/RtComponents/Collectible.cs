using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using PlayShifu.Terra;
using RuntimeInspectorNamespace;
using Terra.Studio;
using Terra.Studio.RTEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RuntimeInspectorNamespace
{
    public enum CollectableEventType
    {
        OnPlayerCollide,
        OnClick
    }

    [EditorDrawComponent("Terra.Studio.Collectable")]
    public class Collectible : MonoBehaviour, IComponent
    {
        public CollectableEventType Start = CollectableEventType.OnPlayerCollide;
        public Atom.PlaySFX PlaySfx = Atom.PlaySFX.Off;
        public Atom.PlayVFX PlayVFX = Atom.PlayVFX.Off;
        public bool ShowScoreUI = false;
        public bool CanUpdateScore = false;
        public float ScoreValue = 0;
        public string Broadcast = "";
        
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
                sfxName = !state.GetItem<bool>("sfx_toggle") ? null : Helper.GetSfxClipNameByIndex(state.GetItem<int>("sfx_dropdown")),
                vfxName = !state.GetItem<bool>("vfx_toggle") ? null : Helper.GetVfxClipNameByIndex(state.GetItem<int>("vfx_dropdown")),
                sfxIndex = state.GetItem<int>("sfx_dropdown"),
                vfxIndex = state.GetItem<int>("vfx_dropdown"),
                canUpdateScore = CanUpdateScore,
                scoreValue = ScoreValue,
                showScoreUI = ShowScoreUI
            };
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var data = JsonConvert.SerializeObject(collectable);
            return (type, data);
        }

        public void Import(EntityBasedComponent cdata)
        {
            var state = GetComponent<InspectorStateManager>();

            CollectableComponent cc = JsonConvert.DeserializeObject<CollectableComponent>($"{cdata.data}");
            CanUpdateScore = cc.canUpdateScore;
            ShowScoreUI = cc.showScoreUI;
            ScoreValue = cc.scoreValue;

            if (cc.ConditionType == "Terra.Studio.TriggerAction")
                Start = CollectableEventType.OnPlayerCollide;
            else if (cc.ConditionType == "Terra.Studio.MouseAction")
                Start = CollectableEventType.OnClick;

            Broadcast = cc.Broadcast;
            
            state.SetItem("sfx_toggle", cc.canPlaySFX);
            state.SetItem("vfx_toggle", cc.canPlayVFX);
            state.SetItem("sfx_dropdown", cc.sfxIndex);
            state.SetItem("vfx_dropdown", cc.vfxIndex);
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
