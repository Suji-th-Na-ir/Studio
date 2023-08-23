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
        private Camera m_MainCamera;
        private RectTransform m_RectTransform;
        public RectTransform RectTransform { get { return m_RectTransform; } }

        private Image m_PointImage;
        private Image m_BroadcastIcon;
        private const float m_MinScalingDistance = 50.0f; // Minimum distance for scaling up
        private const float m_MaxScalingDistance = 1.0f; // Maximum distance for scaling down
        private float initialWidth;
        private float initialHeight;


        List<UILineRenderer> m_LineConnectors;

        ComponentDisplayDock m_ObjectTarget;
        public ComponentDisplayDock GetComponentDisplayDockTarget() { return m_ObjectTarget; }


        [SerializeField] List<ComponentIconNode> m_ListnerTargetNodes;
        public List<ComponentIconNode> ListnerTargets
        {
            get { return m_ListnerTargetNodes; }
            set
            {
                if (m_ListnerTargetNodes == null)
                    m_ListnerTargetNodes = new List<ComponentIconNode>();

                m_ListnerTargetNodes.Clear();
                m_ListnerTargetNodes = value;
            }
        }



        public void RemoveListnerTargets(ComponentIconNode toRemove)
        {
           
            if(ListnerTargets!=null)
            m_ListnerTargetNodes.Remove(toRemove);
        }

        public void Setup(Sprite icon, Sprite broadcastIcon, ComponentDisplayDock displayDock)
        {
            m_ObjectTarget = displayDock;
            if (!m_MainCamera)
                m_MainCamera = Camera.main;
            if (GetComponent<RectTransform>() == null)
                m_RectTransform = gameObject.AddComponent<RectTransform>();
            if (GetComponent<Image>() == null)
                m_PointImage = gameObject.AddComponent<Image>();
            m_PointImage.rectTransform.sizeDelta = new Vector2(50, 50);
            m_PointImage.sprite = icon;
            m_PointImage.raycastTarget = false;
            initialWidth = m_RectTransform.sizeDelta.x;
            initialHeight = m_RectTransform.sizeDelta.y;
            var canvas = FindAnyObjectByType<SceneView>();
            m_RectTransform.SetParent(canvas.transform, false);

            //Broadcast
            GameObject gm = new GameObject("Broadcast_Icon");
            var rectTransform1 = gm.AddComponent<RectTransform>();
            var pointImage1 = gm.AddComponent<Image>();
            pointImage1.rectTransform.sizeDelta = new Vector2(50, 50);
            pointImage1.sprite = broadcastIcon;
            pointImage1.raycastTarget = false;
            rectTransform1.SetParent(this.transform, false);
            rectTransform1.anchoredPosition = m_RectTransform.anchoredPosition + new Vector2(30, 30);
            m_BroadcastIcon = pointImage1;
        }


        private void Update()
        {
            //if(!EditorOp.Resolve<SelectionHandler>().GetSelectedObjects().Contains(currentTarget.componentGameObject))
            //{
            //    rectTransform.sizeDelta = Vector2.zero;
            //    if (lineConnectors != null)
            //    {
            //        for (int i = lineConnectors.Count - 1; i >= 0; i--)
            //        {
            //            lineConnectors[i].gameObject.SetActive(false);
            //        }
            //    }
            //    return;
            //}

            if (IsTargetDestroyed())
            {
                foreach (var line in m_LineConnectors)
                {
                    Destroy(line.gameObject);
                }
                m_LineConnectors.Clear();
                Destroy(this.gameObject);

                return;
            }



            Vector3 targetPosition = m_MainCamera.WorldToScreenPoint(m_ObjectTarget.componentGameObject.transform.position + Vector3.up);
            transform.position = targetPosition;

            float distanceToTarget = Vector3.Distance(m_ObjectTarget.componentGameObject.transform.position, Camera.main.transform.position);
            float scalingFactor = CalculateScalingFactor(distanceToTarget);
            m_RectTransform.sizeDelta = new Vector2(initialWidth * scalingFactor, initialHeight * scalingFactor);

            if (m_ListnerTargetNodes == null)
            {
                m_BroadcastIcon.gameObject.SetActive(false);
                return;
            }

            if (m_LineConnectors == null)
                m_LineConnectors = new List<UILineRenderer>();


            while (m_LineConnectors.Count < m_ListnerTargetNodes.Count)
            {
                GameObject gm = new GameObject("Line_Connector");
                var rect = gm.AddComponent<RectTransform>();

                gm.transform.SetParent(this.transform.parent);
                rect.localScale = Vector2.one;
                rect.anchoredPosition = Vector2.zero;
                var uiconnector = gm.AddComponent<UILineRenderer>();
                gm.AddComponent<CanvasRenderer>();
                m_LineConnectors.Add(uiconnector);

            }

            if (m_ListnerTargetNodes.Count < m_LineConnectors.Count)
            {
                var diff = m_LineConnectors.Count - m_ListnerTargetNodes.Count;
                for (int i = 0; i < diff; i++)
                {
                    m_LineConnectors[(m_LineConnectors.Count - 1) - i].gameObject.SetActive(false);
                }
            }




            //Update Curves
            if (!CheckIfInsideScreen(m_RectTransform) || scalingFactor == 0)
            {
                for (int i = m_LineConnectors.Count - 1; i >= 0; i--)
                {
                    m_LineConnectors[i].gameObject.SetActive(false);
                }
                m_BroadcastIcon.gameObject.SetActive(false);
                return;
            }

            var resolution = 30;

            for (int j = 0; j < m_ListnerTargetNodes.Count; j++)
            {
                m_BroadcastIcon.gameObject.SetActive(true);
                if (m_ListnerTargetNodes[j] == this)
                    continue;
                if (!CheckIfInsideScreen(m_ListnerTargetNodes[j].RectTransform) || m_ListnerTargetNodes[j].RectTransform.sizeDelta == Vector2.zero)
                {
                    m_LineConnectors[j].gameObject.SetActive(false);
                    continue;
                }

                Vector3[] points = new Vector3[resolution + 1];
                var endPoint = m_ListnerTargetNodes[j].RectTransform.anchoredPosition;

                for (int i = 0; i <= resolution; i++)
                {
                    float t = (float)i / resolution;
                    Vector3 controlPoint1 = m_RectTransform.anchoredPosition + new Vector2(0, 100);

                    Vector3 controlPoint2 = (Vector2)endPoint + new Vector2(0, 100);
                    points[i] = CalculateBezierPoint(m_RectTransform.anchoredPosition, controlPoint1, controlPoint2, endPoint, t);
                }
                m_LineConnectors[j].gameObject.SetActive(true);
                m_LineConnectors[j].ClearPoints();
                m_LineConnectors[j].AddPoints(points.ToList());
            }



        }

        private bool CheckIfInsideScreen(RectTransform rect)
        {
            Vector2 anchoredPosition = rect.anchoredPosition;
            Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(null, rect.position);

            bool isInsideScreen =
                screenPosition.x >= 0 && screenPosition.x <= Screen.width &&
                screenPosition.y >= 0 && screenPosition.y <= Screen.height;
            return isInsideScreen;
            
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
            return m_ObjectTarget.componentGameObject == null && !ReferenceEquals(m_ObjectTarget.componentGameObject, null);
        }

        float CalculateScalingFactor(float distance)
        {
            if (distance > m_MinScalingDistance)
            {
                return 0.0f;  // Scale image up
            }
            else if (distance < m_MaxScalingDistance)
            {
                return 0.0f;  // Scale image down
            }
            else
            {
                // Linear interpolation between scaling factors
                return Mathf.Lerp(0.5f, 2.0f, (distance - m_MinScalingDistance) / (m_MaxScalingDistance - m_MinScalingDistance));
            }
        }
    }
}
