using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RTG;
using RuntimeInspectorNamespace;
using Terra.Studio;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using static RuntimeInspectorNamespace.RuntimeHierarchy;
using System.Collections.ObjectModel;
using EasyUI.Helpers;
using PlayShifu.Terra;
using EasyUI.Toast;
using UnityEngine.UI;
using System;

public class SelectionHandler : View
{
    public enum GizmoId
    {
        Move = 1,
        Rotate,
        Scale,
        Universal
    }

    [SerializeField] private ToastUI toastUI;
    private RuntimeHierarchy runtimeHierarchy;
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
    private Selectable nextSelecteField;
    private EditorSystem cachedEditorSystem;
    private PointerEventData pointerEventData;
    private float tabTrailTimer = 0;
    private const float TAB_TRAILTIME = 0.5f;
    public bool canUndo, canRedo;
    public delegate void SelectionChangedDelegate(List<GameObject> gm);
    public SelectionChangedDelegate SelectionChanged;
    public event Action<GizmoId> OnGizmoChanged;

    private void Awake()
    {
        EditorOp.Register(this);
        SystemOp.Resolve<SaveSystem>()?.RegisterToAutosaveChangeListener(true);
    }

    private void OnDestroy()
    {
        SystemOp.Resolve<SaveSystem>()?.RegisterToAutosaveChangeListener(false);
        EditorOp.Unregister(this);
    }

    private void OnHierarchySelectionChanged(ReadOnlyCollection<Transform> _allTransform)
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
                    OnSelectionChanged(current,  SelectOptions.FocusOnSelection);
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
                            UpdateSelectedInHirearchy();
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
                        UpdateSelectedInHirearchy();
                        runtimeHierarchy.OnSelectionChanged += OnHierarchySelectionChanged;
                    });
            }
        }

        _selectedObjects.Clear();
        foreach (Transform tr in _allTransform)
        {
            _selectedObjects.Add(tr.gameObject);
        }
        OnSelectionChanged();
    }

    public override void Init()
    {
        objectMoveGizmo = RTGizmosEngine.Get.CreateObjectMoveGizmo();
        objectRotationGizmo = RTGizmosEngine.Get.CreateObjectRotationGizmo();
        objectScaleGizmo = RTGizmosEngine.Get.CreateObjectScaleGizmo();
        objectUniversalGizmo = RTGizmosEngine.Get.CreateObjectUniversalGizmo();

        ResetAllHandles();

        objectMoveGizmo.SetTargetObjects(_selectedObjects);
        objectMoveGizmo.onPositionRefreshed += RefreshObjectTRS;
        objectRotationGizmo.SetTargetObjects(_selectedObjects);
        objectRotationGizmo.onRotationRefreshed += RefreshObjectTRS;
        objectScaleGizmo.SetTargetObjects(_selectedObjects);
        objectScaleGizmo.onScaleRefreshed += RefreshObjectTRS;
        objectUniversalGizmo.SetTargetObjects(_selectedObjects);

        _workGizmo = objectMoveGizmo;
        _workGizmoId = GizmoId.Move;

        runtimeHierarchy = EditorOp.Resolve<RuntimeHierarchy>();
        cachedEditorSystem = EditorOp.Resolve<EditorSystem>();
        runtimeHierarchy.OnSelectionChanged += OnHierarchySelectionChanged;
        pointerEventData = new PointerEventData(EventSystem.current);
        EditorOp.Resolve<IURCommand>().OnUndoStackAvailable += (isPresent) => { canUndo = isPresent; };
        EditorOp.Resolve<IURCommand>().OnRedoStackAvailable += (isPresent) => { canRedo = isPresent; };
        EditorOp.Resolve<EditorSystem>().OnIncognitoEnabled += (isEnabled) =>
        {
            canUndo = !isEnabled;
            canRedo = !isEnabled;
        };
    }

    private void RefreshObjectTRS(List<GameObject> objects)
    {
        for (int i = 0; i < objects.Count; i++)
        {
            var behaviours = objects[i].GetComponents<BaseBehaviour>();
            foreach (var b in behaviours)
            {
                b.GhostDescription.UpdateSlectionGhostTRS?.Invoke();
            }
        }
    }

    private void Update()
    {
        if (cachedEditorSystem &&
            cachedEditorSystem.IsIncognitoEnabled)
        {
            return;
        }
        TabToFocus();

        if (EventSystem.current == null ||
            EventSystem.current.currentSelectedGameObject != null)
        {
            return;
        }
        Scan();
        DuplicateObjects();
        DeleteObjects();

        if ((RTInput.IsKeyPressed(KeyCode.LeftCommand) || RTInput.IsKeyPressed(KeyCode.LeftControl)) && RTInput.WasKeyPressedThisFrame(KeyCode.S))
        {
            EditorOp.Resolve<SceneDataHandler>().Save();
        }

        if (!Application.isEditor)
        {
            CheckForSave();
        }
    }

    private void TabToFocus()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (pointerEventData.selectedObject == null)
            {
                EditorOp.Resolve<FocusFieldsSystem>().RemoveCurrentSelected();
            }
        }

        if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyUp(KeyCode.Tab))
        {
            tabTrailTimer = 0.0f;
        }


        if (Input.GetKeyDown(KeyCode.Tab) && tabTrailTimer <= 0.01f || Input.GetKey(KeyCode.Tab) && tabTrailTimer > TAB_TRAILTIME)
        {
            if (tabTrailTimer > TAB_TRAILTIME)
                tabTrailTimer = TAB_TRAILTIME * 0.8f;

            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                nextSelecteField = EditorOp.Resolve<FocusFieldsSystem>().LastFocusedGameObject?.GetComponent<Selectable>();
            }
            else
            {
                nextSelecteField = EditorOp.Resolve<FocusFieldsSystem>().NextFocusedGameObject?.GetComponent<Selectable>();
            }

            if (nextSelecteField != null)
            {
                EditorOp.Resolve<FocusFieldsSystem>().SelectFocusedGameObject(nextSelecteField.gameObject);
            }
        }
        if (Input.GetKey(KeyCode.Tab))
        {
            tabTrailTimer += Time.deltaTime;
        }
    }

    private void CheckForSave()
    {
        if (canUndo)
        {
            if ((RTInput.IsKeyPressed(KeyCode.LeftCommand) || RTInput.IsKeyPressed(KeyCode.LeftControl)) && RTInput.WasKeyPressedThisFrame(KeyCode.Z))
            {
                EditorOp.Resolve<IURCommand>().Undo();
            }
        }

        if (canRedo)
        {
            if ((RTInput.IsKeyPressed(KeyCode.LeftCommand) || RTInput.IsKeyPressed(KeyCode.LeftControl)) && RTInput.WasKeyPressedThisFrame(KeyCode.Y))
            {
                EditorOp.Resolve<IURCommand>().Redo();
            }
        }
    }

    private void DeleteObjects()
    {
        if (_selectedObjects.Count > 0)
        {
            //if (RTInput.IsKeyPressed(KeyCode.LeftCommand))
            //{
                if (RTInput.WasKeyPressedThisFrame(KeyCode.Backspace) || RTInput.WasKeyPressedThisFrame(KeyCode.Delete))
                {
                    var selections = new List<GameObject>(_selectedObjects);
                    runtimeHierarchy.Deselect();
                    Snapshots.DeleteGameObjectsSnapshot.CreateSnapshot(selections.ToList());
                    foreach (GameObject obj in selections)
                    {
                        var comps = obj.GetComponentsInChildren<BaseBehaviour>();
                        var canBeDeleted = Rulesets.CanBeDeleted(comps);
                        if (canBeDeleted)
                        {
                            obj.SetActive(false);
                        }
                    }
                    _selectedObjects.Clear();
                    OnSelectionChanged();
                }
            //}
        }
    }

    private void DuplicateObjects(bool a_bByPassShortCut = false)
    {
        if (_selectedObjects.Count > 0)
        {
            if (((RTInput.IsKeyPressed(KeyCode.LeftCommand) || RTInput.IsKeyPressed(KeyCode.LeftControl)) && RTInput.WasKeyPressedThisFrame(KeyCode.D)) || a_bByPassShortCut)
            {
                var duplicatedGms = new List<Transform>();
                foreach (GameObject obj in _selectedObjects)
                {
                    var components = obj.GetComponentsInChildren<BaseBehaviour>();
                    if (components.Any(x => Rulesets.DUPLICATE_IGNORABLES.Contains(x.GetType())))
                    {
                        continue;
                    }
                    var iObj = Instantiate(obj, obj.transform.position, obj.transform.rotation, obj.transform.parent);
                    duplicatedGms.Add(iObj.transform);
                }

                runtimeHierarchy.Refresh();
                _selectedObjects.Clear();

                foreach (var d in duplicatedGms)
                {
                    OnSelectionChanged();
                }

                Snapshots.SpawnGameObjectsSnapshot.CreateSnapshot(duplicatedGms.Select(x => x.gameObject).ToList());
                SelectObjectsInHierarchy(duplicatedGms);
                var color = Helper.GetColorFromHex("#0F1115");
                color.a = 0.8f;
                Toast.Show("Duplicated Selected Object" + (duplicatedGms.Count > 1 ? "s" : ""), 1.0f, color);
            }
        }
    }

    private bool CheckIfThereIsAnyPopups()
    {
        if (EditorOp.Resolve<ObjectReferencePicker>())
            return true;

        return false;
    }

    private void Scan()
    {
        if (Input.GetMouseButtonDown(0) &&
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
                if (RTInput.IsKeyPressed(KeyCode.LeftCommand) || RTInput.IsKeyPressed(KeyCode.LeftControl))
                {
                    if (_selectedObjects.Contains(pickedObject))
                        _selectedObjects.Remove(pickedObject);
                    else
                        _selectedObjects.Add(pickedObject);
                }
                else
                {
                    _selectedObjects.Clear();
                    _selectedObjects.Add(pickedObject);
                }

            }
            else
            {
                _selectedObjects.Clear();
            }
            UpdateSelectedInHirearchy();
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

    public void UpdateSelectedInHirearchy()
    {
        List<Transform> transforms = new List<Transform>();
        foreach (var s in _selectedObjects)
        {
            transforms.Add(s.transform);
        }
        runtimeHierarchy.Select(transforms, SelectOptions.FocusOnSelection);
    }
    public void SelectObjectsInHierarchy(List<Transform> _obj)
    {
        runtimeHierarchy.Select(_obj, SelectOptions.FocusOnSelection);
    }

    public void RefreshHierarchy()
    {
        runtimeHierarchy.Refresh();
    }

    public void SetWorkGizmoId(GizmoId gizmoId, bool shouldRefreshTRSOfGizmo = true)
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
            if (shouldRefreshTRSOfGizmo)
            {
                _workGizmo.SetTargetObjects(GetSelectedObjects());
                _workGizmo.RefreshPositionAndRotation();
            }
        }
        OnGizmoChanged?.Invoke(gizmoId);
    }

    private void OnSelectionChanged(GameObject sObject = null, SelectOptions selectOption = SelectOptions.FocusOnSelection)
    {
        for (int i = 0; i < prevSelectedObjects.Count; i++)
        {
            if (prevSelectedObjects[i] != null)
            {
                EditorUtils.ApplyLayerToChildren(prevSelectedObjects[i].transform, "Default");
            }

            var behaviours = prevSelectedObjects[i].GetComponents<BaseBehaviour>();
            foreach (var b in behaviours)
            {
                b.GhostDescription.HideSelectionGhost?.Invoke();
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
                EditorUtils.ApplyLayerToChildren(sObject.transform, "Default");
            }
            else
            {
                _selectedObjects.Add(sObject);
                runtimeHierarchy.Select(sObject.transform, selectOption);
            }
        }

        for (int i = 0; i < _selectedObjects.Count; i++)
        {
            if (_selectedObjects[i] != null && _selectedObjects[i].layer == LayerMask.NameToLayer("Outline"))
            {
                EditorUtils.ApplyLayerToChildren(_selectedObjects[i].transform, "Default");
                _selectedObjects[i].layer = LayerMask.NameToLayer("Default");
            }
            else
            {
                EditorUtils.ApplyLayerToChildren(_selectedObjects[i].transform, "Outline");
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
            DisableGizmo();
        }

        SelectionChanged?.Invoke(_selectedObjects);
    }

    private void DisableGizmo()
    {
        _workGizmo.Gizmo.SetEnabled(false);
        objectMoveGizmo.Gizmo.SetEnabled(false);
        objectRotationGizmo.Gizmo.SetEnabled(false);
        objectScaleGizmo.Gizmo.SetEnabled(false);
        objectUniversalGizmo.Gizmo.SetEnabled(false);
    }

    public void DeselectAll()
    {
        runtimeHierarchy.Refresh();
        prevSelectedObjects = _selectedObjects.ToList();
        _selectedObjects.Clear();
        OnSelectionChanged();
    }

    public void OverrideGizmoOntoTarget(List<GameObject> targets, GizmoId gizmoId)
    {
        DisableGizmo();
        SetWorkGizmoId(gizmoId, false);
        _workGizmo.SetTargetObjects(targets);
        _workGizmo.Gizmo.SetEnabled(true);
        _workGizmo.RefreshPositionAndRotation();
    }

    public void ToggleGizmo(bool enable)
    {
        _workGizmo.Gizmo.SetEnabled(enable);
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

    public bool IsSelected(GameObject go)
    {
        var result = _selectedObjects.Any(x => x == go);
        return result;
    }

    public void Select(GameObject toSelect)
    {
        OnSelectionChanged(toSelect, SelectOptions.FocusOnSelection);
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

    public override void Draw()
    {

    }

    public override void Flush()
    {

    }

    public override void Repaint()
    {

    }

    public void DuplicateCurrentSelectedObject()
    {
        DuplicateObjects(true);
    }
}
