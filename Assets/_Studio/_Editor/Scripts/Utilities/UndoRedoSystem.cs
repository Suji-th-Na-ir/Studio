using System;
using UnityEngine;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace Terra.Studio
{
    public class UndoRedoSystem : IURCommand, IDisposable
    {
        private const int MAX_STACK_LIMIT = 100;
        private const int CLEAR_THRESHOLD = 10;
        private List<Operation> operations;
        private int currentIndex;

        public event Action<bool> OnUndoStackAvailable;
        public event Action<bool> OnRedoStackAvailable;

        public UndoRedoSystem()
        {
            operations = new();
            currentIndex = -1;
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

        private void RefreshStackIfNeeded()
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

        private void MaintainMaxStackLimit()
        {
            if (operations.Count == MAX_STACK_LIMIT)
            {
                var indexToRemove = CLEAR_THRESHOLD;
                while (--indexToRemove >= 0)
                {
                    var operation = operations[indexToRemove];
                    operation.OnDiscarded?.Invoke();
                    operations.RemoveAt(indexToRemove);
                }
                currentIndex -= CLEAR_THRESHOLD;
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
                var isRedoStackAvailable = operations.Count > 0;
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

        public void UpdateReference<T>(T instance)
        {
            var type = typeof(T);
            for (int i = currentIndex; i >= 0 && i < operations.Count; i++)
            {
                var canUpdate = operations[i].CanValidate?.Invoke(type);
                if (canUpdate != null && canUpdate.Value)
                {
                    operations[i].Validate?.Invoke(instance);
                }
            }
        }

        public void Record(object lastData, object newData, string comment, Action<object> onExecuted)
        {
            Record(lastData, newData, comment, onExecuted, null, null, null);
        }

        public void Record(object lastData, object newData, string comment, Action<object> onExecuted, Action onDiscarded)
        {
            Record(lastData, newData, comment, onExecuted, onDiscarded, null, null);
        }

        public void Record(object lastData, object newData, string comment, Action<object> onExecuted, Func<Type, bool> canValidate, Action<object> validate)
        {
            Record(lastData, newData, comment, onExecuted, null, canValidate, validate);
        }

        public void Record(object lastData, object newData, string comment, Action<object> onExecuted, Action onDiscarded, Func<Type, bool> canValidate, Action<object> validate)
        {
            RefreshStackIfNeeded();
            MaintainMaxStackLimit();
            var stackData = new Operation(onExecuted, onDiscarded, canValidate, validate, lastData, newData, comment);
            operations.Add(stackData);
            currentIndex++;
            CheckForStackAvailability();
        }

        private class Operation
        {
            private readonly Action<object> action;
            private readonly Action<object> validate;
            private readonly Action onDiscarded;
            private readonly Func<Type, bool> canValidate;
            private readonly object oldValue;
            private readonly object newValue;
            private readonly string comment;

            public string Comment => comment;
            public Action OnDiscarded => onDiscarded;
            public Func<Type, bool> CanValidate => canValidate;
            public Action<object> Validate => validate;

            public Operation(Action<object> action, Action onDiscarded, Func<Type, bool> canValidate, Action<object> validate, object oldValue, object newValue, string comment)
            {
                this.action = action;
                this.onDiscarded = onDiscarded;
                this.canValidate = canValidate;
                this.validate = validate;
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

    public class Snapshots
    {
        public class TransformSnapshot
        {
            private Vector3 position;
            private Vector3 eulerAngles;
            private Vector3 localScale;
            private Transform transform;
            private TransformSnapshot redoSnapshot;
            private readonly bool shouldStackRecord;

            public static TransformSnapshot CreateSnapshot(GameObject go)
            {
                return new TransformSnapshot(go);
            }

            public TransformSnapshot(GameObject go) : this(go, true) { }

            public TransformSnapshot(GameObject go, bool shouldStackRecord)
            {
                transform = go.transform;
                this.shouldStackRecord = shouldStackRecord;
                CreateTransformSnapshot(go);
                if (shouldStackRecord)
                {
                    StackIntoUndoRedo();
                }
            }

            public void Undo()
            {
                redoSnapshot ??= new TransformSnapshot(transform.gameObject, shouldStackRecord);
                ApplySnapshot();
            }

            public void Redo()
            {
                redoSnapshot?.Undo();
            }

            public void UpdateTransformRefIfMissing(GameObject go)
            {
                if (transform) return;
                transform = go.transform;
            }

            private void CreateTransformSnapshot(GameObject go)
            {
                var transform = go.transform;
                position = transform.position;
                eulerAngles = transform.eulerAngles;
                localScale = transform.localScale;    
            }

            private void ApplySnapshot()
            {
                transform.position = position;
                transform.eulerAngles = eulerAngles;
                transform.localScale = localScale;
            }

            private void StackIntoUndoRedo()
            {
                EditorOp.Resolve<IURCommand>().Record(true, false, $"{transform.gameObject} modified", (isUndoObj) =>
                {
                    var isUndo = (bool)isUndoObj;
                    if (isUndo)
                    {
                        Undo();
                    }
                    else
                    {
                        Redo();
                    }
                });
            }
        }

        public sealed class TransformsSnapshot
        {
            private TransformSnapshot[] snapshots;

            public static TransformsSnapshot CreateSnapshot(List<GameObject> gos)
            {
                return new TransformsSnapshot(gos);
            }

            public TransformsSnapshot(List<GameObject> gos)
            {
                InitializeData(gos);
                StackIntoUndoRedo();
            }

            private void InitializeData(List<GameObject> gos)
            {
                snapshots = new TransformSnapshot[gos.Count];
                for (int i = 0; i < gos.Count; i++)
                {
                    snapshots[i] = new TransformSnapshot(gos[i], false);
                }
            }

            private void StackIntoUndoRedo()
            {
                EditorOp.Resolve<IURCommand>().Record(true, false, "Transform changed", (isUndoObj) =>
                {
                    var isUndo = (bool)isUndoObj;
                    if (isUndo)
                    {
                        Undo();
                    }
                    else
                    {
                        Redo();
                    }
                }, OnDiscarded);
            }

            private void Undo()
            {
                foreach (var snapshot in snapshots)
                {
                    snapshot.Undo();
                }
            }

            private void Redo()
            {
                foreach (var snapshot in snapshots)
                {
                    snapshot.Redo();
                }
            }

            private void OnDiscarded()
            {
            }
        }

        public partial class GameObjectSnapshot
        {
            private string goName;
            protected GameObject currentRef;
            public event Action<GameObject> OnObjectSpawnedBack;

            protected Action OnUndoInvoked;
            protected Action OnRedoInvoked;

            public GameObjectSnapshot(GameObject go) : this(go, true) { }

            public GameObjectSnapshot(GameObject go, bool shouldStackRecord)
            {
                CreateSnapshot(go);
                if (shouldStackRecord) StackIntoUndoRedo();
            }

            protected void DeleteObject()
            {
                currentRef.SetActive(false);
            }

            protected void SpawnBackDeletedObject()
            {
                currentRef.SetActive(true);
                OnObjectSpawnedBack?.Invoke(currentRef);
            }

            public void Undo()
            {
                OnUndoInvoked?.Invoke();
            }

            public void Redo()
            {
                OnRedoInvoked?.Invoke();
            }

            public virtual void Discard() { }

            private void CreateSnapshot(GameObject go)
            {
                currentRef = go;
                goName = go.name;
            }

            private void StackIntoUndoRedo()
            {
                EditorOp.Resolve<IURCommand>().Record(true, false, $"{GetMessage()} {goName}", (isUndoObj) =>
                {
                    var isUndo = (bool)isUndoObj;
                    if (isUndo)
                    {
                        Undo();
                    }
                    else
                    {
                        Redo();
                    }
                });
            }

            protected virtual string GetMessage()
            {
                return "";
            }
        }

        public sealed class SpawnGameObjectSnapshot : GameObjectSnapshot
        {
            public static SpawnGameObjectSnapshot CreateSnapshot(GameObject go)
            {
                return new SpawnGameObjectSnapshot(go);
            }

            public static SpawnGameObjectSnapshot CreateSnapshot(GameObject go, bool shouldStackIntoRecord)
            {
                return new SpawnGameObjectSnapshot(go, shouldStackIntoRecord);
            }

            public SpawnGameObjectSnapshot(GameObject go) : this(go, true) { }

            public SpawnGameObjectSnapshot(GameObject go, bool shouldStackIntoRecord) : base(go, shouldStackIntoRecord)
            {
                OnUndoInvoked = DeleteObject;
                OnRedoInvoked = SpawnBackDeletedObject;
            }

            protected override string GetMessage()
            {
                return "Spawned";
            }
        }

        public sealed class DeleteGameObjectSnapshot : GameObjectSnapshot
        {
            public static DeleteGameObjectSnapshot CreateSnapshot(GameObject go)
            {
                return new DeleteGameObjectSnapshot(go);
            }

            public static DeleteGameObjectSnapshot CreateSnapshot(GameObject go, bool shouldStackIntoRecord)
            {
                return new DeleteGameObjectSnapshot(go, shouldStackIntoRecord);
            }

            public DeleteGameObjectSnapshot(GameObject go) : this(go, true) { }

            public DeleteGameObjectSnapshot(GameObject go, bool shouldStackIntoRecord) : base(go, shouldStackIntoRecord)
            {
                OnUndoInvoked = SpawnBackDeletedObject;
                OnRedoInvoked = DeleteObject;
            }

            protected override string GetMessage()
            {
                return "Deleted";
            }

            public override void Discard()
            {
                Object.Destroy(currentRef);
            }
        }

        public sealed class SpawnGameObjectsSnapshot
        {
            private SpawnGameObjectSnapshot[] snapshots;

            public static SpawnGameObjectsSnapshot CreateSnapshot(List<GameObject> gos)
            {
                return new SpawnGameObjectsSnapshot(gos);
            }

            public SpawnGameObjectsSnapshot(List<GameObject> gos)
            {
                InitializeData(gos);
                StackIntoUndoRedo();
            }

            private void InitializeData(List<GameObject> gos)
            {
                snapshots = new SpawnGameObjectSnapshot[gos.Count];
                for (int i = 0; i < gos.Count; i++)
                {
                    snapshots[i] = new SpawnGameObjectSnapshot(gos[i], false);
                }
            }

            private void StackIntoUndoRedo()
            {
                EditorOp.Resolve<IURCommand>().Record(true, false, "Spawned", (isUndoObj) =>
                {
                    var isUndo = (bool)isUndoObj;
                    if (isUndo)
                    {
                        Undo();
                    }
                    else
                    {
                        Redo();
                    }
                });
            }

            private void Undo()
            {
                foreach (var snapshot in snapshots)
                {
                    snapshot.Undo();
                }
            }

            private void Redo()
            {
                foreach (var snapshot in snapshots)
                {
                    snapshot.Redo();
                }
            }
        }

        public sealed class DeleteGameObjectsSnapshot
        {
            private DeleteGameObjectSnapshot[] snapshots;

            public static DeleteGameObjectsSnapshot CreateSnapshot(List<GameObject> gos)
            {
                return new DeleteGameObjectsSnapshot(gos);
            }

            public DeleteGameObjectsSnapshot(List<GameObject> gos)
            {
                InitializeData(gos);
                StackIntoUndoRedo();
            }

            private void InitializeData(List<GameObject> gos)
            {
                snapshots = new DeleteGameObjectSnapshot[gos.Count];
                for (int i = 0; i < gos.Count; i++)
                {
                    snapshots[i] = new DeleteGameObjectSnapshot(gos[i], false);
                }
            }

            private void StackIntoUndoRedo()
            {
                EditorOp.Resolve<IURCommand>().Record(true, false, "Spawned", (isUndoObj) =>
                {
                    var isUndo = (bool)isUndoObj;
                    if (isUndo)
                    {
                        Undo();
                    }
                    else
                    {
                        Redo();
                    }
                }, OnDiscarded);
            }

            private void Undo()
            {
                foreach (var snapshot in snapshots)
                {
                    snapshot.Undo();
                }
            }

            private void Redo()
            {
                foreach (var snapshot in snapshots)
                {
                    snapshot.Redo();
                }
            }

            private void OnDiscarded()
            {
                foreach (var snapshot in snapshots)
                {
                    snapshot.Discard();
                }
            }
        }

        public sealed class FXModificationSnapshot
        {
            private readonly PlayFXData dirtyFXData;
            private readonly PlayFXData newFXData;
            private readonly string comment;
            private readonly Type derivedType;
            private readonly Action onDataModified;
            private Atom.BasePlay basePlay;

            public static FXModificationSnapshot CreateSnapshot(Atom.BasePlay basePlay, Type derivedType, PlayFXData dirtyFXData, PlayFXData newFXData, string comment, Action onDataModified)
            {
                return new FXModificationSnapshot(basePlay, derivedType, dirtyFXData, newFXData, comment, onDataModified);
            }

            public FXModificationSnapshot(Atom.BasePlay basePlay, Type derivedType, PlayFXData dirtyFXData, PlayFXData newFXData, string comment, Action onDataModified)
            {
                this.basePlay = basePlay;
                this.dirtyFXData = dirtyFXData;
                this.newFXData = newFXData;
                this.comment = comment;
                this.derivedType = derivedType;
                this.onDataModified = onDataModified;
                StackIntoUndoRedo();
            }

            private void StackIntoUndoRedo()
            {
                EditorOp.Resolve<IURCommand>().Record(true, false, comment, (isUndoObj) =>
                {
                    var isUndo = (bool)isUndoObj;
                    if (isUndo)
                    {
                        basePlay.data = dirtyFXData;
                    }
                    else
                    {
                        basePlay.data = newFXData;
                    }
                    onDataModified?.Invoke();
                }, CanValidate, UpdateReference);
            }

            private bool CanValidate(Type type)
            {
                return type == derivedType;
            }

            private void UpdateReference(object instance)
            {
                var baseInstance = (Atom.BasePlay)instance;
                if (baseInstance.target != basePlay.target) return;
                if (string.IsNullOrEmpty(basePlay.fieldName) && !string.IsNullOrEmpty(baseInstance.fieldName)) return;
                if (!string.IsNullOrEmpty(basePlay.fieldName) && !basePlay.fieldName.Equals(baseInstance.fieldName)) return;
                basePlay = baseInstance;
            }
        }
    }
}