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
        private const string BOTTOM_GROUP_LOC = "BottomGroup";
        private const string HEADING_GROUP_LOC = "HeadingGroup";
        private const string CLOSE_BUTTON_LOC = "Close";
        private const string RESTART_BUTTON_LOC = "Restart";
        private const string EVENT_GROUP_LOC = "EventActionGroup";
        private const string BROADCAST_GROUP_LOC = "BroadcastGroup";
        private const string HEADING_TEXT_LOC = "Heading";
        private const string ICON_IMAGE_LOC = "Icon";
        private const string PROPERTY_GROUP_LOC = "Property";
        private const string NAME_FIELD_LOC = "NameText";
        private const string VALUE_FIELD_LOC = "ValueText";
        private const string LISTENTING_FIELD_LOC = "ListeningTo";
        private const string BROADCAST_FIELD_LOC = "Broadcasting";

        private CanvasGroup eventActionCanvasGroup;
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
            broadcastActionGroup.alpha = 0.5f;
        }

        public void ToggleToBroadcastGroup()
        {
            eventActionCanvasGroup.alpha = 0.5f;
            if (!broadcastActionGroup.gameObject.activeSelf)
            {
                return;
            }
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
            ToggleToEventActionGroup();
            PrepareProperties();
            PrepareBroadcastGroup();
        }

        public void Init<T>(T instance) where T : BaseBehaviour
        {
            data = instance.GetPreviewData();
            data.Init();
            PrepareHeadingGroup(instance);
            PrepareEventGroup();
        }

        private void PrepareHeadingGroup<T>(T instance) where T : BaseBehaviour
        {
            var headingGroup = Helper.FindDeepChild(transform, HEADING_GROUP_LOC);
            var leftGroup = Helper.FindDeepChild(headingGroup, LEFT_GROUP_LOC);
            var headingText = Helper.FindDeepChild<TextMeshProUGUI>(leftGroup, HEADING_TEXT_LOC);
            var icon = Helper.FindDeepChild<Image>(leftGroup, ICON_IMAGE_LOC);
            var rightGroup = Helper.FindDeepChild(headingGroup, RIGHT_GROUP_LOC);
            var restartBtn = Helper.FindDeepChild<Button>(rightGroup, RESTART_BUTTON_LOC);
            var closeBtn = Helper.FindDeepChild<Button>(rightGroup, CLOSE_BUTTON_LOC);
            headingText.text = data.DisplayName;
            var preset = EditorOp.Resolve<EditorSystem>().ComponentIconsPreset;
            var sprite = preset.GetIcon(instance.GetType().Name);
            icon.sprite = sprite;
            AddListenerToButton(closeBtn, () => { EditorOp.Resolve<BehaviourPreview>().Preview(instance); });
            AddListenerToButton(restartBtn, () =>
            {
                data.Init();
                EditorOp.Resolve<BehaviourPreview>().Restart(instance);
            });
            PrepareProperties();
        }

        private void PrepareProperties()
        {
            var properties = data.GetProperty();
            var bottomGroup = Helper.FindDeepChild(transform, BOTTOM_GROUP_LOC);
            var propertyField = Helper.FindDeepChild(bottomGroup, PROPERTY_GROUP_LOC);
            if (properties.Count == 0)
            {
                ClearAllFieldsExceptFirstChild(bottomGroup);
                Destroy(propertyField.gameObject);
                return;
            }
            var fields = new GameObject[properties.Count];
            fields[0] = propertyField.gameObject;
            if (bottomGroup.transform.childCount != properties.Count)
            {
                ClearAllFieldsExceptFirstChild(bottomGroup);
                for (int i = 1; i < properties.Count; i++)
                {
                    var newInstance = Instantiate(propertyField.gameObject, bottomGroup);
                    fields[i] = newInstance;
                }
            }
            else
            {
                for (int i = 1; i < properties.Count; i++)
                {
                    fields[i] = bottomGroup.GetChild(i).gameObject;
                }
            }
            var index = -1;
            foreach (var property in properties)
            {
                var field = fields[++index];
                var nameField = Helper.FindDeepChild<TextMeshProUGUI>(field.transform, NAME_FIELD_LOC);
                var valueField = Helper.FindDeepChild<TextMeshProUGUI>(field.transform, VALUE_FIELD_LOC);
                nameField.text = property.Key;
                valueField.text = $"{property.Value}";
            }
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
            var isAvailable = SystemOp.Resolve<System>().SystemData.TryGetEventDisplayName(data.EventName, out var displayName);
            if (!isAvailable)
            {
                displayName = data.EventName;
            }
            headingText.text = displayName;
            iconImage.sprite = icon;
            var bottomGroup = Helper.FindDeepChild(eventGroup, BOTTOM_GROUP_LOC);
            var isListening = !string.IsNullOrEmpty(data.Listen);
            bottomGroup.gameObject.SetActive(isListening);
            if (isListening)
            {
                var listenField = Helper.FindDeepChild<TextMeshProUGUI>(bottomGroup, LISTENTING_FIELD_LOC);
                listenField.text = data.Listen;
            }
            PrepareBroadcastGroup();
        }

        private void PrepareBroadcastGroup()
        {
            var broadcast = data.GetBroadcast();
            var isBroadcasting = !string.IsNullOrEmpty(broadcast);
            var broadcastGroup = Helper.FindDeepChild(transform, BROADCAST_GROUP_LOC);
            broadcastActionGroup = broadcastGroup.GetComponent<CanvasGroup>();
            if (!isBroadcasting)
            {
                broadcastGroup.gameObject.SetActive(false);
                return;
            }
            var bottomGroup = Helper.FindDeepChild(broadcastGroup, BOTTOM_GROUP_LOC);
            var broadcastField = Helper.FindDeepChild<TextMeshProUGUI>(bottomGroup, BROADCAST_FIELD_LOC);
            broadcastField.text = broadcast;
        }

        private void AddListenerToButton(Button button, Action action)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => action?.Invoke());
        }

        private void ClearAllFieldsExceptFirstChild(Transform transform)
        {
            var childCount = transform.childCount;
            if (childCount <= 1) return;
            for (int i = 1; i < childCount; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
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