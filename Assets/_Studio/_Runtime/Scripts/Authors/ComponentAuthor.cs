using System;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections;
using Newtonsoft.Json.Linq;

namespace Terra.Studio
{
    public class ComponentAuthorOp
    {
        public static IAuthor Author => Author<ComponentAuthor>.Current;

        public static void Generate()
        {
            Author.Generate();
        }

        public static void Generate(object data)
        {
            Author.Generate(data);
        }

        public static void Degenerate(int entityID)
        {
            Author.Degenerate(entityID);
        }

        public static void Flush()
        {
            Author<ComponentAuthor>.Flush();
        }

        private class ComponentAuthor : BaseAuthor
        {
            public override void Generate(object data)
            {
                var tuple = ((int id, EntityBasedComponent data, GameObject obj))data;
                var compData = tuple.data;
                if (compData.Equals(default(EntityBasedComponent)))
                {
                    return;
                }
                string jString = null;
                try
                {
                    var jObject = (JObject)JToken.FromObject(compData.data);
                    jString = jObject.ToString();
                }
                catch
                {
                    jString = $"{compData.data}";
                }
                var indivCompAuthor = RuntimeOp.Resolve<ComponentsData>().GetAuthorForType(compData.type);
                indivCompAuthor?.Generate((tuple.id, tuple.data.type, jString, tuple.obj));
            }
        }
    }
}
