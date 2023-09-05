using System;
using RuntimeInspectorNamespace;

namespace Terra.Studio
{
    public class SwitchField : InspectorField
    {
        public override bool SupportsType(Type type)
        {
            return type == typeof(Atom.SwitchData);
        }
    }
}
