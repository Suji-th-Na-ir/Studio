using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Terra.Studio;
using System.Linq;

namespace RuntimeInspectorNamespace
{
    public abstract class InspectorField : MonoBehaviour, ITooltipContent
    {
        public delegate object Getter();
        public delegate void Setter(object value);
        protected Action<string, string> onStringUpdated;

#pragma warning disable 0649
        [SerializeField]
        protected LayoutElement layoutElement;

        [SerializeField] protected bool ignoreLayout = false;

        [SerializeField]
        protected Text variableNameText;

        [SerializeField]
        protected Image variableNameMask;

        [SerializeField]
        private MaskableGraphic visibleArea;
#pragma warning restore 0649

        private RuntimeInspector m_inspector;
        public RuntimeInspector Inspector
        {
            get { return m_inspector; }
            set
            {
                if (m_inspector != value)
                {
                    m_inspector = value;
                    OnInspectorChanged();
                }
            }
        }

        private int m_skinVersion = 0;
        private UISkin m_skin;
        public UISkin Skin
        {
            get { return m_skin; }
            set
            {
                if (m_skin != value || m_skinVersion != m_skin.Version)
                {
                    m_skin = value;
                    m_skinVersion = m_skin.Version;

                    OnSkinChanged();
                    OnDepthChanged();
                }
            }
        }

        private Type m_boundVariableType;
        protected Type BoundVariableType
        {
            get { return m_boundVariableType; }
            set
            {
                m_boundVariableType = value;
            }
        }

        private Type m_obscuredType;
        protected Type ObscuredType
        {
            get
            {
                return m_obscuredType;
            }
        }

        private IObscurer m_obscurer;
        protected IObscurer Obscurer
        {
            get
            {
                return m_obscurer;
            }
        }

        private object m_obscuredValue;
        protected object ObscuredValue => m_obscuredValue;

        private object m_value;
        public object Value
        {
            get { return m_value; }
            protected set
            {
                try { setter(value); m_value = value; UpdateDataForMultiSelect(); }
                catch { }
            }
        }

        private Type m_Component;
        public Type ComponentType
        {
            get { return m_Component; }
            protected set
            {
                if (m_Component != value)
                    m_Component = value;

            }
        }

        private int m_depth = -1;
        public int Depth
        {
            get { return m_depth; }
            set
            {
                if (m_depth != value)
                {
                    m_depth = value;
                    OnDepthChanged();
                }
            }
        }

        private bool m_isVisible = true;
        public bool IsVisible { get { return m_isVisible; } }

        public string Name
        {
            get { if (variableNameText) return variableNameText.text; return string.Empty; }
            set { if (variableNameText) variableNameText.text = Inspector.UseTitleCaseNaming ? value.ToTitleCase() : value; }
        }

        public string NameRaw
        {
            get { if (variableNameText) return variableNameText.text; return string.Empty; }
            set { if (variableNameText) variableNameText.text = value; }
        }

        private string m_ReflectedName;
        public string ReflectedName
        {
            get { return m_ReflectedName; }
            set { if (m_ReflectedName != value) m_ReflectedName = value; }
        }

        bool ITooltipContent.IsActive { get { return this && gameObject.activeSelf; } }
        string ITooltipContent.TooltipText { get { return NameRaw; } }

        public virtual bool ShouldRefresh { get { return m_isVisible; } }

        protected virtual float HeightMultiplier { get { return 1f; } }

        protected object virtualObject;
        protected object VirtualObject { get { return virtualObject; } }

        protected Getter getter;
        protected Setter setter;
        protected object lastSubmittedValue = null;

        public virtual void Initialize()
        {
            if (visibleArea)
                visibleArea.onCullStateChanged.AddListener((bool isCulled) => m_isVisible = !isCulled);
        }

        public abstract bool SupportsType(Type type);

        public virtual bool CanBindTo(Type type, MemberInfo variable)
        {
            return true;
        }

        public void BindTo(InspectorField parent, MemberInfo variable, string variableName = null)
        {
            m_Component = variable.DeclaringType;
            virtualObject = parent.Value;
            if (variable is FieldInfo field)
            {
                var displayNameAttribute = field.GetCustomAttribute<AliasDrawerAttribute>();
                string displayName = displayNameAttribute?.Alias;
                variableName ??= string.IsNullOrEmpty(displayName) ? field.Name : displayName;
#if UNITY_EDITOR || !NETFX_CORE
                if (!parent.BoundVariableType.IsValueType)
#else
				if( !parent.BoundVariableType.GetTypeInfo().IsValueType )
#endif
                {
                    InjectToOnValueChangedIfAvailable(field, parent.Value);
                    var isObscured = IsObscurerTapped(field);
                    if (!isObscured)
                    {
                        BindTo(field.FieldType, variableName, field.Name, () => field.GetValue(parent.Value), (value) => field.SetValue(parent.Value, value), variable);
                    }
                    else
                    {
                        var value = field.GetValue(parent.Value);
                        BindTo(field.FieldType, variableName, field.Name, value, variable);
                    }
                }
                else
                {
                    BindTo(field.FieldType, variableName, field.Name, () => field.GetValue(parent.Value), (value) =>
                    {
                        field.SetValue(parent.Value, value);
                        parent.Value = parent.Value;
                    }, variable);
                }
            }
            else if (variable is PropertyInfo property)
            {
                var displayNameAttribute = property.GetCustomAttribute<AliasDrawerAttribute>();
                string displayName = displayNameAttribute?.Alias;
                variableName ??= string.IsNullOrEmpty(displayName) ? property.Name : displayName;

#if UNITY_EDITOR || !NETFX_CORE
                if (!parent.BoundVariableType.IsValueType)
#else
				if( !parent.BoundVariableType.GetTypeInfo().IsValueType )
#endif
                {
                    BindTo(property.PropertyType, variableName, property.Name, () => property.GetValue(parent.Value, null), (value) => property.SetValue(parent.Value, value, null), variable);
                }
                else
                {
                    BindTo(property.PropertyType, variableName, property.Name, () => property.GetValue(parent.Value, null), (value) =>
                    {
                        property.SetValue(parent.Value, value, null);
                        parent.Value = parent.Value;
                    }, variable);
                }
            }
            else
            {
                throw new ArgumentException("Variable can either be a field or a property");
            }
        }

        private void InjectToOnValueChangedIfAvailable(FieldInfo fieldInfo, object instance)
        {
            var doesHaveOnValueChangedAttribute = fieldInfo.HasAttribute<OnValueChangedAttribute>();
            if (!doesHaveOnValueChangedAttribute) return;
            var attribute = fieldInfo.GetAttribute<OnValueChangedAttribute>();
            onStringUpdated = attribute.OnValueUpdated((BaseBehaviour)instance);
        }

        private bool IsObscurerTapped(FieldInfo fieldInfo)
        {
            var isInterfacedWithObscurer = typeof(IObscurer).IsAssignableFrom(fieldInfo.FieldType);
            return isInterfacedWithObscurer;
        }

        public void BindTo(Type variableType, string variableName, string reflectedName, Getter getter, Setter setter, MemberInfo variable = null)
        {
            if (variable != null && variable is FieldInfo fieldInfo)
            {
                var alias = fieldInfo.GetAliasIfAny();
                if (!string.IsNullOrEmpty(alias))
                {
                    Name = alias;
                }
                else
                {
                    Name = variableName;
                }
            }
            else
            {
                Name = variableName;
            }
            m_boundVariableType = variableType;
            ReflectedName = variableName;

            this.getter = getter;
            this.setter = setter;

            OnBound(variable);
        }

        public void BindTo(Type variableType, string variableName, string reflectedName, object value, MemberInfo variable = null)
        {
            var obscurer = (IObscurer)value;
            m_obscurer = obscurer;
            m_obscuredType = obscurer.ObscureType;
            m_obscuredValue = value;
            BindTo(variableType, variableName, reflectedName, obscurer.Get, obscurer.Set, variable);
        }

        public void Unbind()
        {
            m_boundVariableType = null;

            getter = null;
            setter = null;

            onStringUpdated = null;

            OnUnbound();
            Inspector.PoolDrawer(this);
        }

        protected virtual void OnBound(MemberInfo variable)
        {
            RefreshValue();
            lastSubmittedValue = Value;
        }

        protected virtual void OnUnbound()
        {
            m_value = null;
        }

        protected virtual void OnInspectorChanged()
        {
            if (!variableNameText)
                return;

            if (m_inspector.ShowTooltips)
            {
                TooltipArea tooltipArea = variableNameText.GetComponent<TooltipArea>();
                if (!tooltipArea)
                    tooltipArea = variableNameText.gameObject.AddComponent<TooltipArea>();

                tooltipArea.Initialize(m_inspector.TooltipListener, this);
                variableNameText.raycastTarget = true;
            }
            else
            {
                TooltipArea tooltipArea = variableNameText.GetComponent<TooltipArea>();
                if (tooltipArea)
                {
                    Destroy(tooltipArea);
                    variableNameText.raycastTarget = false;
                }
            }
        }

        protected virtual void OnSkinChanged()
        {
            if (layoutElement && !ignoreLayout)
                layoutElement.SetHeight(Skin.LineHeight * HeightMultiplier);

            if (variableNameText)
                variableNameText.SetSkinText(Skin);

            if (variableNameMask)
                variableNameMask.color = Skin.BackgroundColor;
        }

        protected virtual void OnDepthChanged()
        {
            if (variableNameText != null && !ignoreLayout)
                variableNameText.rectTransform.sizeDelta = new Vector2(-Skin.IndentAmount * Depth, 0f);
        }

        public virtual void Refresh()
        {
            RefreshValue();
        }

        private void RefreshValue()
        {
            try
            {
                m_value = getter();
            }
            catch
            {
#if UNITY_EDITOR || !NETFX_CORE
                if (BoundVariableType?.IsValueType ?? false)
#else
				if( BoundVariableType?.GetTypeInfo().IsValueType ?? false)
#endif
                    m_value = Activator.CreateInstance(BoundVariableType);
                else
                    m_value = null;
            }
        }

        private void UpdateDataForMultiSelect()
        {
            List<GameObject> selectedObjects = EditorOp.Resolve<SelectionHandler>().GetSelectedObjects();
            if (selectedObjects.Count > 1)
            {
                foreach (var obj in selectedObjects)
                {
                    if (obj.gameObject == ((MonoBehaviour)virtualObject).gameObject)
                    {
                        continue;
                    }
                    var component = obj.GetComponent(ComponentType);
                    if (component != null)
                    {
                        var mInfo = ComponentType.GetField(ReflectedName, BindingFlags.Public | BindingFlags.Instance);
                        mInfo?.SetValue(component, Value);
                    }
                }
            }
        }
    }

    public abstract class ExpandableInspectorField : InspectorField
    {
#pragma warning disable 0649
        [SerializeField]
        protected RectTransform drawArea;

        [SerializeField]
        private PointerEventListener expandToggle;
        private RectTransform expandToggleTransform;
        [SerializeField] bool isExpandable = false;

        [SerializeField]
        private LayoutGroup layoutGroup;

        [SerializeField]
        private Image expandArrow; // Expand Arrow's sprite should look right at 0 rotation
#pragma warning restore 0649

        protected readonly List<InspectorField> elements = new List<InspectorField>(8);
        protected readonly List<ExposedMethodField> exposedMethods = new List<ExposedMethodField>();

        protected virtual int Length { get { return elements.Count; } set { } }

        public override bool ShouldRefresh { get { return true; } }

        private bool m_isExpanded = false;
        public bool IsExpanded
        {
            get { return m_isExpanded; }
            set
            {
                m_isExpanded = value;
                drawArea.gameObject.SetActive(m_isExpanded);

                if (expandArrow != null)
                    expandArrow.rectTransform.localEulerAngles = m_isExpanded ? new Vector3(0f, 0f, -90f) : Vector3.zero;

                if (m_isExpanded)
                    Refresh();
            }
        }

        private RuntimeInspector.HeaderVisibility m_headerVisibility = RuntimeInspector.HeaderVisibility.Collapsible;
        public RuntimeInspector.HeaderVisibility HeaderVisibility
        {
            get { return m_headerVisibility; }
            set
            {
                if (m_headerVisibility != value)
                {
                    if (m_headerVisibility == RuntimeInspector.HeaderVisibility.Hidden)
                    {
                        Depth++;
                        layoutGroup.padding.top = Skin.LineHeight;
                        expandToggle.gameObject.SetActive(true);
                    }
                    else if (value == RuntimeInspector.HeaderVisibility.Hidden)
                    {
                        Depth--;
                        layoutGroup.padding.top = 0;
                        expandToggle.gameObject.SetActive(false);
                    }

                    m_headerVisibility = value;

                    if (m_headerVisibility == RuntimeInspector.HeaderVisibility.Collapsible)
                    {
                        if (expandArrow != null)
                            expandArrow.gameObject.SetActive(true);

                        variableNameText.rectTransform.sizeDelta = new Vector2(-(Skin.ExpandArrowSpacing + Skin.LineHeight * 0.5f), 0f);
                    }
                    else if (m_headerVisibility == RuntimeInspector.HeaderVisibility.AlwaysVisible)
                    {
                        if (expandArrow != null)
                            expandArrow.gameObject.SetActive(false);

                        variableNameText.rectTransform.sizeDelta = new Vector2(0f, 0f);

                        if (!m_isExpanded)
                            IsExpanded = true;
                    }
                    else
                    {
                        if (!m_isExpanded)
                            IsExpanded = true;
                    }
                }
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            expandToggleTransform = (RectTransform)expandToggle.transform;
            if (isExpandable)
            {
                expandArrow.gameObject.SetActive(true);
                expandToggle.PointerClick += (_) =>
                {
                    CheckAndExpand();
                };
            }
            else
            {
                expandArrow.gameObject.SetActive(false);
            }

            IsExpanded = m_isExpanded;
        }

        protected void CheckAndExpand()
        {
            if (m_headerVisibility == RuntimeInspector.HeaderVisibility.Collapsible)
                IsExpanded = !m_isExpanded;
        }

        protected override void OnUnbound()
        {
            base.OnUnbound();

            IsExpanded = false;
            HeaderVisibility = RuntimeInspector.HeaderVisibility.Collapsible;

            ClearElements();
        }

        protected override void OnInspectorChanged()
        {
            base.OnInspectorChanged();

            for (int i = 0; i < elements.Count; i++)
                elements[i].Inspector = Inspector;
        }

        protected override void OnSkinChanged()
        {
            base.OnSkinChanged();

            Vector2 expandToggleSizeDelta = expandToggleTransform.sizeDelta;
            expandToggleSizeDelta.y = Skin.LineHeight;
            expandToggleTransform.sizeDelta = expandToggleSizeDelta;

            if (m_headerVisibility != RuntimeInspector.HeaderVisibility.Hidden)
            {
                layoutGroup.padding.top = Skin.LineHeight;

                if (m_headerVisibility == RuntimeInspector.HeaderVisibility.Collapsible)
                {
                    variableNameText.rectTransform.sizeDelta = new Vector2(-(Skin.LineHeight * 0.05f), 0f);
                    variableNameText.fontSize = Skin.HeadingFontSize;
                    variableNameText.supportRichText = true;
                    variableNameText.fontStyle = FontStyle.Bold;
                }
            }

            if (expandArrow != null)
            {
                expandArrow.color = Skin.ExpandArrowColor;
                expandArrow.rectTransform.anchoredPosition = new Vector2(Skin.LineHeight * 0.25f, 0f);
                expandArrow.rectTransform.sizeDelta = new Vector2(Skin.LineHeight * 0.5f, Skin.LineHeight * 0.5f);
            }

            for (int i = 0; i < elements.Count; i++)
                elements[i].Skin = Skin;

            for (int i = 0; i < exposedMethods.Count; i++)
                exposedMethods[i].Skin = Skin;
        }

        protected override void OnDepthChanged()
        {
            Vector2 expandToggleSizeDelta = expandToggleTransform.sizeDelta;
            expandToggleSizeDelta.x = -Skin.IndentAmount * Depth;
            expandToggleTransform.sizeDelta = expandToggleSizeDelta;

            for (int i = 0; i < elements.Count; i++)
                elements[i].Depth = Depth + 1;
        }

        protected void RegenerateElements()
        {
            if (elements.Count > 0 || exposedMethods.Count > 0)
                ClearElements();

            if (Depth < Inspector.NestLimit)
            {
                drawArea.gameObject.SetActive(true);
                GenerateElements();
                GenerateExposedMethodButtons();
                drawArea.gameObject.SetActive(m_isExpanded);
            }
        }

        protected abstract void GenerateElements();

        private void GenerateExposedMethodButtons()
        {
            bool hideRemoveButtonInAny = false;
            if (elements != null)
            {
                if (elements[elements.Count - 1].ComponentType != null)
                {
                    var comp = Inspector.ShownComponents.FirstOrDefault(component => component.ComponentName == elements[elements.Count - 1].ComponentType.Name);
                    if (comp.hideRemoveButton && !hideRemoveButtonInAny)
                    {
                        hideRemoveButtonInAny = true;
                    }
                }
            }
            if (!hideRemoveButtonInAny)
            {
                if (Inspector.ShowRemoveComponentButton && typeof(Component).IsAssignableFrom(BoundVariableType) && !typeof(Transform).IsAssignableFrom(BoundVariableType) && Inspector.currentPageIndex == 1)
                    CreateExposedMethodButton(GameObjectField.removeComponentMethod, () => this, (value) => { });
            }

            ExposedMethod[] methods = BoundVariableType.GetExposedMethods();
            if (methods != null)
            {
                bool isInitialized = Value != null && !Value.Equals(null);
                for (int i = 0; i < methods.Length; i++)
                {
                    ExposedMethod method = methods[i];
                    if ((isInitialized && method.VisibleWhenInitialized) || (!isInitialized && method.VisibleWhenUninitialized))
                        CreateExposedMethodButton(method, () => Value, (value) => Value = value);
                }
            }
        }

        protected virtual void ClearElements()
        {
            for (int i = 0; i < elements.Count; i++)
                elements[i].Unbind();

            for (int i = 0; i < exposedMethods.Count; i++)
                exposedMethods[i].Unbind();

            elements.Clear();
            exposedMethods.Clear();
        }

        public override void Refresh()
        {
            base.Refresh();

            if (m_isExpanded)
            {
                if (Length != elements.Count)
                    RegenerateElements();

                for (int i = 0; i < elements.Count; i++)
                {
                    if (elements[i].ShouldRefresh)
                        elements[i].Refresh();
                }
            }
        }

        public InspectorField CreateDrawerForComponent(Component component, string variableName = null)
        {
            InspectorField variableDrawer = Inspector.CreateDrawerForType(component.GetType(), drawArea, Depth + 1, false);
            if (variableDrawer != null)
            {
                if (variableName == null)
                {
                    var displayNameAttribute = component.GetType().GetCustomAttribute<AliasDrawerAttribute>();
                    string displayName = displayNameAttribute?.Alias;
                    variableName ??= string.IsNullOrEmpty(displayName) ? component.GetType().Name : displayName;
                }

                variableDrawer.BindTo(component.GetType(), string.Empty, string.Empty, () => component, (value) => { });
                variableDrawer.NameRaw = variableName;

                elements.Add(variableDrawer);
            }

            return variableDrawer;
        }

        public InspectorField CreateDrawerForVariable(MemberInfo variable, string variableName = null)
        {
            // xnx 
            if (variable.Name.ToLower() == "enabled")
                return null;
            // xnx

            Type variableType = variable is FieldInfo ? ((FieldInfo)variable).FieldType : ((PropertyInfo)variable).PropertyType;
            if (variable is FieldInfo fi && TryGetHeaderField(fi, out var header))
            {
                var headerDrawer = Inspector.CreateDrawerForType(typeof(HeaderAttribute), drawArea, Depth + 1, true, variable);
                if (headerDrawer != null)
                {
                    headerDrawer.NameRaw = header;
                }
                elements.Add(headerDrawer);
            }
            InspectorField variableDrawer = Inspector.CreateDrawerForType(variableType, drawArea, Depth + 1, true, variable);
            if (variableDrawer != null)
            {
                variableDrawer.BindTo(this, variable, variableName == null ? null : string.Empty);
                if (variableName != null)
                    variableDrawer.NameRaw = variableName;

                elements.Add(variableDrawer);
            }

            return variableDrawer;
        }

        private bool TryGetHeaderField(FieldInfo fieldInfo, out string headerValue)
        {
            headerValue = null;
            var attribs = fieldInfo.GetCustomAttributes(typeof(HeaderAttribute), false) as HeaderAttribute[];
            if (attribs.Length > 0)
            {
                headerValue = attribs[0].header;
                return true;
            }
            return false;
        }

        public InspectorField CreateDrawer(Type variableType, string variableName, Getter getter, Setter setter, bool drawObjectsAsFields = true)
        {
            InspectorField variableDrawer = Inspector.CreateDrawerForType(variableType, drawArea, Depth + 1, drawObjectsAsFields);
            if (variableDrawer != null)
            {
                variableDrawer.BindTo(variableType, variableName == null ? null : string.Empty, variableName == null ? null : string.Empty, getter, setter);
                if (variableName != null)
                    variableDrawer.NameRaw = variableName;

                elements.Add(variableDrawer);
            }

            return variableDrawer;
        }

        public ExposedMethodField CreateExposedMethodButton(ExposedMethod method, Getter getter, Setter setter)
        {
            ExposedMethodField methodDrawer = (ExposedMethodField)Inspector.CreateDrawerForType(typeof(ExposedMethod), drawArea, Depth + 1, false);
            if (methodDrawer != null)
            {
                methodDrawer.BindTo(typeof(ExposedMethod), string.Empty, string.Empty, getter, setter);
                methodDrawer.SetBoundMethod(method);

                exposedMethods.Add(methodDrawer);
            }

            return methodDrawer;
        }
    }
}