using System;
using UnityEngine;

namespace Terra.Studio
{
    public interface IMouseEvents
    {
        public Action<GameObject> OnClicked { get; set; }
    }
}
