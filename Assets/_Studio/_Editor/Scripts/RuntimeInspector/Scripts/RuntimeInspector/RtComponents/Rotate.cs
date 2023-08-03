using Newtonsoft.Json;
using Terra.Studio;
using Terra.Studio.RTEditor;
using UnityEngine;


namespace RuntimeInspectorNamespace
{
    public class RotateComponentData
    {
        public string rotateType = null;
        public string axis = null;
        public string direction = null;
        public float degrees = 0f;
        public float speed = 0f;
        public float repeat = 0f;
        public float increment = 0f;
        public float pauseBetween = 0f;

    }
    public class Rotate : MonoBehaviour, IComponent
    {
        public StartOn Start = StartOn.GameStart;
        public Atom.Rotate Rtotate = new Atom.Rotate(); 
        
        public (string type, string data) Export()
        {
            RotateComponent rotateComponent = new();
            rotateComponent.IsBroadcastable = true;
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var data = JsonConvert.SerializeObject(rotateComponent);
            return (type, data);
        }

        public void Import(EntityBasedComponent data)
        {
            
        }
    }
}
