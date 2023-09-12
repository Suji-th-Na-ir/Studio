using System;
using UnityEngine;
using System.Collections.Generic;

namespace Terra.Studio
{
    public class UndoRedoSystem : IURCommand, IDisposable
    {
        private List<Operation> operations;
        private int currentIndex;

        public event Action<bool> OnUndoStackAvailable;
        public event Action<bool> OnRedoStackAvailable;

        public UndoRedoSystem()
        {
            operations = new();
            currentIndex = -1;
        }

        public void Record(object oldValue, object newValue, string comment, Action<object> onExecuted)
        {
            RefreshStackIfNeeded();
            var stackData = new Operation(onExecuted, oldValue, newValue, comment);
            operations.Add(stackData);
            currentIndex++;
            CheckForStackAvailability();
        }

        public void Undo()
        {
            if (currentIndex == -1)
            {
                Debug.Log(Messages.NOTHING_TO_UNDO);
                return;
            }
            Operate(true);
            --currentIndex;
            CheckForStackAvailability();
        }

        public void Redo()
        {
            if (currentIndex == operations.Count - 1)
            {
                Debug.Log(Messages.NOTHING_TO_REDO);
                return;
            }
            ++currentIndex;
            Operate(false);
            CheckForStackAvailability();
        }

        public void RefreshStackIfNeeded()
        {
            var stackTopIndex = operations.Count - 1;
            if (currentIndex < stackTopIndex)
            {
                while (currentIndex < stackTopIndex)
                {
                    var lastIndex = operations.Count - 1;
                    operations.RemoveAt(lastIndex);
                    --stackTopIndex;
                }
            }
        }

        public void Dispose()
        {
            operations.Clear();
        }

        private void CheckForStackAvailability()
        {
            if (currentIndex < 0)
            {
                OnUndoStackAvailable?.Invoke(false);
                var isRedoStackAvailable = operations.Count > 1;
                OnRedoStackAvailable?.Invoke(isRedoStackAvailable);
            }
            if (currentIndex >= 0)
            {
                OnUndoStackAvailable?.Invoke(true);
                var isRedoStackAvailable = (operations.Count - currentIndex - 1) != 0;
                OnRedoStackAvailable?.Invoke(isRedoStackAvailable);
            }
        }

        private Operation Operate(bool isUndo)
        {
            var operation = operations[currentIndex];
            operation.Execute(isUndo);
            return operation;
        }

        private class Operation
        {
            private readonly Action<object> action;
            private readonly object oldValue;
            private readonly object newValue;
            private readonly string comment;

            public string Comment => comment;

            public Operation(Action<object> action, object oldValue, object newValue, string comment)
            {
                this.action = action;
                this.oldValue = oldValue;
                this.newValue = newValue;
                this.comment = comment;
            }

            public void Execute(bool isUndo)
            {
                var data = isUndo ? oldValue : newValue;
                var msg = isUndo ? Messages.UNDO_IN_PROGRESS : Messages.REDO_IN_PROGRESS;
                Debug.Log($"{msg} {comment}");
                action?.Invoke(data);
            }
        }

        private class Messages
        {
            public const string NOTHING_TO_UNDO = "Nothing to undo!";
            public const string NOTHING_TO_REDO = "Nothing to redo!";
            public const string UNDO_IN_PROGRESS = "Undoing";
            public const string REDO_IN_PROGRESS = "Redoing";
        }
    }
}
