using System;

namespace Terra.Studio
{
    public interface IURCommand
    {
        public event Action<bool> onUndoStackAvailable;
        public event Action<bool> onRedoStackAvailable;
        public void Undo();
        public void Redo();
        public void Record(object lastData, object newData, string comment, Action<object> onExecuted);
        public void RefreshStackIfNeeded();
    }
}
