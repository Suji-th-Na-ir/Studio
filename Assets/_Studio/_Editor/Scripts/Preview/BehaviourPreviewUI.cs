using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;
using PlayShifu.Terra;

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
            eventActionCanvasGroup.alpha = 0.5f;
            propertiesActionCanvasGroup.alpha = 0.5f;
            broadcastActionGroup.alpha = 1f;
        }

        public void Init<T>(T instance) where T : BaseBehaviour
        {
            PrepareHeadingGroup(instance);
            PrepareEventGroup(instance);
            PreparePropertiesGroup(instance);
            PrepareBroadcastGroup(instance);
        }

        private void PrepareHeadingGroup<T>(T instance) where T : BaseBehaviour
        {
            var headingGroup = Helper.FindDeepChild(transform, HEADING_GROUP_LOC);
            var leftGroup = Helper.FindDeepChild(headingGroup, LEFT_GROUP_LOC);
            var headingText = Helper.FindDeepChild<TextMeshProUGUI>(leftGroup, HEADING_TEXT_LOC);
            var rightGroup = Helper.FindDeepChild(transform, RIGHT_GROUP_LOC);
            var restartBtn = Helper.FindDeepChild<Button>(rightGroup, RESTART_BUTTON_LOC);
            var closeBtn = Helper.FindDeepChild<Button>(rightGroup, CLOSE_BUTTON_LOC);
            AddListenerToButton(restartBtn, () => { EditorOp.Resolve<BehaviourPreview>().Restart(instance); });
            AddListenerToButton(closeBtn, () => { EditorOp.Resolve<BehaviourPreview>().Preview(instance); });
        }

        private void PrepareEventGroup<T>(T instance) where T : BaseBehaviour
        {
            var eventGroup = Helper.FindDeepChild(transform, EVENT_GROUP_LOC);
            eventActionCanvasGroup = eventGroup.GetComponent<CanvasGroup>();
            var leftGroup = Helper.FindDeepChild(eventGroup, LEFT_GROUP_LOC);
            var headingText = Helper.FindDeepChild<TextMeshProUGUI>(leftGroup, HEADING_TEXT_LOC);
            var iconImage = Helper.FindDeepChild<Image>(leftGroup, ICON_IMAGE_LOC);
            var eventName = instance.GetEventCondition();
            var dataManagerSO = SystemOp.Load<RTDataManagerSO>("DataManagerSO");
            var foundData = dataManagerSO.GetEventData(eventName);
            headingText.text = "Clicked";//foundData.DisplayName;
            var componentIconSO = EditorOp.Load<ComponentIconsPreset>("SOs/Component_Icon_SO");
            //var foundIcon = componentIconSO.GetIcon(eventName);
            //iconImage.sprite = foundIcon;
        }

        private void PreparePropertiesGroup<T>(T instance) where T : BaseBehaviour
        {
            var propertiesGroup = Helper.FindDeepChild(transform, PROPERTIES_GROUP_LOC);
            propertiesActionCanvasGroup = propertiesGroup.GetComponent<CanvasGroup>();
            var properites = instance.GetPreviewProperties();
        }

        private void PrepareBroadcastGroup<T>(T instance) where T : BaseBehaviour
        {
            var broadcastGroup = Helper.FindDeepChild(transform, BROADCAST_GROUP_LOC);
            broadcastActionGroup = broadcastGroup.GetComponent<CanvasGroup>();
            var broadcastString = "xyz";
        }

        private void AddListenerToButton(Button button, Action action)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => action?.Invoke());
        }
    }
}