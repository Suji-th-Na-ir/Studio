using System;
using System.Linq;
using UnityEngine;
using PlayShifu.Terra;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace Terra.Studio
{
    public class Recorder
    {
        public struct GhostDescription
        {
            public Action ToggleGhostMode;
            public Func<Vector3[]> SpawnTRS;
            public bool ShowVisualsOnMultiSelect;
            public Action<object> OnGhostInteracted;
            public Action<bool> OnGhostModeToggled;
            public Func<object> GetLastValue;
            public Func<object> GetRecentValue;
            public bool IsGhostInteractedInLastRecord;
            public GameObject GhostTo;
        }

        private Dictionary<BaseBehaviour, RecordVisualiser> activeRecorders = new();
        private List<GameObject> ghosts = new();

        public void TrackPosition_NoGhostOnMultiselect<T>(T instance, bool enableModification) where T : BaseBehaviour
        {
            var selections = EditorOp.Resolve<SelectionHandler>().GetSelectedObjects();
            if (selections.Count > 1)
            {
                HandleInstance_NoGhostOnMultiselect(instance, RecordVisualiser.Record.Position, enableModification);
            }
            else
            {
                HandleInstance(instance, RecordVisualiser.Record.Position, enableModification, true);
            }
            HandleSelection(SelectionHandler.GizmoId.Move);
        }

        public void TrackPosition_ShowGhostOnMultiselect<T>(T instance, bool enableModification) where T : BaseBehaviour
        {
            var selections = EditorOp.Resolve<SelectionHandler>().GetSelectedObjects();
            if (selections.Count > 1)
            {
                foreach (var selection in selections)
                {
                    if (selection.TryGetComponent<T>(out var component))
                    {
                        HandleInstance(component, RecordVisualiser.Record.Position, enableModification, false);
                    }
                }
            }
            else
            {
                HandleInstance(instance, RecordVisualiser.Record.Position, enableModification, true);
            }
            HandleSelection(SelectionHandler.GizmoId.Move);
        }

        public void TrackRotation_ShowGhostOnMultiselect<T>(T instance, bool enableModification) where T : BaseBehaviour
        {
            var selections = EditorOp.Resolve<SelectionHandler>().GetSelectedObjects();
            if (selections.Count > 1)
            {
                foreach (var selection in selections)
                {
                    if (selection.TryGetComponent<T>(out var component))
                    {
                        HandleInstance(component, RecordVisualiser.Record.Rotation, enableModification, false);
                    }
                }
            }
            else
            {
                HandleInstance(instance, RecordVisualiser.Record.Rotation, enableModification, true);
            }
            HandleSelection(SelectionHandler.GizmoId.Rotate);
        }

        public void TrackGhost(bool isTracking, GameObject ghost)
        {
            if (isTracking)
            {
                ghosts.Add(ghost);
            }
            else
            {
                ghosts.Remove(ghost);
            }
        }

        private void HandleInstance<T>(T instance, RecordVisualiser.Record recorder, bool enableModification, bool isTrulySingleInstance) where T : BaseBehaviour
        {
            var isPresent = ToggleGhostMode(instance);
            if (isPresent) return;
            var trs = instance.GhostDescription.SpawnTRS?.Invoke();
            var visualiser = new RecordVisualiser(instance.GhostDescription.GhostTo,
                recorder,
                instance.GhostDescription.OnGhostInteracted,
                () =>
                {
                    if (isTrulySingleInstance)
                    {
                        HandleUndoRedo_SingleInstance(instance);
                    }
                    else
                    {
                        var references = GetAllComponentsForSelected(instance, false);
                        HandleUndoRedo_MultipleInstances(references);
                    }
                },
                enableModification, trs);
            instance.GhostDescription.OnGhostModeToggled(true);
            activeRecorders[instance] = visualiser;
        }

        private void HandleInstance_NoGhostOnMultiselect<T>(T instance, RecordVisualiser.Record recorder, bool enableModification) where T : BaseBehaviour
        {
            var isPresent = ToggleGhostMode(instance);
            if (isPresent) return;
            var components = GetAllComponentsForSelected(instance, true);
            var trs = instance.GhostDescription.SpawnTRS?.Invoke();
            var visualiser = new RecordVisualiser(instance.GhostDescription.GhostTo,
                recorder,
                (data) =>
                {
                    instance.GhostDescription.OnGhostInteracted?.Invoke(data);
                    foreach (var component in components)
                    {
                        component.GhostDescription.OnGhostInteracted?.Invoke(data);
                    }
                },
                () =>
                {
                    var references = GetAllComponentsForSelected(instance, false);
                    HandleUndoRedo_MultipleInstances(references);
                },
                enableModification, trs);
            instance.GhostDescription.OnGhostModeToggled(true);
            activeRecorders[instance] = visualiser;
        }

        private bool ToggleGhostMode<T>(T instance) where T : BaseBehaviour
        {
            var isPresent = activeRecorders.TryGetValue(instance, out var visualiser);
            if (isPresent)
            {
                visualiser.Dispose();
                activeRecorders.Remove(instance);
                instance.GhostDescription.OnGhostModeToggled(false);
            }
            else
            {
                activeRecorders.Add(instance, null);
            }
            return isPresent;
        }

        private void HandleSelection(SelectionHandler.GizmoId gizmoId)
        {
            var isGhostPresent = ghosts.Count > 0;
            if (isGhostPresent)
            {
                EditorOp.Resolve<SelectionHandler>().OverrideGizmoOntoTarget(ghosts, gizmoId);
            }
            else
            {
                var selections = EditorOp.Resolve<SelectionHandler>().GetSelectedObjects();
                EditorOp.Resolve<SelectionHandler>().OverrideGizmoOntoTarget(selections, gizmoId);
            }
        }

        private void HandleUndoRedo_SingleInstance<T>(T instance) where T : BaseBehaviour
        {
            var description = instance.GhostDescription;
            var lastValue = description.GetLastValue.Invoke();
            var recentValue = description.GetRecentValue.Invoke();
            if (lastValue.Equals(recentValue))
            {
                return;
            }
            EditorOp.Resolve<IURCommand>().Record(
                (lastValue, description.OnGhostInteracted),
                (recentValue, description.OnGhostInteracted),
                $"Vector3 modified: {description.GetRecentValue.Invoke()}",
                (data) =>
                {
                    var (value, action) = ((object, Action<object>))data;
                    action?.Invoke(value);
                });
        }

        private void HandleUndoRedo_MultipleInstances<T>(IEnumerable<T> instances) where T : BaseBehaviour
        {
            var descriptions = instances.Select(x => x.GhostDescription);
            var undoRecord = descriptions.Select(y =>
            {
                var lastValue = y.GetLastValue.Invoke();
                var recentValue = y.GetRecentValue.Invoke();
                if (lastValue.Equals(recentValue))
                {
                    return new MultiselectUndoRedoChanges();
                }
                var data = new MultiselectUndoRedoChanges()
                {
                    value = y.GetLastValue.Invoke(),
                    action = y.OnGhostInteracted
                };
                return data;
            });
            var redoRecord = descriptions.Select(y =>
            {
                var lastValue = y.GetLastValue.Invoke();
                var recentValue = y.GetRecentValue.Invoke();
                if (lastValue.Equals(recentValue))
                {
                    return new MultiselectUndoRedoChanges();
                }
                var data = new MultiselectUndoRedoChanges()
                {
                    value = y.GetRecentValue.Invoke(),
                    action = y.OnGhostInteracted
                };
                return data;
            });
            EditorOp.Resolve<IURCommand>().Record(
                undoRecord.ToList(),
                redoRecord.ToList(),
                $"Multiselected Vector3 modified",
                (data) =>
                {
                    var values = (List<MultiselectUndoRedoChanges>)data;
                    foreach (var value in values)
                    {
                        if (value.value == null)
                        {
                            continue;
                        }
                        value.action?.Invoke(value.value);
                    }
                });
        }

        private IEnumerable<T> GetAllComponentsForSelected<T>(T instance, bool ignoreInstance) where T : BaseBehaviour
        {
            var selections = EditorOp.Resolve<SelectionHandler>().GetSelectedObjects();
            var components = selections.
                Where(x => x.TryGetComponent<T>(out _) == true).
                Select(y => y.GetComponent<T>());
            if (ignoreInstance)
            {
                components = components.Where(x => x != instance);
            }
            return components;
        }

        private struct MultiselectUndoRedoChanges
        {
            public object value;
            public Action<object> action;
        }
    }

    public class RecordVisualiser : IDisposable
    {
        public enum Record
        {
            Position,
            Rotation
        }

        private const string GHOST_RESOURCE_PATH = "Prefabs/Ghost";
        private const string GHOST_MATERIAL_PATH = "Materials/Ghost";
        private readonly GameObject ghost;
        private readonly Material ghostMaterial;
        private readonly Action onGhostDataModified;
        private BaseRecorder baseRecorder;

        public RecordVisualiser(GameObject gameObject, Record recordFor, Action<object> onRecordDataModified, Action onGhostDataModified, bool enableModification, params Vector3[] trs)
        {
            this.onGhostDataModified = onGhostDataModified;
            ghostMaterial = EditorOp.Load<Material>(GHOST_MATERIAL_PATH);
            var ghostObj = EditorOp.Load<GameObject>(GHOST_RESOURCE_PATH);
            ghost = Object.Instantiate(ghostObj);
            ghost.name = string.Concat(ghost.name, "_", gameObject.name);
            RuntimeWrappers.DuplicateGameObject(gameObject, ghost.transform, Vector3.zero);
            RuntimeWrappers.ResolveTRS(ghost, null, trs);
            var child = ghost.transform.GetChild(0);
            child.localPosition = Vector3.zero;
            child.localScale = gameObject.transform.lossyScale;
            Clean();
            ghost.SetActive(true);
            if (enableModification)
            {
                EditorOp.Resolve<EditorSystem>().RequestIncognitoMode(true);
                EditorOp.Resolve<Recorder>().TrackGhost(true, ghost);
                AttachRecorder(recordFor, onRecordDataModified);
            }
        }

        private void Clean()
        {
            var allChildren = Helper.GetChildren(ghost.transform, true);
            for (int i = 0; i < allChildren.Count; i++)
            {
                var child = allChildren[i];
                CleanBehaviour(child);
                CleanCollider(child);
                CleanMaterial(child);
            }
        }

        private void CleanBehaviour(Transform child)
        {
            if (child.TryGetComponent(out BaseBehaviour behaviour))
            {
                Object.Destroy(behaviour);
            }
            if (child.TryGetComponent(out Outline outline))
            {
                Object.Destroy(outline);
            }
            if (child.TryGetComponent(out StudioGameObject gameObject))
            {
                Object.Destroy(gameObject);
            }
        }

        private void CleanCollider(Transform child)
        {
            if (child.TryGetComponent(out Collider collider))
            {
                Object.Destroy(collider);
            }
        }

        private void CleanMaterial(Transform child)
        {
            if (child.TryGetComponent(out MeshFilter mesh))
            {
                var subMeshCount = mesh.mesh.subMeshCount;
                if (child.TryGetComponent(out Renderer renderer))
                {
                    if (renderer.materials.Length > subMeshCount)
                    {
                        var materials = new Material[subMeshCount];
                        for (int i = 0; i < subMeshCount; i++)
                        {
                            materials[i] = ghostMaterial;
                        }
                        renderer.materials = materials;
                    }
                }
            }
            if (child.TryGetComponent(out SkinnedMeshRenderer skinnedMesh))
            {
                var materials = new Material[skinnedMesh.materials.Length];
                for (int i = 0; i < skinnedMesh.materials.Length; i++)
                {
                    materials[i] = ghostMaterial;
                }
                skinnedMesh.materials = materials;
            }
        }

        private void AttachRecorder(Record recordFor, Action<object> onRecordDataModified)
        {
            baseRecorder = recordFor switch
            {
                Record.Position => ghost.AddComponent<PositionRecorder>(),
                Record.Rotation => ghost.AddComponent<RotationRecorder>(),
                _ => throw new NotImplementedException()
            };
            baseRecorder.Init(onRecordDataModified);
        }

        public void Dispose()
        {
            Object.Destroy(ghost);
            EditorOp.Resolve<EditorSystem>().RequestIncognitoMode(false);
            EditorOp.Resolve<Recorder>().TrackGhost(false, ghost);
            onGhostDataModified?.Invoke();
        }

        public abstract class BaseRecorder : MonoBehaviour
        {
            public Action<object> onDataModified;
            protected abstract Func<bool> IsModified { get; }
            protected abstract object Result { get; }
            protected abstract int FRAMES_INTERVAL_TO_CHECK { get; }

            private int currentFrame;

            public virtual void Init(Action<object> onDataModified)
            {
                this.onDataModified = onDataModified;
            }

            private void Update()
            {
                if (++currentFrame >= FRAMES_INTERVAL_TO_CHECK)
                {
                    currentFrame = 0;
                }
                else
                {
                    return;
                }
                if (IsModified?.Invoke() ?? false)
                {
                    onDataModified?.Invoke(Result);
                }
            }
        }

        public class PositionRecorder : BaseRecorder
        {
            private Vector3 cachedPosition;
            protected override int FRAMES_INTERVAL_TO_CHECK => 10;
            protected override object Result => transform.position;
            protected override Func<bool> IsModified => IsDataModified;

            public override void Init(Action<object> onDataModified)
            {
                base.Init(onDataModified);
                cachedPosition = transform.position;
            }

            public bool IsDataModified()
            {
                var didPositionChange = cachedPosition != transform.position;
                cachedPosition = transform.position;
                return didPositionChange;
            }
        }

        public class RotationRecorder : BaseRecorder
        {
            private Vector3 cachedRotation;
            protected override Func<bool> IsModified => IsDataModified;
            protected override object Result => transform.rotation.eulerAngles;
            protected override int FRAMES_INTERVAL_TO_CHECK => 15;

            public override void Init(Action<object> onDataModified)
            {
                base.Init(onDataModified);
                cachedRotation = transform.eulerAngles;
                if (transform.childCount > 0)
                {
                    transform.GetChild(0).localRotation = Quaternion.Euler(Vector3.zero);
                }
            }

            public bool IsDataModified()
            {
                var didRotationChange = cachedRotation != transform.eulerAngles;
                cachedRotation = transform.eulerAngles;
                return didRotationChange;
            }
        }
    }
}