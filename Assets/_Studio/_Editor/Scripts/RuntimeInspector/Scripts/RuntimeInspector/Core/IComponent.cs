using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using Terra.Studio;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace RuntimeInspectorNamespace
{
    public interface IComponent
    {
        (string type, string data) Export();

        void Import(EntityBasedComponent data);
    }
}