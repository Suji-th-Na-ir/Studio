using System.Collections.Generic;
using Terra.Studio;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RuntimeInspectorNamespace
{
	public class HierarchyDataRootScene : HierarchyDataRoot
	{
		public override string Name { get { return Scene.name; } }
		public override int ChildCount { get { return rootObjects.Count; } }

		public Scene Scene { get; private set; }

		private List<GameObject> rootObjects = new List<GameObject>();
		private List<GameObject> filteredObjects = new List<GameObject>();

		public HierarchyDataRootScene( RuntimeHierarchy hierarchy, Scene target ) : base( hierarchy )
		{
			Scene = target;
		}

		public override void RefreshContent()
		{
			rootObjects.Clear();

			if (Scene.isLoaded)
			{
				Scene.GetRootGameObjects(filteredObjects);
				foreach (GameObject goo in filteredObjects)
				{
					if (goo.GetComponent<HideInHierarchy>() == null)
					{
						rootObjects.Add(goo);
					}
				}
			}
		}

		public override Transform GetChild( int index )
		{
			return rootObjects[index].transform;
		}

		public override Transform GetNearestRootOf( Transform target )
		{
			return ( target.gameObject.scene == Scene ) ? target.root : null;
		}
	}
}