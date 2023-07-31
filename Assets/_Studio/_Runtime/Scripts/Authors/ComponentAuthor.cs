using System;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections;
using Newtonsoft.Json.Linq;

namespace Terra.Studio
{
    public class ComponentAuthor : BaseAuthor
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
            var indivCompAuthor = Interop<RuntimeInterop>.Current.Resolve<ComponentsData>().GetAuthorForType(compData.type);
            indivCompAuthor?.Generate((tuple.id, tuple.data.type, jString, tuple.obj));
        }
    }
}
