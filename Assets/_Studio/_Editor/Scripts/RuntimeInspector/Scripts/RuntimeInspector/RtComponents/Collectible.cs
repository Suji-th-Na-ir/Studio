using Newtonsoft.Json;
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

    [EditorDrawComponent("Terra.Studio.Collectable")]
    public class Collectible : MonoBehaviour, IComponent
    {
        public CollectableEventType Start = CollectableEventType.OnPlayerCollide;
        public Atom.PlaySfx PlaySFX = new Atom.PlaySfx();
        public Atom.PlayVfx PlayVFX = new Atom.PlayVfx();
        public bool ShowScoreUI = false;
        public bool CanUpdateScore = false;
        public float ScoreValue = 0;
        public string Broadcast = "";
        
        public (string type, string data) Export()
        {
            CollectableComponent collectable = new();
            {
                collectable.IsConditionAvailable = true;
                collectable.ConditionType = GetStartEvent();
                collectable.ConditionData = GetStartCondition();
                collectable.IsBroadcastable = Broadcast != "";
                collectable.Broadcast = string.IsNullOrEmpty(Broadcast) ? null : Broadcast;

                collectable.canPlaySFX = PlaySFX.canPlay;
                collectable.canPlayVFX = PlayVFX.canPlay;

                collectable.sfxName = PlaySFX.clipName;
                collectable.vfxName = PlayVFX.clipName;

                collectable.sfxIndex = PlaySFX.clipIndex;
                collectable.vfxIndex = PlayVFX.clipIndex;

                collectable.canUpdateScore = CanUpdateScore;
                collectable.scoreValue = ScoreValue;
                collectable.showScoreUI = ShowScoreUI;
            }
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var data = JsonConvert.SerializeObject(collectable);
            return (type, data);
        }

        public void Import(EntityBasedComponent cdata)
        {
            CollectableComponent cc = JsonConvert.DeserializeObject<CollectableComponent>($"{cdata.data}");
            CanUpdateScore = cc.canUpdateScore;
            ShowScoreUI = cc.showScoreUI;
            ScoreValue = cc.scoreValue;

            if (cc.ConditionType == "Terra.Studio.TriggerAction")
                Start = CollectableEventType.OnPlayerCollide;
            else if (cc.ConditionType == "Terra.Studio.MouseAction")
                Start = CollectableEventType.OnClick;

            Broadcast = cc.Broadcast;

            PlaySFX.canPlay = cc.canPlaySFX;
            PlaySFX.clipIndex = cc.sfxIndex;
            PlaySFX.clipName = cc.sfxName;
            PlayVFX.canPlay = cc.canPlayVFX;
            PlayVFX.clipIndex = cc.vfxIndex;
            PlayVFX.clipName = cc.vfxName;
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
