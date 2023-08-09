using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using Terra.Studio;
#if UNITY_EDITOR
using UnityEditor.AssetImporters;
#endif
using UnityEngine;

namespace RuntimeInspectorNamespace
{
    public interface IComponent
    {
        (string type, string data) Export();

        void Import(EntityBasedComponent data);
    }
}