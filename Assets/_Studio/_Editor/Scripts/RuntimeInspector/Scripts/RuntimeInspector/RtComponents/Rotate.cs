using Newtonsoft.Json;
using Terra.Studio;
using Terra.Studio.RTEditor;
using UnityEngine;


namespace RuntimeInspectorNamespace
{
    public class Rotate : MonoBehaviour, IComponent
    {
        public GlobalEnums.StartOn Start = GlobalEnums.StartOn.GameStart;
        public GlobalEnums.RotationType Type = GlobalEnums.RotationType.RotateOnce;
        public GlobalEnums.Axis Axis = GlobalEnums.Axis.X;
        public GlobalEnums.Direction Direction = GlobalEnums.Direction.Clockwise;
        public float Degrees = 0f;
        public float Speed = 0f;
        public Atom.PlaySfx PlaySFX = new Atom.PlaySfx();
        public Atom.PlayVfx PlayVFX = new Atom.PlayVfx();
        public string Broadcast = "";
    
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
