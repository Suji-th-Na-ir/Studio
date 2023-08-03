using UnityEngine;

namespace Terra.Studio
{
    public abstract class View : MonoBehaviour
    {
        public abstract void Init();
        public abstract void Draw();
        public abstract void Flush();
        public abstract void Repaint();
    }
}
