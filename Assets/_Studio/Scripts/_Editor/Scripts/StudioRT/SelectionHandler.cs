using UnityEngine;
using RTG;
using RuntimeInspectorNamespace;
using RuntimeCommon;
using Terra.Studio;
using StudioRT;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;

public class SelectionHandler : MonoBehaviour
{
    private enum GizmoId
    {
        Move = 1,
        Rotate,
        Scale,
        Universal
    }

    private static SelectionHandler _instance;

    public static SelectionHandler Instance
    {
        get
        {
            if (_instance == null)
                _instance = new SelectionHandler();
            return _instance;
        }
    }

    [SerializeField] private Camera mainCamera;
    [SerializeField] private RuntimeHierarchy runtimeHierarchy;
    [SerializeField] private RuntimeInspector runtimeInspector;
    [SerializeField] private GameObject cubeObject;


    private ObjectTransformGizmo objectMoveGizmo;
    private ObjectTransformGizmo objectRotationGizmo;
    private ObjectTransformGizmo objectScaleGizmo;
    private ObjectTransformGizmo objectUniversalGizmo;

    private GizmoId _workGizmoId;
    private ObjectTransformGizmo _workGizmo;
    private GameObject _targetObject;

    private void Awake()
    {
        runtimeHierarchy.OnSelectionChanged += OnHierarchySelectionChanged;
    }

    private void OnHierarchySelectionChanged(System.Collections.ObjectModel.ReadOnlyCollection<Transform> _allTransform)
    {
        if (_allTransform.Count > 0)
        {
            if (_allTransform[0].gameObject != _targetObject)
            {
                OnTargetObjectChanged(_allTransform[0].gameObject);
                SelectObjectInHierarchy(_allTransform[0].gameObject);
            }
        }
    }

    private void Start()
    {
        objectMoveGizmo = RTGizmosEngine.Get.CreateObjectMoveGizmo();
        objectRotationGizmo = RTGizmosEngine.Get.CreateObjectRotationGizmo();
        objectScaleGizmo = RTGizmosEngine.Get.CreateObjectScaleGizmo();
        objectUniversalGizmo = RTGizmosEngine.Get.CreateObjectUniversalGizmo();

        ResetAllHandles();

        _workGizmo = objectMoveGizmo;
        _workGizmoId = GizmoId.Move;
    }


    private void Update()
    {
        Scan();
        SetGizmo();
    }

    private void ResetAllHandles()
    {
        objectMoveGizmo.Gizmo.SetEnabled(false);
        objectRotationGizmo.Gizmo.SetEnabled(false);
        objectScaleGizmo.Gizmo.SetEnabled(false);
        objectUniversalGizmo.Gizmo.SetEnabled(false);
    }

    private void SetGizmo()
    {
        if (RTInput.WasKeyPressedThisFrame(KeyCode.W))
        {
            _workGizmoId = GizmoId.Move;
            _workGizmo = objectMoveGizmo;
            ResetAllHandles();
            objectMoveGizmo.Gizmo.SetEnabled(true);
            objectMoveGizmo.RefreshPosition();
            objectMoveGizmo.RefreshRotation();
        }
        else if (RTInput.WasKeyPressedThisFrame(KeyCode.E))
        {
            _workGizmoId = GizmoId.Rotate;
            _workGizmo = objectRotationGizmo;
            ResetAllHandles();
            objectRotationGizmo.Gizmo.SetEnabled(true);
            objectRotationGizmo.RefreshPosition();
            objectRotationGizmo.RefreshRotation();
        }
        else if (RTInput.WasKeyPressedThisFrame(KeyCode.R))
        {
            _workGizmoId = GizmoId.Scale;
            _workGizmo = objectScaleGizmo;
            ResetAllHandles();
            objectScaleGizmo.Gizmo.SetEnabled(true);
            objectScaleGizmo.RefreshPosition();
            objectScaleGizmo.RefreshRotation();
        }
        else if (RTInput.WasKeyPressedThisFrame(KeyCode.T))
        {
            _workGizmoId = GizmoId.Universal;
            _workGizmo = objectUniversalGizmo;
            ResetAllHandles();
            objectUniversalGizmo.Gizmo.SetEnabled(true);
            objectUniversalGizmo.RefreshPosition();
            objectUniversalGizmo.RefreshRotation();
        }
    }

    private void Scan()
    {
        if (RTInput.WasLeftMouseButtonPressedThisFrame() &&
            RTGizmosEngine.Get.HoveredGizmo == null)
        {
            GameObject pickedObject = PickGameObject();
            if (pickedObject != null && pickedObject != _targetObject)
            {
                OnTargetObjectChanged(pickedObject);
                SelectObjectInHierarchy(pickedObject);
            }
        }
    }

    private GameObject PickGameObject()
    {
        // Build a ray using the current mouse cursor position
        Ray ray = Camera.main.ScreenPointToRay(RTInput.MousePosition);
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(RTInput.MousePosition);
        // Check if the ray intersects a game object. If it does, return it
        
        RaycastHit rayHit;
        // in additive scene the normal raycast doesn't work
        // if (Physics.Raycast(ray, out rayHit, float.MaxValue))
        //     return rayHit.collider.gameObject;
        
        PhysicsScene pScene = SceneManager.GetActiveScene().GetPhysicsScene();
        if (pScene.Raycast(worldPoint, ray.direction, out var hit, float.MaxValue))
            return hit.collider.gameObject;

        return null;
    }
    
    public void SelectObjectInHierarchy(GameObject _obj)
    {
        runtimeHierarchy.Select(_obj.transform, RuntimeHierarchy.SelectOptions.FocusOnSelection);
    }

    private void SetWorkGizmoId(GizmoId gizmoId)
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

        if (_targetObject != null) _workGizmo.Gizmo.SetEnabled(true);
    }

    private void OnTargetObjectChanged(GameObject newTargetObject)
    {
        // Store the new target object
        _targetObject = newTargetObject;

        // Is the target object valid?
        if (_targetObject != null)
        {
            objectMoveGizmo.SetTargetObject(_targetObject);
            objectRotationGizmo.SetTargetObject(_targetObject);
            objectScaleGizmo.SetTargetObject(_targetObject);
            objectUniversalGizmo.SetTargetObject(_targetObject);

            _workGizmo.Gizmo.SetEnabled(true);
        }
        else
        {
            objectMoveGizmo.Gizmo.SetEnabled(false);
            objectRotationGizmo.Gizmo.SetEnabled(false);
            objectScaleGizmo.Gizmo.SetEnabled(false);
            objectUniversalGizmo.Gizmo.SetEnabled(false);
        }
    }

    public string GetSceneData(StudioRT.OscillateComponent oscialteComp)
    {
        WorldData wdd = new WorldData();
        wdd.entities = new VirtualEntity[1];
        wdd.entities[0].id = 1;
        wdd.entities[0].primitiveType = "Cube";
        if (!cubeObject)
        {
            cubeObject = GameObject.Find("Cube");
        }
        wdd.entities[0].position = cubeObject.transform.position;
        wdd.entities[0].rotation = cubeObject.transform.eulerAngles;
        wdd.entities[0].scale = cubeObject.transform.localScale;
        wdd.entities[0].components = new EntityBasedComponent[1];
        wdd.entities[0].components[0].type = "Terra.Studio.OscillateComponent";

        var compData = new Terra.Studio.OscillateComponent
        {
            fromPoint = oscialteComp.fromPoint,
            toPoint = oscialteComp.toPoint,
            loop = oscialteComp.loop,
            speed = 1.5f,
            ConditionType = "Terra.Studio.MouseAction",
            ConditionData = "OnClick",
            IsConditionAvailable = oscialteComp.onClick,
            CanExecute = !oscialteComp.onClick
        };

        wdd.entities[0].components[0].data = compData;
        var json = JsonConvert.SerializeObject(wdd);
        Debug.Log($"Data: {json}");

        return json;
    }
}
