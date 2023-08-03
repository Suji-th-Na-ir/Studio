using Newtonsoft.Json;
using Terra.Studio;
using Terra.Studio.RTEditor;
using UnityEngine;


namespace RuntimeInspectorNamespace
{
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
