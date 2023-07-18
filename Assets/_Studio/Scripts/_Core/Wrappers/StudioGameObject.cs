using System;
using UnityEngine;
using System.Collections.Generic;

namespace Terra.Studio
{
    public class StudioGameObject
    {
        private readonly string _id;
        private readonly GameObject reference;
        public string Id => _id;
        public GameObject Ref => reference;
        public Dictionary<Type, (Type, object)> componentsAttached;

        private StudioGameObject(PrimitiveType primitive)
        {
            componentsAttached = new();
            _id = Guid.NewGuid().ToString("N");
            reference = GameObject.CreatePrimitive(primitive);
            Interop<EditorInterop>.Current.Resolve<StudioGameObjectsHolder>().RegisterStudioGO(this);
        }

        public void RegisterComponent<T1, T2>(T2 behaviour)
        {
            componentsAttached.Add(typeof(T1), (typeof(T2), behaviour));
        }

        public void UnregisterComponent<T>()
        {
            componentsAttached.Remove(typeof(T));
        }

        public StudioEditorBehaviour<T2> GetBehaviour<T1, T2>()
        {
            var behaviour = componentsAttached[typeof(T1)].Item2 as StudioEditorBehaviour<T2>;
            return behaviour;
        }

        public static StudioGameObject CreateGameObject(PrimitiveType primitive)
        {
            return new StudioGameObject(primitive);
        }
    }

    public class StudioGameObjectsHolder
    {
        private List<StudioGameObject> gameObjects;

        private StudioGameObjectsHolder()
        {
            gameObjects = new();
        }

        public static StudioGameObjectsHolder GetReference()
        {
            return new StudioGameObjectsHolder();
        }

        public StudioGameObject Resolve(GameObject gameObject)
        {
            var res = gameObjects.Find(x => x.Ref.Equals(gameObject));
            return res;
        }

        public StudioGameObject Resolve(string referenceID)
        {
            var res = gameObjects.Find(x => x.Id.Equals(referenceID));
            return res;
        }

        public void RegisterStudioGO(StudioGameObject gameObject)
        {
            gameObjects.Add(gameObject);
        }
    }
}
