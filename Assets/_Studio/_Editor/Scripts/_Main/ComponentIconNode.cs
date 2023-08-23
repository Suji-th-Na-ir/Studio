using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.UI;


namespace Terra.Studio
{
    public class ComponentIconNode : MonoBehaviour
    {
        Camera mainCamera;
        RectTransform rectTransform;
        public RectTransform RectTransform { get { return rectTransform; } }
        Image pointImage;

        ComponentDisplayDock currentTarget;
        public ComponentDisplayDock GetComponentDisplayDockTarget() { return currentTarget; }


        List<UILineRenderer> lineConnectors;

        private List<LineRenderer> lineRenderers = new List<LineRenderer>();


        [SerializeField] List<ComponentIconNode> listnerTargets;
        public List<ComponentIconNode> ListnerTargets
        {
            get { return listnerTargets; }
            set
            {
                if (listnerTargets == null)
                    listnerTargets = new List<ComponentIconNode>();

                listnerTargets.Clear();
                Debug.Log(value.Count);
                listnerTargets = value;
            }
        }



        public void RemoveListnerTargets(ComponentIconNode toRemove)
        {
           
            if(ListnerTargets!=null)
            listnerTargets.Remove(toRemove);
        }

        public void Setup(Sprite icon, ComponentDisplayDock displayDock)
        {
            currentTarget = displayDock;
                if (!mainCamera)
                mainCamera = Camera.main;
            if(GetComponent<RectTransform>()==null)
            rectTransform = gameObject.AddComponent<RectTransform>();
            if (GetComponent<Image>() == null)
                pointImage = gameObject.AddComponent<Image>();

            pointImage.sprite = icon;
            pointImage.raycastTarget = false;
           
            var canvas = FindAnyObjectByType<SceneView>();
            rectTransform.SetParent(canvas.transform, false);
        }


        private void Update()
        {
            if (IsTargetDestroyed())
            {
                foreach (var line in lineConnectors)
                {
                    Destroy(line.gameObject);
                }
                lineConnectors.Clear();
                Destroy(this.gameObject);

                return;
            }
            Vector3 targetPosition = mainCamera.WorldToScreenPoint(currentTarget.componentGameObject.transform.position + Vector3.up);
            transform.position = targetPosition;

            if (listnerTargets == null)
                return;

            if (lineConnectors == null)
                lineConnectors = new List<UILineRenderer>();


            while (lineConnectors.Count < listnerTargets.Count)
            {
                GameObject gm = new GameObject("Line_Connector");
                var rect = gm.AddComponent<RectTransform>();
              
                gm.transform.SetParent(this.transform.parent);
                rect.localScale = Vector2.one;
                rect.anchoredPosition = Vector2.zero;
                var uiconnector = gm.AddComponent<UILineRenderer>();
                gm.AddComponent<CanvasRenderer>();
                lineConnectors.Add(uiconnector);

            }

            if(listnerTargets.Count< lineConnectors.Count)
            {
                var diff = lineConnectors.Count - listnerTargets.Count;
                for (int i = 0; i < diff; i++)
                {
                    lineConnectors[(lineConnectors.Count-1) - i].gameObject.SetActive(false);
                }
            }


            //Update Curves

            var resolution = 30;

            for (int j = 0; j < listnerTargets.Count; j++)
            {
                if (listnerTargets[j] == this)
                    continue;
                Vector3[] points = new Vector3[resolution + 1];
                var endPoint = listnerTargets[j].RectTransform.anchoredPosition;

                for (int i = 0; i <= resolution; i++)
                {
                    float t = (float)i / resolution;
                    Vector3 controlPoint1 = rectTransform.anchoredPosition + new Vector2(0, 100);

                    Vector3 controlPoint2 = (Vector2)endPoint + new Vector2(0, 100);
                    points[i] = CalculateBezierPoint(rectTransform.anchoredPosition, controlPoint1, controlPoint2, endPoint, t);
                }
                lineConnectors[j].gameObject.SetActive(true);
                lineConnectors[j].ClearPoints();
                lineConnectors[j].AddPoints(points.ToList());
            }

            

        }

        private Vector3 CalculateBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            Vector3 p = uu * uu * p0 + 3 * uu * t * p1 + 3 * u * tt * p2 + tt * tt * p3;
            return p;
        }

        private bool IsTargetDestroyed()
        {
            return currentTarget.componentGameObject == null && !ReferenceEquals(currentTarget.componentGameObject, null);
        }
    }
}
