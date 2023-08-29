using UnityEngine;

namespace RTG
{
    public class SceneGizmoCamViewportUpdater : ISceneGizmoCamViewportUpdater
    {
        private SceneGizmo _sceneGizmo;

        public SceneGizmoCamViewportUpdater(SceneGizmo sceneGizmo)
        {
            _sceneGizmo = sceneGizmo;
        }

        public void Update(RTSceneGizmoCamera sceneGizmoCamera)
        {
            SceneGizmoLookAndFeel lookAndFeel = _sceneGizmo.LookAndFeel;
            Vector2 screenOffset = lookAndFeel.ScreenOffset;
            Rect sceneCameraViewRect = sceneGizmoCamera.SceneCamera.pixelRect;
            Vector2 camPrjSwitchLabelRectSize = lookAndFeel.CalculateMaxPrjSwitchLabelRectSize();
            bool usesPrjSwitchLabel = lookAndFeel.IsCamPrjSwitchLabelVisible;
            lookAndFeel.ScreenSize = Screen.height / 10.8f;
            float screenSize = lookAndFeel.ScreenSize;
            screenOffset.x = -Screen.width / 2.90002f;
            screenOffset.y = -Screen.height / 10.8f;
            if (lookAndFeel.ScreenCorner == SceneGizmoScreenCorner.TopRight)
                //sceneGizmoCamera.Camera.pixelRect = new Rect ((sceneCameraViewRect.xMax / 2.90f) + screenSize, (sceneCameraViewRect.yMax / 10.08f) - screenSize, screenSize, screenSize);
            sceneGizmoCamera.Camera.pixelRect = new Rect(sceneCameraViewRect.xMax - screenSize + screenOffset.x, sceneCameraViewRect.yMax - screenSize + screenOffset.y, screenSize, screenSize);
            else
            if (lookAndFeel.ScreenCorner == SceneGizmoScreenCorner.TopLeft)
                sceneGizmoCamera.Camera.pixelRect = new Rect (sceneCameraViewRect.xMin + screenOffset.x, sceneCameraViewRect.yMax - screenSize + screenOffset.y, screenSize, screenSize);
            else
            if (lookAndFeel.ScreenCorner == SceneGizmoScreenCorner.BottomRight)
                sceneGizmoCamera.Camera.pixelRect = new Rect (sceneCameraViewRect.xMax - screenSize + screenOffset.x,
                    sceneCameraViewRect.yMin + (usesPrjSwitchLabel ? camPrjSwitchLabelRectSize.y + 1.0f : 0.0f) + screenOffset.y, screenSize, screenSize);
            else
                sceneGizmoCamera.Camera.pixelRect = new Rect (sceneCameraViewRect.xMin + screenOffset.x,
                    sceneCameraViewRect.yMin + (usesPrjSwitchLabel ? camPrjSwitchLabelRectSize.y + 1.0f : 0.0f) + screenOffset.y, screenSize, screenSize);
        }
    }
}
