using Terra.Studio;

namespace RuntimeInspectorNamespace
{
    public interface IComponent
    {
        (string type, string data) Export();

        void Import(EntityBasedComponent data);
    }
}