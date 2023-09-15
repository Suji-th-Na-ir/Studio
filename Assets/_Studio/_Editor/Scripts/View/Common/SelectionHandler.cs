using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RTG;
using RuntimeInspectorNamespace;
using Terra.Studio;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Reflection;
using static RuntimeInspectorNamespace.RuntimeHierarchy;

public class SelectionHandler : View
{
    public enum GizmoId
    {
        Move = 1,
        Rotate,
        Scale,
        Universal
    }

    [SerializeField] private RuntimeHierarchy runtimeHierarchy;

    private ObjectTransformGizmo objectMoveGizmo;
    private ObjectTransformGizmo objectRotationGizmo;
    private ObjectTransformGizmo objectScaleGizmo;
    private ObjectTransformGizmo objectUniversalGizmo;

    private GizmoId _workGizmoId;
    private ObjectTransformGizmo _workGizmo;
    private List<GameObject> _selectedObjects = new List<GameObject>();
    private List<GameObject> prevSelectedObjects = new List<GameObject>();
    private GameObject lastPickedGameObject;
    private Camera mainCamera;

    public delegate void SelectionChangedDelegate(List<GameObject> gm);
    public SelectionChangedDelegate SelectionChanged;

    private void Awake()
    {
        EditorOp.Register(this);
        runtimeHierarchy.OnSelectionChanged += OnHierarchySelectionChanged;
    }

    private void OnDestroy()
    {
        EditorOp.Unregister(this);
        runtimeHierarchy.OnSelectionChanged -= OnHierarchySelectionChanged;
    }

    private void OnHierarchySelectionChanged(System.Collections.ObjectModel.ReadOnlyCollection<Transform> _allTransform)
    {
        if (_allTransform.Count == 0)
        {
            EditorOp.Resolve<IURCommand>().Record(
                (prevSelectedObjects.ToList(), true),
                (new List<GameObject>(), false),
                $"Deselected all",
                (tuple) =>
                {
                    runtimeHierarchy.OnSelectionChanged -= OnHierarchySelectionChanged;
                    var (stack, isUndo) = ((List<GameObject>, bool))tuple;
                    if (isUndo)
                    {
                        var option = stack.Count == 1 ? SelectOptions.FocusOnSelection : SelectOptions.Additive;
                        for (int i = 0; i < stack.Count; i++)
                        {
                            OnSelectionChanged(stack[i], option);
                        }
                    }
                    else
                    {
                        DeselectAll();
                    }
                    runtimeHierarchy.OnSelectionChanged += OnHierarchySelectionChanged;
                });
        }
        else if (_allTransform.Count == 1)
        {
            var prevSelected = prevSelectedObjects.Count == 0 ? null : prevSelectedObjects[^1];
            var selected = _allTransform[0].gameObject;
            EditorOp.Resolve<IURCommand>().Record(
                (prevSelected, selected),
                (prevSelected, selected),
                $"Selected {selected.name}",
                (tuple) =>
                {
                    runtimeHierarchy.OnSelectionChanged -= OnHierarchySelectionChanged;
                    var (prev, current) = ((GameObject, GameObject))tuple;
                    var isPrevAvailable = prev != null;
                    OnSelectionChanged(current, SelectOptions.FocusOnSelection);
                    if (isPrevAvailable)
                    {
                        OnSelectionChanged(prev, SelectOptions.FocusOnSelection);
                    }
                    runtimeHierarchy.OnSelectionChanged += OnHierarchySelectionChanged;
                });
        }
        else
        {
            var diff = _allTransform.Count - prevSelectedObjects.Count;
            if (diff == 1)
            {
                var selected = _allTransform[^1].gameObject;
                EditorOp.Resolve<IURCommand>().Record(
                    selected,
                    selected,
                    $"Multiselected {selected.name}",
                    (obj) =>
                    {
                        runtimeHierarchy.OnSelectionChanged -= OnHierarchySelectionChanged;
                        var go = (GameObject)obj;
                        var isUndo = _selectedObjects.Contains(go);
                        OnSelectionChanged(go, SelectOptions.Additive);
                        if (isUndo)
                        {
                            runtimeHierarchy.Deselect();
                            runtimeHierarchy.Select(_selectedObjects.Select(y => y.transform).ToList(), SelectOptions.Additive);
                        }
                        runtimeHierarchy.OnSelectionChanged += OnHierarchySelectionChanged;
                    });
            }
            else
            {
                EditorOp.Resolve<IURCommand>().Record(
                    prevSelectedObjects.ToList(),
                    _allTransform.Select(x => x.gameObject).ToList(),
                    $"Multiselected {Mathf.Abs(diff)} objects",
                    (obj) =>
                    {
                        runtimeHierarchy.OnSelectionChanged -= OnHierarchySelectionChanged;
                        prevSelectedObjects = _selectedObjects.ToList();
                        _selectedObjects.Clear();
                        var newList = (List<GameObject>)obj;
                        for (int i = 0; i < newList.Count; i++)
                        {
                            _selectedObjects.Add(newList[i]);
                        }
                        OnSelectionChanged();
                        runtimeHierarchy.Deselect();
                        runtimeHierarchy.Select(_selectedObjects.Select(y => y.transform).ToList(), SelectOptions.Additive);
                        runtimeHierarchy.OnSelectionChanged += OnHierarchySelectionChanged;
                    });
            }
        }
        if (_allTransform.Count > 0)
        {
            _selectedObjects.Clear();
            foreach (Transform tr in _allTransform)
            {
                _selectedObjects.Add(tr.gameObject);
            }
            OnSelectionChanged();
        }
    }

    public override void Init()
    {
        objectMoveGizmo = RTGizmosEngine.Get.CreateObjectMoveGizmo();
        objectRotationGizmo = RTGizmosEngine.Get.CreateObjectRotationGizmo();
        objectScaleGizmo = RTGizmosEngine.Get.CreateObjectScaleGizmo();
        objectUniversalGizmo = RTGizmosEngine.Get.CreateObjectUniversalGizmo();

        ResetAllHandles();

        objectMoveGizmo.SetTargetObjects(_selectedObjects);
        objectRotationGizmo.SetTargetObjects(_selectedObjects);
        objectScaleGizmo.SetTargetObjects(_selectedObjects);
        objectUniversalGizmo.SetTargetObjects(_selectedObjects);

        _workGizmo = objectMoveGizmo;
        _workGizmoId = GizmoId.Move;
    }

    public override void Draw()
    {

    }

    public override void Flush()
    {

    }

    public override void Repaint()
    {

    }

    private void Update()
    {
        if (EventSystem.current.currentSelectedGameObject != null)
            return;
        Scan();
        DuplicateObjects();
        DeleteObjects();

        if (RTInput.IsKeyPressed(KeyCode.LeftCommand) && RTInput.WasKeyPressedThisFrame(KeyCode.S))
        {
            EditorOp.Resolve<SceneDataHandler>().Save();
        }
    }

    private void DeleteObjects()
    {
        if (_selectedObjects.Count > 0)
        {
            if (RTInput.IsKeyPressed(KeyCode.LeftCommand))
            {
                if (RTInput.WasKeyPressedThisFrame(KeyCode.Backspace))
                {
                    runtimeHierarchy.Deselect();
                    foreach (GameObject obj in _selectedObjects)
                    {
                        var comps = obj.GetComponents<IComponent>();
                        foreach (var comp in comps)
                        {
                            EditorOp.Resolve<UILogicDisplayProcessor>().RemoveComponentIcon(new ComponentDisplayDock() { componentGameObject = obj, componentType = comp.GetType().Name });
                        }
                        Destroy(obj);
                    }
                    _selectedObjects.Clear();
                    OnSelectionChanged();
                }
            }
        }
    }

    private void DuplicateObjects()
    {
        if (_selectedObjects.Count > 0)
        {
            if (RTInput.IsKeyPressed(KeyCode.LeftCommand) && RTInput.WasKeyPressedThisFrame(KeyCode.D))
            {

                foreach (GameObject obj in _selectedObjects)
                {
                    var iObj = Instantiate(obj, obj.transform.position, obj.transform.rotation);
                    var components = iObj.GetComponents<IComponent>();

                    for (int i = 0; i < components.Length; i++)
                    {
                        var componentType = components[i].GetType();
                        EditorOp.Resolve<UILogicDisplayProcessor>().AddComponentIcon(new ComponentDisplayDock
                        { componentGameObject = iObj, componentType = componentType.Name });

                        var mInfo = componentType.GetField("Broadcast", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                        if (mInfo != null)
                        {
                            var oldValue = mInfo?.GetValue(components[i]);
                            EditorOp.Resolve<UILogicDisplayProcessor>().UpdateBroadcastString(oldValue.ToString(), ""
                                , new ComponentDisplayDock() { componentGameObject = iObj, componentType = componentType.Name });
                        }

                        var mInfo1 = componentType.GetField("BroadcastListen", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                        if (mInfo1 != null)
                        {
                            var oldValue1 = mInfo1?.GetValue(components[i]);
                            EditorOp.Resolve<UILogicDisplayProcessor>().UpdateListenerString(oldValue1.ToString(), "",
                             new ComponentDisplayDock() { componentGameObject = iObj, componentType = componentType.Name });
                        }

                    }
                }

            }
        }
    }

    bool CheckIfThereIsAnyPopups()
    {
        if (EditorOp.Resolve<ObjectReferencePicker>())
            return true;

        return false;
    }

    private void Scan()
    {
        if (RTInput.WasLeftMouseButtonPressedThisFrame() &&
            RTGizmosEngine.Get.HoveredGizmo == null)
        {
            if (CheckIfThereIsAnyPopups()) return;

            if (EventSystem.current.IsPointerOverGameObject())
            {
                return; // Return early if the mouse is over UI
            }
            // Pick a game object
            GameObject pickedObject = PickGameObject();

            if (pickedObject != null)
            {
                if (RTInput.IsKeyPressed(KeyCode.LeftCommand))
                {
                    if (_selectedObjects.Contains(pickedObject))
                        _selectedObjects.Remove(pickedObject);
                    else
                        _selectedObjects.Add(pickedObject);

                    runtimeHierarchy.Select(pickedObject.transform, SelectOptions.Additive);
                    OnSelectionChanged();
                }
                else
                {
                    runtimeHierarchy.Select(pickedObject.transform, SelectOptions.FocusOnSelection);
                    _selectedObjects.Clear();
                    _selectedObjects.Add(pickedObject);
                    OnSelectionChanged();
                }
            }
            else
            {
                _selectedObjects.Clear();
                OnSelectionChanged();
            }
        }
        if (RTInput.WasKeyPressedThisFrame(KeyCode.W)) SetWorkGizmoId(GizmoId.Move);
        else if (RTInput.WasKeyPressedThisFrame(KeyCode.E)) SetWorkGizmoId(GizmoId.Rotate);
        else if (RTInput.WasKeyPressedThisFrame(KeyCode.R)) SetWorkGizmoId(GizmoId.Scale);
        else if (RTInput.WasKeyPressedThisFrame(KeyCode.T)) SetWorkGizmoId(GizmoId.Universal);
    }

    private void ResetAllHandles()
    {
        objectMoveGizmo.Gizmo.SetEnabled(false);
        objectRotationGizmo.Gizmo.SetEnabled(false);
        objectScaleGizmo.Gizmo.SetEnabled(false);
        objectUniversalGizmo.Gizmo.SetEnabled(false);
    }

    private GameObject PickGameObject()
    {
        var ray = GetCamera().ScreenPointToRay(RTInput.MousePosition);
        var worldPoint = GetCamera().ScreenToWorldPoint(RTInput.MousePosition);
        var pScene = SceneManager.GetActiveScene().GetPhysicsScene();
        if (pScene.Raycast(worldPoint, ray.direction, out var hit, 1000f))
        {
            var hitObject = hit.collider.gameObject;
            var pickedObject = hitObject;
            if (lastPickedGameObject != hitObject)
            {
                if (hitObject.transform.root != null && lastPickedGameObject != hitObject.transform.root.gameObject)
                    pickedObject = hitObject.transform.root.gameObject;

            }
            lastPickedGameObject = pickedObject;
            return pickedObject;
        }

        return null;
    }

    private Camera GetCamera()
    {
        if (!mainCamera)
        {
            mainCamera = Camera.main;
        }
        return mainCamera;
    }

    public void SetWorkGizmoId(GizmoId gizmoId)
    {
        // If the specified gizmo id is the same as the current id, there is nothing left to do
        if (gizmoId == _workGizmoId) return;

        // Start with a clean slate and disable all gizmos
        ResetAllHandles();

        _workGizmoId = gizmoId;
        if (gizmoId == GizmoId.Move) _workGizmo = objectMoveGizmo;
        else if (gizmoId == GizmoId.Rotate) _workGizmo = objectRotationGizmo;
        else if (gizmoId == GizmoId.Scale) _workGizmo = objectScaleGizmo;
        else if (gizmoId == GizmoId.Universal) _workGizmo = objectUniversalGizmo;

        if (_selectedObjects.Count != 0)
        {
            _workGizmo.Gizmo.SetEnabled(true);
            _workGizmo.RefreshPositionAndRotation();
        }
    }

    public void OnSelectionChanged(GameObject sObject = null, SelectOptions selectOption = SelectOptions.FocusOnSelection)
    {
        for (int i = 0; i < prevSelectedObjects.Count; i++)
        {
            if (prevSelectedObjects[i] != null && prevSelectedObjects[i].TryGetComponent(out Outline outline))
            {
                outline.enabled = false;
            }
        }

        if (_selectedObjects.Count > 0)
        {
            prevSelectedObjects = _selectedObjects.ToList();
        }

        if (sObject != null)
        {
            if (_selectedObjects.Contains(sObject))
            {
                _selectedObjects.Remove(sObject);
                if (sObject.TryGetComponent(out Outline outline))
                {
                    outline.enabled = false;
                }
            }
            else
            {
                _selectedObjects.Add(sObject);
                runtimeHierarchy.Select(sObject.transform, selectOption);
            }
        }

        for (int i = 0; i < _selectedObjects.Count; i++)
        {
            if (_selectedObjects[i] != null && _selectedObjects[i].TryGetComponent(out Outline outline))
            {
                outline.enabled = true;
            }
            else
            {
                outline = _selectedObjects[i].AddComponent<Outline>();
                outline.OutlineWidth = 5f;
                outline.OutlineColor = Color.yellow;
                outline.enabled = true;
            }
        }

        if (_selectedObjects.Count != 0)
        {
            _workGizmo.Gizmo.SetEnabled(true);
            _workGizmo.SetTargetObjects(_selectedObjects);
            _workGizmo.RefreshPositionAndRotation();
        }
        else
        {
            runtimeHierarchy.Deselect();
            _workGizmo.Gizmo.SetEnabled(false);
            objectMoveGizmo.Gizmo.SetEnabled(false);
            objectRotationGizmo.Gizmo.SetEnabled(false);
            objectScaleGizmo.Gizmo.SetEnabled(false);
            objectUniversalGizmo.Gizmo.SetEnabled(false);
        }

        SelectionChanged?.Invoke(_selectedObjects);
    }

    public void DeselectAll()
    {
        runtimeHierarchy.Refresh();
        prevSelectedObjects = _selectedObjects.ToList();
        _selectedObjects.Clear();
        OnSelectionChanged();
    }

    public void RefreshGizmo()
    {
        _workGizmo.RefreshPositionAndRotation();
    }

    public List<GameObject> GetPrevSelectedObjects()
    {
        return prevSelectedObjects;
    }
    public List<GameObject> GetSelectedObjects()
    {
        return _selectedObjects;
    }

    public void ListenForHierarchyChanges(bool listen)
    {
        if (listen)
        {
            runtimeHierarchy.OnSelectionChanged += OnHierarchySelectionChanged;
        }
        else
        {
            runtimeHierarchy.OnSelectionChanged -= OnHierarchySelectionChanged;
        }
    }
}
