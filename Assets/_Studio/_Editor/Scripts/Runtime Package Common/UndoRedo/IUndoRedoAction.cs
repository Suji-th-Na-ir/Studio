using UnityEngine;

namespace RuntimeCommon
{
    public interface IUndoRedoAction
    {
        void Execute();
        void Undo();
        void Redo();
        void OnRemovedFromUndoRedoStack();
    }
}
