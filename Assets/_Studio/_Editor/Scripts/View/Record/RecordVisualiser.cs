using System;
using UnityEngine;
using PlayShifu.Terra;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace Terra.Studio
{
    public class Recorder
    {
        private Dictionary<BaseBehaviour, RecordVisualiser> activeRecorders = new();

        public void TrackPosition<T>(T instance, bool enableModification, params Vector3[] trs) where T : BaseBehaviour
        {
            var isPresent = ToggleGhostMode(instance);
            if (isPresent) return;
            var visualiser = new RecordVisualiser(instance.gameObject, RecordVisualiser.Record.Position, instance.OnGhostInteracted, enableModification, trs);
            activeRecorders[instance] = visualiser;
        }

        private bool ToggleGhostMode<T>(T instance) where T : BaseBehaviour
        {
            var isPresent = activeRecorders.TryGetValue(instance, out var visualiser);
            if (isPresent)
            {
                visualiser.Dispose();
                activeRecorders.Remove(instance);
            }
            else
            {
                activeRecorders.Add(instance, null);
            }
            return isPresent;
        }
    }

    public class RecordVisualiser : IDisposable
    {
        public enum Record
        {
            Position
        }

        private const string GHOST_RESOURCE_PATH = "Prefabs/Ghost";
        private const string GHOST_MATERIAL_PATH = "Materials/Ghost";
        private readonly GameObject ghost;
        private readonly GameObject originalObject;
        private readonly Material ghostMaterial;
        private BaseRecorder baseRecorder;

        public RecordVisualiser(GameObject gameObject, Record recordFor, Action<object> onRecordDataModified, bool enableModification, params Vector3[] trs)
        {
            originalObject = gameObject;
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
                EditorOp.Resolve<SelectionHandler>().OverrideGizmoOntoTarget(ghost);
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
        }

        private void AttachRecorder(Record recordFor, Action<object> onRecordDataModified)
        {
            var firstChild = ghost.transform.GetChild(0).gameObject;
            baseRecorder = recordFor switch
            {
                _ => firstChild.AddComponent<PositionRecorder>(),
            };
            baseRecorder.Init(onRecordDataModified);
        }

        public void Dispose()
        {
            Object.Destroy(ghost);
            EditorOp.Resolve<EditorSystem>().RequestIncognitoMode(false);
            EditorOp.Resolve<SelectionHandler>().OverrideGizmoOntoTarget(originalObject);
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

            public bool IsDataModified()
            {
                var didPositionChange = cachedPosition != transform.position;
                cachedPosition = transform.position;
                return didPositionChange;
            }
        }
    }
}