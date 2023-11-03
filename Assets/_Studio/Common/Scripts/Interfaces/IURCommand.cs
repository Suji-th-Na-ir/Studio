using System;

namespace Terra.Studio
{
    public interface IURCommand
    {
        public event Action<bool> OnUndoStackAvailable;
        public event Action<bool> OnRedoStackAvailable;
        public int CurrentIndex { get; }
        public void Undo();
        public void Redo();
        public void Record(object lastData, object newData, string comment, Action<object> onExecuted);
        public void Record(object lastData, object newData, string comment, Action<object> onExecuted, Action onDiscarded);
        public void Record(object lastData, object newData, string comment, Action<object> onExecuted, Func<Type, bool> canValidate, Action<object> validate);
        public void Record(object lastData, object newData, string comment, Action<object> onExecuted, Action onDiscarded, Func<Type, bool> canValidate, Action<object> validate);
        public void UpdateReference<T>(T instance);
    }
}
