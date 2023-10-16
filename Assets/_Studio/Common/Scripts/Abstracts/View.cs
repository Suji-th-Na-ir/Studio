using UnityEngine;

namespace Terra.Studio
{
    public abstract class View : MonoBehaviour
    {
        public virtual void Init() { }
        public virtual void Draw() { }
        public virtual void Flush() { }
        public virtual void Repaint() { }
        public virtual GameObject AttachDynamicUI(int entity, GameObject go) { return default; }
    }
}
