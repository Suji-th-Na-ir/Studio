using System;

namespace Terra.Studio
{
    public interface IURCommand
    {
        public event Action<bool> OnUndoStackAvailable;
        public event Action<bool> OnRedoStackAvailable;
        public void Undo();
        public void Redo();
        public void Record(object lastData, object newData, string comment, Action<object> onExecuted);
    }
}
