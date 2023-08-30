using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
using Terra.Studio;
using UnityEngine.UI;

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

		private readonly List<Component> components = new List<Component>( 8 );

        private readonly List<bool> componentsExpandedStates = new List<bool>();

		private Type[] addComponentTypes;

		internal static ExposedMethod addComponentMethod = new ExposedMethod( typeof( GameObjectField ).GetMethod( "AddComponentButtonClicked", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance ), new RuntimeInspectorButtonAttribute( "Add Component", false, ButtonVisibility.InitializedObjects ), false );
		internal static ExposedMethod removeComponentMethod = new ExposedMethod( typeof( GameObjectField ).GetMethod( "RemoveComponentButtonClicked", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static ), new RuntimeInspectorButtonAttribute( "Remove Component", false, ButtonVisibility.InitializedObjects ), true );

		public override void Initialize()
		{
			
			base.Initialize();
			length = 4; // 4: active, name, tag, layer
            isActiveGetter = () => ( (GameObject) Value ).activeSelf;
			isActiveSetter = ( value ) => ( (GameObject) Value ).SetActive( (bool) value );

			nameGetter = () => ( (GameObject) Value ).name;
			nameSetter = ( value ) =>
			{
				( (GameObject) Value ).name = (string) value;
				NameRaw = Value.GetNameWithType();

				RuntimeHierarchy hierarchy = Inspector.ConnectedHierarchy;
               // Inspector.currentPageChanged += this.Refresh;
                if ( hierarchy )
					hierarchy.RefreshNameOf( ( (GameObject) Value ).transform );
			};

			tagGetter = () =>
			{
				GameObject go = (GameObject) Value;
				if( !go.CompareTag( currentTag ) )
					currentTag = go.tag;

				return currentTag;
			};
			tagSetter = ( value ) => ( (GameObject) Value ).tag = (string) value;

			layerProp = typeof( GameObject ).GetProperty( "layer" );	
		}

		public override bool SupportsType( Type type )
		{
			return type == typeof( GameObject );
		}

		protected override void OnBound( MemberInfo variable )
		{
			base.OnBound( variable );
			currentTag = ( (GameObject) Value ).tag;
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
			for( int i = 0; i < elements.Count; i++ )
			{
				// Don't keep track of non-expandable drawers' or destroyed components' expanded states
				if( elements[i] is ExpandableInspectorField && ( elements[i].Value as Object ) )
					componentsExpandedStates.Add( ( (ExpandableInspectorField) elements[i] ).IsExpanded );
			}
		
			base.ClearElements();
            Length = 4;
        }

		protected override void GenerateElements()
		{
			//if( components.Count == 0 )
			//	return;
		
			if (Inspector.currentPageIndex == 0)
			{
				CreateDrawer(typeof(bool), "Is Active", isActiveGetter, isActiveSetter);
				StringField nameField = CreateDrawer(typeof(string), "Name", nameGetter, nameSetter) as StringField;
				StringField tagField = CreateDrawer(typeof(string), "Tag", tagGetter, tagSetter) as StringField;
				CreateDrawerForVariable(layerProp, "Layer");

				if (nameField)
					nameField.SetterMode = StringField.Mode.OnSubmit;

				if (tagField)
					tagField.SetterMode = StringField.Mode.OnSubmit;
            }

            for (int i = 0, j = 0; i < components.Count; i++)
			{
				InspectorField componentDrawer = CreateDrawerForComponent(components[i]);
				if (componentDrawer as ExpandableInspectorField && j < componentsExpandedStates.Count && componentsExpandedStates[j++])
					((ExpandableInspectorField)componentDrawer).IsExpanded = true;
			}

			

			if (Inspector.ShowAddComponentButton && Inspector.currentPageIndex == 1)
				CreateExposedMethodButton(addComponentMethod, () => this, (value) => { });

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
						|| !Inspector.ShownComponents.Contains(components[i].GetType().Name))
					{
						if (components[i] as Transform == null)
							components.RemoveAt(i);
					}

				}
                if (Inspector.currentPageIndex == 0)
                {
                    Length = components.Count + 4;
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
			GameObject target = (GameObject) Value;
			if( !target )
				return;

			if( addComponentTypes == null )
			{
				List<Type> componentTypes = new List<Type>( 128 );

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
				foreach( Assembly assembly in assemblies )
				{
#if( NET_4_6 || NET_STANDARD_2_0 ) && ( UNITY_EDITOR || !NETFX_CORE )
					if( assembly.IsDynamic )
						continue;
#endif
					try
					{
						foreach( Type type in assembly.GetExportedTypes() )
						{
							// show classes that only inherits from Terra.Studio.RTEditor.IComponent
							if(!type.GetTypeInfo().GetInterfaces().Contains(typeof(IComponent)))
								continue;
							
							if( !typeof( Component ).IsAssignableFrom( type ) )
								continue;

#if UNITY_EDITOR || !NETFX_CORE
							if( type.IsGenericType || type.IsAbstract )
#else
							if( type.GetTypeInfo().IsGenericType || type.GetTypeInfo().IsAbstract )
#endif
								continue;

							componentTypes.Add( type );
						}
					}
					catch( NotSupportedException ) { }
					catch( System.IO.FileNotFoundException ) { }
					catch( Exception e )
					{
						Debug.LogError( "Couldn't search assembly for Component types: " + assembly.GetName().Name + "\n" + e.ToString() );
					}
				}

				addComponentTypes = componentTypes.ToArray();
			}

			ObjectReferencePicker.Instance.Skin = Inspector.Skin;
			ObjectReferencePicker.Instance.Show(
				null, ( type ) =>
				{
					// Make sure that RuntimeInspector is still inspecting this GameObject
					if( type != null && target && Inspector && ( Inspector.InspectedObject as GameObject ) == target )
					{
						// xcx instead add comp to all selected objects 
						//target.AddComponent( (Type) type );
						foreach (GameObject tObject in EditorOp.Resolve<SelectionHandler>().GetPrevSelectedObjects())
						{
							tObject.AddComponent((Type)type);
							EditorOp.Resolve<UILogicDisplayProcessor>().AddComponentIcon(new ComponentDisplayDock()
							{ componentGameObject = tObject, componentType = ((Type)type).Name });
						}
						Inspector.Refresh();
					}
				},
				( type ) => ( (Type) type ).FullName,
				( type ) => ( (Type) type ).FullName,
				addComponentTypes, null, false, "Add Component", Inspector.Canvas );
		}

		[UnityEngine.Scripting.Preserve] // This method is bound to removeComponentMethod
		private static void RemoveComponentButtonClicked( ExpandableInspectorField componentDrawer )
		{
			if( !componentDrawer || !componentDrawer.Inspector )
				return;

			Component component = componentDrawer.Value as Component;
			if( component && !( component is Transform ) )
				componentDrawer.StartCoroutine( RemoveComponentCoroutine( component, componentDrawer.Inspector ) );
			
			// surendran - destroy same component on other selected objects.
			List<GameObject> selectedObjects = EditorOp.Resolve<SelectionHandler>().GetSelectedObjects();
			foreach (var obj in selectedObjects)
			{
				EditorOp.Resolve<UILogicDisplayProcessor>().RemoveComponentIcon(new ComponentDisplayDock()
				{ componentGameObject = obj, componentType = componentDrawer.Value.GetType().Name });

                Destroy(obj.GetComponent(componentDrawer.Value.GetType()));
			}
		}

		private static IEnumerator RemoveComponentCoroutine( Component component, RuntimeInspector inspector )
		{
			Destroy( component );

			// Destroy operation doesn't take place immediately, wait for the component to be fully destroyed
			yield return null;

			inspector.Refresh();
			inspector.EnsureScrollViewIsWithinBounds(); // Scroll view's contents can get out of bounds after removing a component
		}
	}
}