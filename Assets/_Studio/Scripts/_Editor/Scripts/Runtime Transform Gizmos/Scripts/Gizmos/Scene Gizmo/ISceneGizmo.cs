using UnityEngine;
using RuntimeCommon;

namespace RTG
{
    public interface ISceneGizmo
    {
        Gizmo OwnerGizmo { get; }
        Camera SceneCamera { get; }
    }
}
