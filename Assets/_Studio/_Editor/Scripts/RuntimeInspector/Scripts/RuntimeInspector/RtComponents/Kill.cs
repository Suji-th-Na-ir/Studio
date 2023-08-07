using Newtonsoft.Json;
using Terra.Studio;
using Terra.Studio.RTEditor;
using UnityEngine;

namespace RuntimeInspectorNamespace
{
    
        public class Kill : MonoBehaviour, IComponent
        {
            public (string type, string data) Export()
            {
                return (null,null);
            }

            public void Import(EntityBasedComponent data)
            {
              
            }

           
        }
    
}