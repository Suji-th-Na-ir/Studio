using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
