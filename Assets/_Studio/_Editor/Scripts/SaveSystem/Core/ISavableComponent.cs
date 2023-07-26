using System;

namespace Terra.Studio
{
    public interface ISavableComponent
    {
        int uniqueID { get; }
        int executionOrder { get; }

        ComponentData Serialize();

        void Deserialize(ComponentData data);
    }
}
