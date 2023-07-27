using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Terra.Studio
{
    public abstract class BaseAuthor : IAuthor
    {
        public virtual void Generate()
        {

        }

        public virtual void Generate(object data)
        {

        }

        public virtual void Degenerate(int entityID)
        {

        }
    }

    public class Author<T> where T : IAuthor
    {
        public static T _instance;

        public static IAuthor Current
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Activator.CreateInstance<T>();
                }
                return _instance as IAuthor;
            }
        }

        public static void Flush()
        {
            _instance = default;
        }
    }
}
