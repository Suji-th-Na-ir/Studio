using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RTG;
using RuntimeInspectorNamespace;
using Terra.Studio;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Reflection;
using System.Collections.ObjectModel;
using PlayShifu.Terra;
using EasyUI.Helpers;
using EasyUI.Toast;

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
    public ToastUI toastUI;
    
    private ObjectTransformGizmo objectMoveGizmo;
    private ObjectTransformGizmo objectRotationGizmo;
    private ObjectTransformGizmo objectScaleGizmo;
    private ObjectTransformGizmo objectUniversalGizmo;

    private GizmoId _workGizmoId;
    private ObjectTransformGizmo _workGizmo;
    private List<GameObject> _selectedObjects = new List<GameObject>();
    private List<GameObject> prevSelectedObjects = new List<GameObject>();
    private GameObject lastPickedGameObject;

    public  delegate void SelectionChangedDelegate(List<GameObject> gm);
    public SelectionChangedDelegate SelectionChanged;
    private void Awake()
    {
        // setting target framerate to 60
        Application.targetFrameRate = 60;
        EditorOp.Register(this);
        runtimeHierarchy.OnSelectionChanged += OnHierarchySelectionChanged;
    }

    private void OnDestroy()
    {
        EditorOp.Unregister(this);
        runtimeHierarchy.OnSelectionChanged -= OnHierarchySelectionChanged;
    }

    private void OnHierarchySelectionChanged(ReadOnlyCollection<Transform> _allTransform)
    {
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

        if((RTInput.IsKeyPressed(KeyCode.LeftCommand) || RTInput.IsKeyPressed(KeyCode.LeftControl)) && RTInput.WasKeyPressedThisFrame(KeyCode.S))
        {
            EditorOp.Resolve<SceneDataHandler>().Save();
        }


        if ((RTInput.IsKeyPressed(KeyCode.LeftCommand) || RTInput.IsKeyPressed(KeyCode.LeftControl)) && RTInput.WasKeyPressedThisFrame(KeyCode.Z))
        {
            var color = Helper.GetColorFromHex("#0F1115");
            color.a = 0.8f;
            Toast.Show("Undo Feature Will Come Soon!", 1.0f, color);
        }


        if ((RTInput.IsKeyPressed(KeyCode.LeftCommand) || RTInput.IsKeyPressed(KeyCode.LeftControl)) && RTInput.IsKeyPressed(KeyCode.LeftShift) && RTInput.WasKeyPressedThisFrame(KeyCode.Z))
        {
            var color = Helper.GetColorFromHex("#0F1115");
            color.a = 0.8f;
            Toast.Show("Redo Feature Will Come Soon!", 1.0f, color);
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
             
                List<Transform> duplicatedGms = new List<Transform>();
                foreach (GameObject obj in _selectedObjects)
                {
                    var iObj = Instantiate(obj, obj.transform.position, obj.transform.rotation, obj.transform.parent);                
                    duplicatedGms.Add(iObj.transform);
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
                runtimeHierarchy.Refresh();
                _selectedObjects.Clear();
                foreach (var d in duplicatedGms)
                {
                    OnSelectionChanged();
                }
                SelectObjectsInHierarchy(duplicatedGms);
            }
        }
    }



    bool CheckIfThereIsAnyPopups()
    {
        if (GameObject.FindObjectOfType<ObjectReferencePicker>() != null)
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
                    
                    runtimeHierarchy.Select(pickedObject.transform, RuntimeHierarchy.SelectOptions.Additive);
                    OnSelectionChanged();
                }
                else
                {
                    runtimeHierarchy.Select(pickedObject.transform, RuntimeHierarchy.SelectOptions.FocusOnSelection);
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
        // Build a ray using the current mouse cursor position
        Ray ray = Camera.main.ScreenPointToRay(RTInput.MousePosition);
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(RTInput.MousePosition);
        
        PhysicsScene pScene = SceneManager.GetActiveScene().GetPhysicsScene();
        if (pScene.Raycast(worldPoint, ray.direction, out var hit, float.MaxValue))
        {
            var hitObject = hit.collider.gameObject;
            var pickedObject = hitObject;
            if (lastPickedGameObject != hitObject)
            {
                if (hitObject.transform.root != null && lastPickedGameObject!= hitObject.transform.root.gameObject)
                    pickedObject = hitObject.transform.root.gameObject;

            }
            lastPickedGameObject = pickedObject;
            return pickedObject;
        }

        return null;
    }

    public void SelectObjectInHierarchy(GameObject _obj)
    {
        runtimeHierarchy.Select(_obj.transform, RuntimeHierarchy.SelectOptions.FocusOnSelection);
    }
    public void SelectObjectsInHierarchy(List<Transform> _obj)
    {
        runtimeHierarchy.Select(_obj, RuntimeHierarchy.SelectOptions.FocusOnSelection);
    }

    public void RefreshHierarchy()
    {
        runtimeHierarchy.Refresh();
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

    public void OnSelectionChanged(GameObject sObject = null)
    {
        for (int i = 0; i < prevSelectedObjects.Count; i++)
        {
            if (prevSelectedObjects[i]!=null && prevSelectedObjects[i].GetComponent<Outline>())
                prevSelectedObjects[i].GetComponent<Outline>().enabled = false;
        }
            if (_selectedObjects.Count > 0)
            prevSelectedObjects = _selectedObjects.ToList();

        for (int i = 0; i < _selectedObjects.Count; i++)
        {
            if (_selectedObjects[i] != null && _selectedObjects[i]?.GetComponent<Outline>() != null)
            {
                _selectedObjects[i].GetComponent<Outline>().enabled = true;
            }
            else
            {
                var comp = _selectedObjects[i]?.AddComponent<Outline>();
                comp.OutlineWidth = 5f;
                comp.OutlineColor = Color.yellow;
                comp.enabled = true;
            }
        }
        
        if (sObject != null)
        {
            if (_selectedObjects.Contains(sObject)) _selectedObjects.Remove(sObject);
            else _selectedObjects.Add(sObject);
        }

        if (_selectedObjects.Count != 0)
        {
            _workGizmo.Gizmo.SetEnabled(true);
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

    public List<GameObject> GetPrevSelectedObjects()
    {
        return prevSelectedObjects;
    }
    public List<GameObject> GetSelectedObjects()
    {
        return _selectedObjects;
    }
}
