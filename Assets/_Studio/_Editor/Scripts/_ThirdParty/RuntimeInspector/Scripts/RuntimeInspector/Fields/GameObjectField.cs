using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PlayShifu.Terra;
using UnityEngine;
using Terra.Studio;

namespace RuntimeInspectorNamespace
{
    public class GameObjectField : ExpandableInspectorField
    {
        int length;
        protected override int Length { get { return length; } set { length = value; } }

        private string currentTag = null;

        private Getter isActiveGetter, nameGetter, tagGetter;
        private Setter isActiveSetter, nameSetter, tagSetter;
        private PropertyInfo layerProp;

        private readonly List<Component> components = new List<Component>(8);

        private readonly List<bool> componentsExpandedStates = new List<bool>();

        private Type[] addComponentTypes;

        internal static ExposedMethod addComponentMethod = new ExposedMethod(typeof(GameObjectField).GetMethod("AddComponentButtonClicked", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance), new RuntimeInspectorButtonAttribute("Add Behaviour", false, ButtonVisibility.InitializedObjects), false);
        internal static ExposedMethod removeComponentMethod = new ExposedMethod(typeof(GameObjectField).GetMethod("RemoveComponentButtonClicked", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static), new RuntimeInspectorButtonAttribute("Remove Behaviour", false, ButtonVisibility.InitializedObjects), true);

        public override void Initialize()
        {

            base.Initialize();
            length = 0; // 4: active, name, tag, layer
            isActiveGetter = () => ((GameObject)Value).activeSelf;
            isActiveSetter = (value) => ((GameObject)Value).SetActive((bool)value);

            nameGetter = () => ((GameObject)Value).name;
            nameSetter = (value) =>
            {
                ((GameObject)Value).name = (string)value;
                NameRaw = Value.GetNameWithType();

                RuntimeHierarchy hierarchy = Inspector.ConnectedHierarchy;
                // Inspector.currentPageChanged += this.Refresh;
                if (hierarchy)
                    hierarchy.RefreshNameOf(((GameObject)Value).transform);
            };

            tagGetter = () =>
            {
                GameObject go = (GameObject)Value;
                if (!go.CompareTag(currentTag))
                    currentTag = go.tag;

                return currentTag;
            };
            tagSetter = (value) => ((GameObject)Value).tag = (string)value;

            layerProp = typeof(GameObject).GetProperty("layer");
        }

        public override bool SupportsType(Type type)
        {
            return type == typeof(GameObject);
        }

        protected override void OnBound(MemberInfo variable)
        {
            base.OnBound(variable);
            currentTag = ((GameObject)Value).tag;
        }

        protected override void OnUnbound()
        {
            base.OnUnbound();

            components.Clear();
            componentsExpandedStates.Clear();
        }

        protected override void ClearElements()
        {
            componentsExpandedStates.Clear();
            for (int i = 0; i < elements.Count; i++)
            {
                // Don't keep track of non-expandable drawers' or destroyed components' expanded states
                if (elements[i] is ExpandableInspectorField && (elements[i].Value as UnityEngine.Object))
                    componentsExpandedStates.Add(((ExpandableInspectorField)elements[i]).IsExpanded);
            }

            base.ClearElements();
            Length = 1;
        }

        public override void GenerateElements()
        {
            //Region for name, layer and tag
            //if( components.Count == 0 )
            //	return;

            //if (Inspector.currentPageIndex == 0)
            //{
            //	//CreateDrawer(typeof(bool), "Is Active", isActiveGetter, isActiveSetter);
            //	//StringField nameField = CreateDrawer(typeof(string), "Name", nameGetter, nameSetter) as StringField;
            //	//StringField tagField = CreateDrawer(typeof(string), "Tag", tagGetter, tagSetter) as StringField;
            //	//CreateDrawerForVariable(layerProp, "Layer");

            //	//if (nameField)
            //	//	nameField.SetterMode = StringField.Mode.OnSubmit;

            //	//if (tagField)
            //	//	tagField.SetterMode = StringField.Mode.OnSubmit;
            //         }

            for (int i = 0, j = 0; i < components.Count; i++)
            {
                InspectorField componentDrawer = CreateDrawerForComponent(components[i]);
                if (componentDrawer as ExpandableInspectorField && j < componentsExpandedStates.Count && componentsExpandedStates[j++])
                    ((ExpandableInspectorField)componentDrawer).IsExpanded = true;

            }


            bool hideAddButtonInAny = false;
            var comp = Inspector.ShownComponents.FirstOrDefault(component => component.ComponentName == components[components.Count - 1].GetType().Name);
            if (comp.hideAddButton && !hideAddButtonInAny)
            {
                hideAddButtonInAny = true;
            }

            if (!hideAddButtonInAny)
            {
                if (Inspector.ShowAddComponentButton && Inspector.currentPageIndex == 1)
                    CreateExposedMethodButton(addComponentMethod, () => this, (value) => { });
            }

            componentsExpandedStates.Clear();
        }

        public override void Refresh()
        {
            // Refresh components
            components.Clear();
            GameObject go = Value as GameObject;
            if (go)
            {
                go.GetComponents(components);

                for (int i = components.Count - 1; i >= 0; i--)
                {
                    if (!components[i] || (Inspector.currentPageIndex == 0 && (components[i].GetType().Namespace == "RuntimeInspectorNamespace" ||
                        components[i].GetType().Namespace == "Terra.Studio")) ||
                        (Inspector.currentPageIndex == 1 && (components[i].GetType().Namespace != "RuntimeInspectorNamespace" &&
                        components[i].GetType().Namespace != "Terra.Studio"))
                        || !Inspector.ShownComponents.Any(component => component.ComponentName == components[i].GetType().Name))
                    {
                        if (components[i] as Transform == null)
                            components.RemoveAt(i);
                    }

                }
                if (Inspector.currentPageIndex == 0)
                {
                    Length = components.Count;
                }
                else
                {
                    Length = components.Count;
                }

                if (Inspector.ComponentFilter != null)
                    Inspector.ComponentFilter(go, components);
            }

            // Regenerate components' drawers, if necessary
            base.Refresh();
        }

        [UnityEngine.Scripting.Preserve] // This method is bound to addComponentMethod
        private void AddComponentButtonClicked()
        {
            if (EditorOp.Resolve<EditorSystem>().IsIncognitoEnabled)
                return;
            GameObject target = (GameObject)Value;
            if (!target)
                return;
            addComponentTypes = null;
            if (addComponentTypes == null)
            {
                List<Type> componentTypes = new List<Type>(128);

#if UNITY_EDITOR || !NETFX_CORE
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
#else
				// Common Unity assemblies
				IEnumerable<Assembly> assemblies = new HashSet<Assembly> 
				{
					typeof( Transform ).Assembly,
					typeof( RectTransform ).Assembly,
					typeof( Rigidbody ).Assembly,
					typeof( Rigidbody2D ).Assembly,
					typeof( AudioSource ).Assembly
				};
#endif
                // Search assemblies for Component types
                foreach (Assembly assembly in assemblies)
                {
#if (NET_4_6 || NET_STANDARD_2_0) && (UNITY_EDITOR || !NETFX_CORE)
                    if (assembly.IsDynamic)
                        continue;
#endif
                    try
                    {
                        foreach (Type type in assembly.GetExportedTypes())
                        {
                            if (SystemOp.Resolve<Terra.Studio.System>().SystemData.gameEssentialBehaviours.Contains(type))
                            {
                                continue;
                            }

                            bool canSkip = false;
                            foreach (var c in target.GetComponents<BaseBehaviour>())
                            {
                                if(c.GetType().Equals(type))
                                {
                                    canSkip = true;
                                    break;
                                }    
                            }

                            if (canSkip)
                                continue;

                            // show classes that only inherits from Terra.Studio.RTEditor.IComponent
                            if (!type.GetTypeInfo().GetInterfaces().Contains(typeof(IComponent)))
                                continue;

                            if (!typeof(Component).IsAssignableFrom(type))
                                continue;

#if UNITY_EDITOR || !NETFX_CORE
                            if (type.IsGenericType || type.IsAbstract)
#else
							if( type.GetTypeInfo().IsGenericType || type.GetTypeInfo().IsAbstract )
#endif
                                continue;
                            
                            componentTypes.Add(type);
                        }
                    }
                    catch (NotSupportedException) { }
                    catch (System.IO.FileNotFoundException) { }
                    catch (Exception e)
                    {
                        Debug.LogError("Couldn't search assembly for Component types: " + assembly.GetName().Name + "\n" + e.ToString());
                    }
                }

                addComponentTypes = componentTypes.ToArray();
            }

            ObjectReferencePicker.Instance.Skin = Inspector.Skin;
            ObjectReferencePicker.Instance.Show(
                null, (type) =>
                {
                    // Make sure that RuntimeInspector is still inspecting this GameObject
                    if (type != null && target && Inspector && (Inspector.InspectedObject as GameObject) == target)
                    {
                        // surendran instead add comp to all selected objects 
                        var cachedType = (Type)type;
                        var selectedObjs = EditorOp.Resolve<SelectionHandler>().GetPrevSelectedObjects();
                        EditorOp.Resolve<IURCommand>().Record(
                            (cachedType, selectedObjs.ToList(), true),
                            (cachedType, selectedObjs.ToList(), false),
                            $"{cachedType.Name} is added",
                            (obj) =>
                            {
                                var (type, selections, isUndo) = ((Type, List<GameObject>, bool))obj;
                                if (isUndo)
                                {
                                    Detach(selections, type);
                                }
                                else
                                {
                                    Attach(selections, type);
                                }
                            });

                        Attach(selectedObjs, cachedType);
                        Inspector.Refresh();
                    }
                },
                (type) => ((Type)type).FullName,
                (type) =>
                {
                    var displayNameAttribute = ((Type)type).GetCustomAttribute<AliasDrawerAttribute>();
                    string displayName = displayNameAttribute?.Alias;
                    var name = string.IsNullOrEmpty(displayName) ? ((Type)type).FullName : displayName;
                    return name;

                },
                addComponentTypes, null, false, "Add Component", Inspector.Canvas);
        }

        private static void Attach(IEnumerable<GameObject> selections, Type type)
        {
            Component comp = null;
            foreach (var tObject in selections)
            {
                 comp = tObject.AddComponent(type);
            }
            var behaviour = comp as BaseBehaviour;
            if (behaviour)
            {
                behaviour.StartCoroutine(ShowSelectionGhostIfAny(behaviour));
            }
        }

        private static void AttachAndImport(IEnumerable<(GameObject, EntityBasedComponent)> selections, Type type)
        {
            Component comp = null;
            foreach (var (obj, compData) in selections)
            {
                 comp = obj.AddComponent(type);
                var iComp = comp as IComponent;
               
                iComp.Import(compData);
            }
            var behaviour = comp as BaseBehaviour;
            if (behaviour)
            {
                behaviour.StartCoroutine(ShowSelectionGhostIfAny(behaviour));
            }
        }

        private static void Detach(IEnumerable<GameObject> selections, Type type)
        {
            foreach (var tObject in selections)
            {
                var component = tObject.GetComponent(type);
                HideSelectionGhostIfAny(component);
                Destroy(component);
            }
        }

        private static void Detach(IEnumerable<(GameObject, EntityBasedComponent)> selections, Type type)
        {
            foreach (var (obj, _) in selections)
            {
                var component = obj.GetComponent(type);
                HideSelectionGhostIfAny(component);
                Destroy(component);
            }
        }

        [UnityEngine.Scripting.Preserve] // This method is bound to removeComponentMethod
        private static void RemoveComponentButtonClicked(ExpandableInspectorField componentDrawer)
        {
            if (EditorOp.Resolve<EditorSystem>().IsIncognitoEnabled)
                return;
            if (!componentDrawer || !componentDrawer.Inspector)
                return;

            Component component = componentDrawer.Value as Component;
            if (component && component is not Transform)
                componentDrawer.StartCoroutine(RemoveComponentCoroutine(component, componentDrawer.Inspector));

            var cachedType = componentDrawer.Value.GetType();
            var selectedObjs = EditorOp.Resolve<SelectionHandler>().GetPrevSelectedObjects();
            var selectedObjsData = new List<(GameObject, EntityBasedComponent)>();
            for (int i = 0; i < selectedObjs.Count &&
                selectedObjs[i].TryGetComponent(cachedType, out var extractedComponent); i++)
            {
                var templateComponent = extractedComponent as IComponent;
                var exportedData = templateComponent.Export();
                var data = new EntityBasedComponent()
                {
                    type = exportedData.type,
                    data = exportedData.data
                };
                selectedObjsData.Add((selectedObjs[i], data));
            }
            EditorOp.Resolve<IURCommand>().Record(
                (cachedType, selectedObjsData.ToList(), true),
                (cachedType, selectedObjsData.ToList(), false),
                $"{cachedType.Name} is removed",
                (obj) =>
                {
                    var (type, selections, isUndo) = ((Type, List<(GameObject, EntityBasedComponent)>, bool))obj;
                    if (isUndo)
                    {
                        AttachAndImport(selections, type);
                    }
                    else
                    {
                        Detach(selections, type);
                    }
                });

            Detach(selectedObjs, cachedType);
        }

        private static IEnumerator RemoveComponentCoroutine(Component component, RuntimeInspector inspector)
        {
            HideSelectionGhostIfAny(component);
            Destroy(component);

            // Destroy operation doesn't take place immediately, wait for the component to be fully destroyed
            yield return null;

            inspector.Refresh();
            inspector.EnsureScrollViewIsWithinBounds(); // Scroll view's contents can get out of bounds after removing a component
        }

        private static void HideSelectionGhostIfAny(Component component)
        {
            var behaviour = component as BaseBehaviour;
            if (behaviour)
            {
                behaviour.GhostDescription.HideSelectionGhost?.Invoke();
            }
        }

        private static IEnumerator ShowSelectionGhostIfAny(BaseBehaviour behaviour)
        {
            yield return new WaitForSeconds(0.1f);
            var selected = EditorOp.Resolve<SelectionHandler>().GetSelectedObjects();
            for (int i = 0; i < selected.Count; i++)
            {
                var behaviours = selected[i].GetComponents(behaviour.GetType());
                foreach (var b in behaviours)
                {
                    var comp = b as BaseBehaviour;
                    comp.GhostDescription.ShowSelectionGhost?.Invoke();
                }
            }
        } 
    }
}