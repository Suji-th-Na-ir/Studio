using System;
using UnityEngine;

namespace Terra.Studio
{
    public abstract class BaseAuthor : IAuthor
    {
        public virtual void Generate() { }

        public virtual void Generate(object data) { }

        public virtual void Degenerate<T>(int entityID) where T : struct, IBaseComponent { }

        protected struct ComponentGenerateData
        {
            public int entity;
            public EntityBasedComponent data;
            public GameObject obj;
        }

        protected struct ComponentAuthorData
        {
            public int entity;
            public string type;
            public string compData;
            public GameObject obj;
        }
    }

    public class Author<T> where T : IAuthor
    {
        public static T _instance;

        public static IAuthor Current
        {
            get
            {
                _instance ??= Activator.CreateInstance<T>();
                return _instance;
            }
        }

        public static void Flush()
        {
            _instance = default;
        }
    }
}
