using System;
using UnityEngine;
using System.Linq;
using Terra.Studio;
using System.Reflection;
using RuntimeInspectorNamespace;

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class ComponentListAttribute : PropertyAttribute
{
    public string[] componentNames;

    public ComponentListAttribute()
    {
        componentNames = FetchComponentNames();
    }

    private string[] FetchComponentNames()
    {
        var assembly = Assembly.GetAssembly(typeof(IBaseComponent));
        var componentNames = assembly.GetTypes()
            .Where(type => type.IsValueType && typeof(IBaseComponent).IsAssignableFrom(type))
            .Select(type => type.FullName)
            .ToList();
        componentNames.Insert(0, "None");
        return componentNames.ToArray();
    }
}

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class SystemListAttribute : PropertyAttribute
{
    public string[] systemNames;

    public SystemListAttribute()
    {
        systemNames = FetchSystemNames();
    }

    private string[] FetchSystemNames()
    {
        var assembly = Assembly.GetAssembly(typeof(IBaseComponent));
        var systemNames = assembly.GetTypes()
            .Where(type => type.IsClass && typeof(BaseSystem).IsAssignableFrom(type))
            .Select(type => type.FullName)
            .ToList();
        systemNames.Insert(0, "None");
        return systemNames.ToArray();
    }
}

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class EditorDrawerListAttribute : PropertyAttribute
{
    public string[] drawerNames;

    public EditorDrawerListAttribute()
    {
        drawerNames = FetchDrawerNames();
    }

    private string[] FetchDrawerNames()
    {
        var assembly = Assembly.GetAssembly(typeof(IBaseComponent));
        var systemNames = assembly.GetTypes()
            .Where(type => type.IsClass && typeof(IComponent).IsAssignableFrom(type))
            .Select(type => type.FullName)
            .ToList();
        systemNames.Insert(0, "None");
        return systemNames.ToArray();
    }
}

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class ActionEventListAttribute : PropertyAttribute
{
    public string[] eventNames;

    public ActionEventListAttribute()
    {
        eventNames = FetchActionEventNames();
    }

    private string[] FetchActionEventNames()
    {
        var assembly = Assembly.GetAssembly(typeof(IBaseComponent));
        var eventNames = assembly.GetTypes()
            .Where(type => type.IsValueType && typeof(IEventExecutor).IsAssignableFrom(type))
            .Select(type => type.FullName)
            .ToList();
        eventNames.Insert(0, "None");
        return eventNames.ToArray();
    }
}