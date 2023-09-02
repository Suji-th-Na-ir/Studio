using System;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ComponentListAttribute))]
public class ComponentListDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ComponentListAttribute listAttribute = (ComponentListAttribute)attribute;

        if (listAttribute.componentNames != null)
        {
            int selectedIndex = Mathf.Max(0, Array.IndexOf(listAttribute.componentNames, property.stringValue));

            EditorGUI.BeginChangeCheck();
            selectedIndex = EditorGUI.Popup(position, label.text, selectedIndex, listAttribute.componentNames);

            if (EditorGUI.EndChangeCheck())
            {
                property.stringValue = listAttribute.componentNames[selectedIndex];
            }
        }
        else
        {
            EditorGUI.LabelField(position, label.text, "Component list is empty.");
        }
    }
}

[CustomPropertyDrawer(typeof(SystemListAttribute))]
public class SystemListDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SystemListAttribute listAttribute = (SystemListAttribute)attribute;

        if (listAttribute.systemNames != null)
        {
            int selectedIndex = Mathf.Max(0, Array.IndexOf(listAttribute.systemNames, property.stringValue));

            EditorGUI.BeginChangeCheck();
            selectedIndex = EditorGUI.Popup(position, label.text, selectedIndex, listAttribute.systemNames);

            if (EditorGUI.EndChangeCheck())
            {
                property.stringValue = listAttribute.systemNames[selectedIndex];
            }
        }
        else
        {
            EditorGUI.LabelField(position, label.text, "System list is empty.");
        }
    }
}

[CustomPropertyDrawer(typeof(EditorDrawerListAttribute))]
public class EditorDrawerListDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorDrawerListAttribute listAttribute = (EditorDrawerListAttribute)attribute;

        if (listAttribute.drawerNames != null)
        {
            int selectedIndex = Mathf.Max(0, Array.IndexOf(listAttribute.drawerNames, property.stringValue));

            EditorGUI.BeginChangeCheck();
            selectedIndex = EditorGUI.Popup(position, label.text, selectedIndex, listAttribute.drawerNames);

            if (EditorGUI.EndChangeCheck())
            {
                property.stringValue = listAttribute.drawerNames[selectedIndex];
            }
        }
        else
        {
            EditorGUI.LabelField(position, label.text, "Drawer list is empty.");
        }
    }
}

[CustomPropertyDrawer(typeof(ActionEventListAttribute))]
public class ActionEventListDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ActionEventListAttribute listAttribute = (ActionEventListAttribute)attribute;

        if (listAttribute.eventNames != null)
        {
            int selectedIndex = Mathf.Max(0, Array.IndexOf(listAttribute.eventNames, property.stringValue));

            EditorGUI.BeginChangeCheck();
            selectedIndex = EditorGUI.Popup(position, label.text, selectedIndex, listAttribute.eventNames);

            if (EditorGUI.EndChangeCheck())
            {
                property.stringValue = listAttribute.eventNames[selectedIndex];
            }
        }
        else
        {
            EditorGUI.LabelField(position, label.text, "Drawer list is empty.");
        }
    }
}
