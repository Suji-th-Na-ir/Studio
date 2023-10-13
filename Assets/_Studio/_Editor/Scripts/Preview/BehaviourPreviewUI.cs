using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;
using PlayShifu.Terra;
using System.Collections.Generic;

namespace Terra.Studio
{
    public class BehaviourPreviewUI : View
    {
        private const string LEFT_GROUP_LOC = "LeftGroup";
        private const string RIGHT_GROUP_LOC = "RightGroup";
        private const string HEADING_GROUP_LOC = "HeadingGroup";
        private const string CLOSE_BUTTON_LOC = "Close";
        private const string RESTART_BUTTON_LOC = "Restart";
        private const string EVENT_GROUP_LOC = "EventActionGroup";
        private const string PROPERTIES_GROUP_LOC = "PropertiesGroup";
        private const string BROADCAST_GROUP_LOC = "BroadcastGroup";
        private const string HEADING_TEXT_LOC = "Heading";
        private const string ICON_IMAGE_LOC = "Icon";

        private CanvasGroup eventActionCanvasGroup;
        private CanvasGroup propertiesActionCanvasGroup;
        private CanvasGroup broadcastActionGroup;
        private PreviewData data;

        public PreviewData CurrentPreviewData { get { return data; } }

        private void Awake()
        {
            EditorOp.Register(this);
        }

        private void OnDestroy()
        {
            EditorOp.Unregister(this);
        }

        public void ToggleToEventActionGroup()
        {
            eventActionCanvasGroup.alpha = 1f;
            propertiesActionCanvasGroup.alpha = 0.5f;
            broadcastActionGroup.alpha = 0.5f;
        }

        public void ToggleToPropertiesGroup()
        {
            eventActionCanvasGroup.alpha = 0.5f;
            propertiesActionCanvasGroup.alpha = 1f;
            broadcastActionGroup.alpha = 0.5f;
        }

        public void ToggleToBroadcastGroup()
        {
            if (!broadcastActionGroup.gameObject.activeSelf)
            {
                return;
            }
            eventActionCanvasGroup.alpha = 0.5f;
            propertiesActionCanvasGroup.alpha = 0.5f;
            broadcastActionGroup.alpha = 1f;
        }

        public void ToggleToNextPropertyState()
        {
            if (data.Properties.Length <= 0)
            {
                Debug.Log($"Toggling state not possible for: {data.DisplayName}");
                return;
            }
            data.IterateState();
            PreparePropertiesGroup();
            ToggleToEventActionGroup();
        }

        public void Init<T>(T instance) where T : BaseBehaviour
        {
            data = instance.GetPreviewData();
            data.Init();
            PrepareHeadingGroup(instance);
            PrepareEventGroup();
            PreparePropertiesGroup();
        }

        private void PrepareHeadingGroup<T>(T instance) where T : BaseBehaviour
        {
            var headingGroup = Helper.FindDeepChild(transform, HEADING_GROUP_LOC);
            var leftGroup = Helper.FindDeepChild(headingGroup, LEFT_GROUP_LOC);
            var headingText = Helper.FindDeepChild<TextMeshProUGUI>(leftGroup, HEADING_TEXT_LOC);
            var rightGroup = Helper.FindDeepChild(transform, RIGHT_GROUP_LOC);
            var restartBtn = Helper.FindDeepChild<Button>(rightGroup, RESTART_BUTTON_LOC);
            var closeBtn = Helper.FindDeepChild<Button>(rightGroup, CLOSE_BUTTON_LOC);
            headingText.text = data.DisplayName;
            AddListenerToButton(closeBtn, () => { EditorOp.Resolve<BehaviourPreview>().Preview(instance); });
            AddListenerToButton(restartBtn, () =>
            {
                data.Init();
                EditorOp.Resolve<BehaviourPreview>().Restart(instance);
            });
        }

        private void PrepareEventGroup()
        {
            var eventGroup = Helper.FindDeepChild(transform, EVENT_GROUP_LOC);
            eventActionCanvasGroup = eventGroup.GetComponent<CanvasGroup>();
            var leftGroup = Helper.FindDeepChild(eventGroup, LEFT_GROUP_LOC);
            var headingText = Helper.FindDeepChild<TextMeshProUGUI>(leftGroup, HEADING_TEXT_LOC);
            var iconImage = Helper.FindDeepChild<Image>(leftGroup, ICON_IMAGE_LOC);
            var preset = EditorOp.Resolve<EditorSystem>().ComponentIconsPreset;
            var icon = preset.GetIcon(data.EventName);
            headingText.text = data.EventName;
            iconImage.sprite = icon;
        }

        private void PreparePropertiesGroup()
        {
            var propertiesGroup = Helper.FindDeepChild(transform, PROPERTIES_GROUP_LOC);
            var leftGroup = Helper.FindDeepChild(propertiesGroup, LEFT_GROUP_LOC);
            var headingText = Helper.FindDeepChild<TextMeshProUGUI>(leftGroup, HEADING_TEXT_LOC);
            headingText.text = data.DisplayName;
            propertiesActionCanvasGroup = propertiesGroup.GetComponent<CanvasGroup>();
            //var properites = data.GetProperty();
            var broadcast = data.GetBroadcast();
            var isBroadcasting = !string.IsNullOrEmpty(broadcast);
            PrepareBroadcastGroup(isBroadcasting, broadcast);
        }

        private void PrepareBroadcastGroup(bool isAvailable, string broadcast)
        {
            var broadcastGroup = Helper.FindDeepChild(transform, BROADCAST_GROUP_LOC);
            broadcastActionGroup = broadcastGroup.GetComponent<CanvasGroup>();
            if (!isAvailable)
            {
                broadcastGroup.gameObject.SetActive(false);
                return;
            }
            Debug.Log($"broadcasting: {broadcast}");
            //Implement a child which has the broadcast string in it
        }

        private void AddListenerToButton(Button button, Action action)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => action?.Invoke());
        }

        public struct PreviewData
        {
            public string DisplayName;
            public string EventName;
            public Dictionary<string, object>[] Properties;
            public string[] Broadcast;
            public string Listen;

            private int maxProperties;
            private int currentPropertyIndex;
            public readonly int MaxProperties => maxProperties;
            public readonly int CurrentPropertyIndex => currentPropertyIndex;
            public void IterateState() => currentPropertyIndex++;

            public void Init()
            {
                maxProperties = Properties.Length;
                currentPropertyIndex = 0;
            }

            public Dictionary<string, object> GetProperty()
            {
                currentPropertyIndex %= Properties.Length;
                return Properties[currentPropertyIndex];
            }

            public string GetBroadcast()
            {
                currentPropertyIndex %= Broadcast.Length;
                return Broadcast[currentPropertyIndex];
            }
        }
    }
}