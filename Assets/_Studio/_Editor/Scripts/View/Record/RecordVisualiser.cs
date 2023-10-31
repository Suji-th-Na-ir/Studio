using System;
using System.Linq;
using UnityEngine;
using PlayShifu.Terra;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace Terra.Studio
{
    public class Recorder : IDisposable
    {
        public Recorder()
        {
            Init();
        }

        public struct GhostDescription
        {
            public Action ToggleRecordMode;
            public Action ShowSelectionGhost;
            public Action HideSelectionGhost;
            public Action UpdateSlectionGhostTRS;
            public Action UpdateSelectionGhostsRepeatCount;
            public Func<Vector3[]> SelectionGhostsTRS;
            public Action<object> OnGhostInteracted;
            public Action<bool> OnGhostModeToggled;
            public Func<object> GetLastValue;
            public Func<object> GetRecentValue;
            public bool IsGhostInteractedInLastRecord;
            public GameObject GhostTo;
            public bool showGhostWithTravelLine;
        }
        public Dictionary<BaseBehaviour, List<RecordVisualiser>> activeSelectionRecorders = new();
        private List<GameObject> ghosts = new();

        private void Init()
        {
            EditorOp.Resolve<SelectionHandler>().SelectionChanged += OnSelectionChanged;
        }


        public void Dispose()
        {
            EditorOp.Resolve<SelectionHandler>().SelectionChanged -= OnSelectionChanged;
        }

        private void OnSelectionChanged(List<GameObject> selected)
        {
            foreach (var g in selected)
            {
                var behaviours = g.GetComponents<BaseBehaviour>();
                foreach (var b in behaviours)
                {
                    if (!activeSelectionRecorders.TryGetValue(b, out var value))
                    {
                        b.GhostDescription.ShowSelectionGhost?.Invoke();
                    }
                }
            }
            var previousSelected = EditorOp.Resolve<SelectionHandler>().GetPrevSelectedObjects();
          
            foreach (var p in previousSelected)
            {
                if (selected.Contains(p))
                    continue;
                var behaviours = p.GetComponents<BaseBehaviour>();
                foreach (var b in behaviours)
                {
                    if (activeSelectionRecorders.TryGetValue(b, out var value))
                    {
                        b.GhostDescription.HideSelectionGhost?.Invoke();
                    }
                }
            }
        }

        public void TrackPosition_Multiselect<T>(T instance,bool noGhost) where T : BaseBehaviour
        {
            var selections = EditorOp.Resolve<SelectionHandler>().GetSelectedObjects();
            if (selections.Count > 1)
            {
                if (noGhost)
                {
                    HandleInstance_NoGhostOnMultiselect(instance);
                }
                else
                {
                    foreach (var selection in selections)
                    {
                        if (selection.TryGetComponent<T>(out var component))
                        {
                            HandleInstance(component);
                        }
                    }
                }
            }
            else
            {
                HandleInstance(instance);
            }
            HandleSelection(SelectionHandler.GizmoId.Move);
        }

        public void TrackRotation_ShowGhostOnMultiselect<T>(T instance) where T : BaseBehaviour
        {
            var selections = EditorOp.Resolve<SelectionHandler>().GetSelectedObjects();
            if (selections.Count > 1)
            {
                foreach (var selection in selections)
                {
                    if (selection.TryGetComponent<T>(out var component))
                    {
                        HandleInstance(component);
                    }
                }
            }
            else
            {
                HandleInstance(instance);
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

        public void ShowSelectionGhost_Position<T>(T instance, bool show) where T : BaseBehaviour
        {
            ShowSelectionGhost_RepeatPosition(instance, 1, show, RepeatDirectionType.SameDirection);
        }

        public void ShowSelectionGhost_Rotation<T>(T instance, bool show) where T : BaseBehaviour
        {
            var selections = EditorOp.Resolve<SelectionHandler>().GetSelectedObjects();
            if (selections.Count > 1)
            {
                foreach (var selection in selections)
                {
                    if (selection.TryGetComponent<T>(out var component))
                    {
                        HandleSelectionInstance(component, RecordVisualiser.Record.Rotation, show, 1, false, RepeatDirectionType.SameDirection);
                    }
                }
            }
            else
            {
                HandleSelectionInstance(instance, RecordVisualiser.Record.Rotation, show, 1, true, RepeatDirectionType.SameDirection);
            }
           
        }

        public void ShowSelectionGhost_RepeatPosition<T>(T instance, int repeatCount, bool show, RepeatDirectionType directionType) where T : BaseBehaviour
        {
            if (repeatCount == 0)
                return;
            if (repeatCount == 1)
            {
                directionType = RepeatDirectionType.SameDirection;
            }
            var selections = EditorOp.Resolve<SelectionHandler>().GetSelectedObjects();
            if (selections.Count > 1)
            {
                foreach (var selection in selections)
                {
                    if (selection.TryGetComponent<T>(out var component))
                    {
                        HandleSelectionInstance(component, RecordVisualiser.Record.Position, show, repeatCount, false, directionType);
                    }
                }
            }
            else
            {
                HandleSelectionInstance(instance, RecordVisualiser.Record.Position, show, repeatCount, true, directionType);
            }
          
        }

        public void UpdateGhostRepeatCount_Multiselect<T>(T instance, int count, RepeatDirectionType directionType) where T : BaseBehaviour
        {
            var selections = EditorOp.Resolve<SelectionHandler>().GetSelectedObjects();
            if (selections.Count > 1)
            {
                foreach (var selection in selections)
                {
                    if (selection.TryGetComponent<T>(out var component))
                    {
                        UpdateGhostRepeatCount(component, count, directionType);
                    }
                }
            }
            else
            {
                UpdateGhostRepeatCount(instance, count, directionType);
            }
        }

        public void UpdateGhostRepeatCount<T>(T instance, int count, RepeatDirectionType directionType) where T : BaseBehaviour
        {
            var isPresent = activeSelectionRecorders.TryGetValue(instance, out var visualisers);
            if (isPresent)
            {
                bool wasRecording=false;
                for (int i = 0; i < visualisers.Count; i++)
                {
                    if(i==0)
                    {
                        wasRecording = visualisers[i].HasRecorder;
                    }
                    visualisers[i].Dispose();
                }
                activeSelectionRecorders.Remove(instance);
                ShowSelectionGhost_RepeatPosition(instance, count, true, directionType);
                if (wasRecording)
                    instance.GhostDescription.ToggleRecordMode?.Invoke();
            }
            else
            {
                ShowSelectionGhost_RepeatPosition(instance, count, true, directionType);
            }
        }

        public void UpdateTRS_Multiselect<T>(T instance) where T : BaseBehaviour
        {
            var selections = EditorOp.Resolve<SelectionHandler>().GetSelectedObjects();
            if (selections.Count > 1)
            {
                foreach (var selection in selections)
                {
                    if (selection.TryGetComponent<T>(out var component))
                    {
                        UpdateTRS(component);
                    }
                }
            }
            else
            {
                UpdateTRS(instance);
            }
        }

        private void UpdateTRS<T>(T instance) where T : BaseBehaviour
        {
            var isPresent = activeSelectionRecorders.TryGetValue(instance, out var visualisers);
            if (isPresent)
            {
                var alltrs = instance.GhostDescription.SelectionGhostsTRS?.Invoke();

                Vector3 point1, point2;
                point1 = alltrs[0];
                point2 = instance.transform.position;
                float distance = (point2 - point1).magnitude * 0.7f;
                Vector3 dir = (point1 - point2).normalized;

                for (int i = 0; i < visualisers.Count; i++)
                {
                    if (instance.GhostDescription.showGhostWithTravelLine)
                    {
                        visualisers[i].CreateTravelLine(distance, dir);
                    }
                    if (visualisers[i].recordFor == RecordVisualiser.Record.Position)
                        visualisers[i].UpdateTRS(alltrs[i]);
                    else
                        visualisers[i].UpdateTRS(alltrs);
                }
            }
        }

        private void HandleSelectionInstance<T>(T instance, RecordVisualiser.Record recorder, bool show, int count, bool isTrulySingleInstance, RepeatDirectionType directionType = RepeatDirectionType.SameDirection) where T : BaseBehaviour
        {
            var isPresent = activeSelectionRecorders.TryGetValue(instance, out var visualisers);
            if (isPresent)
            {
                if (!show)
                {
                    foreach (var v in visualisers)
                    {
                        v.Dispose();
                    }
                    activeSelectionRecorders.Remove(instance);
                    return;
                }
            }
            else
            {
                if (!show)
                    return;
                var trs = instance.GhostDescription.SelectionGhostsTRS?.Invoke();
                List<RecordVisualiser> recordVisulisers = new List<RecordVisualiser>();

                Vector3 point1, point2;
                point1 = trs[0];
                point2 = instance.GhostDescription.GhostTo.transform.position;
                float distance = (point2 - point1).magnitude * 0.7f;
                Vector3 dir = (point1 - point2).normalized;

                bool repeatforver = count == int.MaxValue;
                if (count > 10)
                    count = 10;

                for (int i = 0; i < count; i++)
                {
                    var contextTrs = (recorder == RecordVisualiser.Record.Position ?new Vector3[] { trs[i] } : trs);
                    var visualiser = new RecordVisualiser(instance.GhostDescription.GhostTo,
                    recorder,
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
                    },contextTrs);
                    recordVisulisers.Add(visualiser);

                    if (instance.GhostDescription.showGhostWithTravelLine)
                    {
                        if (i == 0 && directionType == RepeatDirectionType.PingPong)
                        {
                            visualiser.CreateTravelLine(distance, dir, directionType, false);
                            break;
                        }
                        visualiser.CreateTravelLine(distance, dir, directionType, repeatforver && i == 9);
                    }
                }
                activeSelectionRecorders.Add(instance, recordVisulisers);
            }
        }

        private void HandleInstance<T>(T instance) where T : BaseBehaviour
        {
            var isPresent = ToggleGhostMode(instance);
            if (isPresent) return;
            var trs = instance.GhostDescription.SelectionGhostsTRS?.Invoke();
            if (activeSelectionRecorders.TryGetValue(instance, out var visualiser))
            {
                visualiser[0].AttachRecorder((data)=>
                {
                    instance.GhostDescription.OnGhostInteracted?.Invoke(data);
                    UpdateTRS(instance);
                });
            }
            instance.GhostDescription.OnGhostModeToggled(true);
        }

        private void HandleInstance_NoGhostOnMultiselect<T>(T instance) where T : BaseBehaviour
        {
            var isPresent = ToggleGhostMode(instance);
            if (isPresent) return;
            var components = GetAllComponentsForSelected(instance, true);
            var trs = instance.GhostDescription.SelectionGhostsTRS?.Invoke();
            if (activeSelectionRecorders.TryGetValue(instance, out var visualiser))
            {
                if (visualiser.Count > 0)
                {
                    visualiser[0].AttachRecorder((data) =>
                    {
                        instance.GhostDescription.OnGhostInteracted?.Invoke(data);
                        foreach (var component in components)
                        {
                            component.GhostDescription.OnGhostInteracted?.Invoke(data);
                            UpdateTRS_Multiselect(instance);
                        }
                    });
                }
            }
            instance.GhostDescription.OnGhostModeToggled(true);
        }

        private bool ToggleGhostMode<T>(T instance) where T : BaseBehaviour
        {
            activeSelectionRecorders.TryGetValue(instance, out var visualiser);
            var isPresent = visualiser.Count > 0;
            if (isPresent)
            {
                if (visualiser[0].HasRecorder)
                {
                    visualiser[0].RemoveRecorder();
                    instance.GhostDescription.OnGhostModeToggled(false);
                }
                else
                    return false;
            }
            else
            {
                Debug.Log("Ghost was not created on selection");
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
                (lastValue, description.OnGhostInteracted,instance.gameObject),
                (recentValue, description.OnGhostInteracted,instance.gameObject),
                $"Vector3 modified: {description.GetRecentValue.Invoke()}",
                (data) =>
                {
                    var (value, action,GObject) = ((object, Action<object>,GameObject))data;
                    var comp = instance as BaseBehaviour;
                    if (comp == null)
                    {
                         comp = GObject.GetComponent(instance.GetType()) as BaseBehaviour;
                        if(comp)
                        {
                            action = comp.GhostDescription.OnGhostInteracted;
                        }
                    }
                    action?.Invoke(value);
                    UpdateTRS(comp);
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
                    instance = instances.Single(z => z.GhostDescription.Equals(y)),
                    value = y.GetLastValue.Invoke(),
                    action = y.OnGhostInteracted,
                    gObject = instances.Single(z => z.GhostDescription.Equals(y)).gameObject
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
                    instance = instances.Single(z => z.GhostDescription.Equals(y)),
                    value = y.GetRecentValue.Invoke(),
                    action = y.OnGhostInteracted,
                    gObject = instances.Single(z => z.GhostDescription.Equals(y)).gameObject
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
                        if (value.instance == null)
                        {
                            var comp = value.gObject.GetComponent(instances.First().GetType()) as BaseBehaviour;
                            if (comp)
                            {
                                comp.GhostDescription.OnGhostInteracted?.Invoke(value.value);
                                continue;
                            }

                        }
                        if (value.value == null)
                        {
                            continue;
                        }
                        value.action?.Invoke(value.value);

                    }
                    UpdateTRS_Multiselect(instances.First());
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
            public BaseBehaviour instance;
            public object value;
            public Action<object> action;
            public GameObject gObject;
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
        private Transform childGhostMesh;
        private GameObject ghostLine;
        private GameObject lastLine;
        private float lastArrowLength;
        public readonly Record recordFor;
        private bool showLastLine;
        RepeatDirectionType cachedDirectionType;
        public Transform GhostMeshTransform { get { return childGhostMesh; } }
        public bool HasRecorder => baseRecorder != null;

        public RecordVisualiser(GameObject gameObject, Record recordFor, Action onGhostDataModified, params Vector3[] trs)
        {
            this.recordFor = recordFor;
            this.onGhostDataModified = onGhostDataModified;
            ghostMaterial = EditorOp.Load<Material>(GHOST_MATERIAL_PATH);
            var ghostObj = EditorOp.Load<GameObject>(GHOST_RESOURCE_PATH);
            ghost = Object.Instantiate(ghostObj);
            ghost.name = string.Concat(ghost.name, "_", gameObject.name);
            RuntimeWrappers.DuplicateGameObject(gameObject, ghost.transform, Vector3.zero);
            RuntimeWrappers.ResolveTRS(ghost, null, trs);
            childGhostMesh = ghost.transform.GetChild(0);
            childGhostMesh.localPosition = Vector3.zero;
            childGhostMesh.localRotation = Quaternion.identity;
            childGhostMesh.localScale = gameObject.transform.lossyScale;
            Clean();
            ghost.SetActive(true);
        }

        public void RemoveRecorder()
        {
            EditorOp.Resolve<EditorSystem>().RequestIncognitoMode(false);
            EditorOp.Resolve<Recorder>().TrackGhost(false, ghost);
            if (baseRecorder)
                Object.Destroy(baseRecorder);
            onGhostDataModified?.Invoke();
        }

        public void AttachRecorder(Action<object> onRecordDataModified)
        {
            EditorOp.Resolve<EditorSystem>().RequestIncognitoMode(true);
            EditorOp.Resolve<Recorder>().TrackGhost(true, ghost);

            baseRecorder = recordFor switch
            {
                Record.Position => ghost.AddComponent<PositionRecorder>(),
                Record.Rotation => ghost.AddComponent<RotationRecorder>(),
                _ => throw new NotImplementedException()
            };
            baseRecorder.Init(onRecordDataModified);
        }

        public void CreateTravelLine(float length, Vector3 direction)
        {
            CreateTravelLine(length, direction, cachedDirectionType, showLastLine);
        }

        public void CreateTravelLine(float length, Vector3 direction, RepeatDirectionType directionType, bool showLastLine)
        {
            this.showLastLine = showLastLine;
            cachedDirectionType = directionType;
            MeshFilter mf;
            MeshRenderer mr;
            if ((ghostLine && lastArrowLength != length) || ghostLine == null)
            {
                GameObject.Destroy(ghostLine);
                GameObject.Destroy(lastLine);
                ghostLine = new GameObject("Arrow_Line");
                mf = ghostLine.AddComponent<MeshFilter>();
                mr = ghostLine.AddComponent<MeshRenderer>();
                mf.mesh = new VisulisationLineGenerator(ghostLine.transform, length, 0.1f, 0.4f, 0.3f, 36, directionType == RepeatDirectionType.PingPong).Mesh;
                ghostLine.transform.SetParent(ghost.transform);
                Vector3 localDirection = ghostLine.transform.InverseTransformDirection(direction);
                ghostLine.transform.localPosition = Vector3.zero - localDirection * length * 1.3f;
                mr.material = ghostMaterial;

                if (showLastLine)
                {
                    lastLine = RuntimeWrappers.DuplicateGameObject(ghostLine, ghost.transform, Vector3.zero);
                    lastLine.transform.localPosition = Vector3.zero;
                }
            }
            lastArrowLength = length;

            Quaternion rotation = Quaternion.FromToRotation(-Vector3.back, direction);
            ghostLine.transform.rotation = rotation;
            if (lastLine)
                lastLine.transform.rotation = rotation;
        }

        private void Clean()
        {
            var allChildren = Helper.GetChildren(ghost.transform, true);
            for (int i = 0; i < allChildren.Count; i++)
            {
                var child = allChildren[i];
                RuntimeWrappers.CleanBehaviour(child);
                CleanCollider(child);
                CleanMaterial(child);
            }
        }

        public void UpdateTRS(params Vector3[] trs)
        {
            RuntimeWrappers.ResolveTRS(ghost, null, trs);
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

        public void Dispose()
        {
            EditorOp.Resolve<Recorder>().TrackGhost(false, ghost);
            Object.Destroy(ghost);
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
