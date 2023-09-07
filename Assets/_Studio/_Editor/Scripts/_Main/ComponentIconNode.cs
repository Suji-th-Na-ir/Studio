using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using RuntimeInspectorNamespace;
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
        private const float m_MinScalingDistance = 40.0f; // Minimum distance for scaling up
        private const float m_MaxScalingDistance = 0.0f; // Maximum distance for scaling down
        private const int RESOLUTION = 20;
        private float initialWidth;
        private float initialHeight;

        private Sprite m_broadcastSprite;
        private Sprite m_broadcastNoListnerSprite;
        private Sprite m_GameWonBroadcastSprite;
        RuntimeInspector Inspector;
        private bool m_isBroadcating = false;
        private GameObject m_LineRenderGO;
        public bool ISBroadcasting
        {
            get { return m_isBroadcating; }
            set
            {
                m_isBroadcating = value;
                if (!m_isBroadcating)
                {
                    ClearAllLineRenderers();
                }
            }
        }
        
        public bool m_isBroadcatingGameWon = false;
        public int m_componentIndex = 0;
        public bool isTargetSelected;
        List<LineRenderer> m_LineConnectors;

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

        [SerializeField] List<ComponentIconNode> m_BroadcastTargetNodes;
        public List<ComponentIconNode> BroadcastTargets
        {
            get { return m_BroadcastTargetNodes; }
            set
            {
                if (m_BroadcastTargetNodes == null)
                    m_BroadcastTargetNodes = new List<ComponentIconNode>();

                m_BroadcastTargetNodes.Clear();
                m_BroadcastTargetNodes = value;
            }
        }

        public void RemoveListnerTargets(ComponentIconNode toRemove)
        {  
            if(ListnerTargets!=null)
            m_ListnerTargetNodes.Remove(toRemove);
        }

        public void Setup(Sprite icon, Sprite broadcastIcon,Sprite broadcast_noListners,Sprite gameWonIcon,  ComponentDisplayDock displayDock)
        {
            Inspector = FindAnyObjectByType<RuntimeInspector>();
            m_ObjectTarget = displayDock;
            if (!m_MainCamera)
                m_MainCamera = Camera.main;
            if (GetComponent<RectTransform>() == null)
                m_RectTransform = gameObject.AddComponent<RectTransform>();
            if (GetComponent<Image>() == null)
                m_PointImage = gameObject.AddComponent<Image>();
            m_PointImage.rectTransform.sizeDelta = new Vector2(30, 30);
            m_PointImage.sprite = icon;
            m_PointImage.raycastTarget = false;
            initialWidth = 1.5f;
            initialHeight =1.5f;
            var canvas = FindAnyObjectByType<SceneView>();
            m_RectTransform.SetParent(canvas.transform, false);

            //Broadcast
            GameObject gm = new GameObject("Broadcast_Icon");
            var rectTransform1 = gm.AddComponent<RectTransform>();
            var pointImage1 = gm.AddComponent<Image>();
            pointImage1.rectTransform.sizeDelta = new Vector2(30, 30);
            pointImage1.raycastTarget = false;
            m_BroadcastIcon = pointImage1;
            rectTransform1.SetParent(this.transform, false);
            rectTransform1.anchoredPosition = m_RectTransform.anchoredPosition + new Vector2(10, 10);
            rectTransform1.localScale = new Vector2(initialWidth*0.5f,  initialHeight * 0.5f);
            m_broadcastSprite = broadcastIcon;
            m_broadcastNoListnerSprite = broadcast_noListners;
            m_GameWonBroadcastSprite = gameWonIcon;
            m_LineRenderGO = EditorOp.Load<GameObject>("Prefabs/Line");
        }


        public void DestroyThisIcon()
        {
            ClearAllLineRenderers();
            Destroy(this.gameObject);
        }

        private void Update()
        {

            if(Inspector.currentPageIndex==0)
            {
                m_BroadcastIcon.enabled = m_PointImage.enabled = false;
                RectTransform.localScale = Vector3.zero;
                ClearAllLineRenderers();
                return;
            }
            else
            {
                m_BroadcastIcon.enabled = m_PointImage.enabled = true;
            }

            if (IsTargetDestroyed())
            {
                return;
            }
            Vector3 effectivePos = m_ObjectTarget.componentGameObject.transform.position;
            
            if (m_MainCamera.WorldToScreenPoint(effectivePos + Vector3.up * 0.3f).z<0)
            {
                effectivePos = m_MainCamera.transform.position +Vector3.ClampMagnitude(effectivePos - m_MainCamera.transform.position, 1.0f);
            }
            var objectScreenPos = m_MainCamera.WorldToViewportPoint(effectivePos + Vector3.up * 0.3f);
            objectScreenPos = m_MainCamera.ViewportToScreenPoint(objectScreenPos);
            var offset = m_MainCamera.WorldToViewportPoint(effectivePos + Vector3.up * 0.5f);
            offset = m_MainCamera.ViewportToScreenPoint(offset);
            var radius = (objectScreenPos - offset).magnitude;
            Vector3 screenPoint = CalculateCircularPositionAtIndex(objectScreenPos, radius, 3, m_componentIndex);
            float distanceToTarget = Vector3.Distance(m_ObjectTarget.componentGameObject.transform.position, m_MainCamera.transform.position);
            float scalingFactor = CalculateScalingFactor(distanceToTarget);

            if ((screenPoint.z > 0 &&
    screenPoint.x > 0 && screenPoint.x <= Screen.width &&
    screenPoint.y > 0 && screenPoint.y <= Screen.height) || isTargetSelected)
            {
                m_RectTransform.localScale = new Vector3(initialWidth * scalingFactor, initialHeight * scalingFactor, 1);
                transform.position = screenPoint;
            }
            else
            {
                transform.localScale = Vector3.zero;
            }
           

            if (m_isBroadcating)
            {
                m_BroadcastIcon.gameObject.SetActive(true);
                if(m_isBroadcatingGameWon)
                {
                    m_BroadcastIcon.sprite = m_GameWonBroadcastSprite;
                }
                else if(m_LineConnectors!=null && m_ListnerTargetNodes.Count>0)
                {
                    m_BroadcastIcon.sprite = m_broadcastSprite;
                }
                else
                {
                    m_BroadcastIcon.sprite = m_broadcastNoListnerSprite;
                }
            }
            else
            {
                m_BroadcastIcon.gameObject.SetActive(false);
                return;
            }

            if (m_ListnerTargetNodes == null)
                return;

            if (m_LineConnectors == null)
                m_LineConnectors = new List<LineRenderer>();

            

            while (m_LineConnectors.Count < m_ListnerTargetNodes.Count)
            {

                GameObject gm = Instantiate(m_LineRenderGO);
                //var rect = gm.AddComponent<RectTransform>();
                var parent = FindAnyObjectByType<HideInHierarchy>();
                gm.transform.SetParent(parent.transform);
              
                var uiconnector = gm.GetComponent<LineRenderer>();
               
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
            if (!isTargetSelected)
            {
                if ( scalingFactor == 0)
                {
                    for (int i = 0; i < m_LineConnectors.Count; i++)
                    {
                       
                        m_LineConnectors[i].gameObject.SetActive(false);
                    }
                    m_BroadcastIcon.gameObject.SetActive(false);
                    return;
                }
            }


            for (int j = 0; j < m_ListnerTargetNodes.Count; j++)
            {
                if (m_ListnerTargetNodes[j] == this)
                    continue;
                if (m_ListnerTargetNodes[j] == null || !CheckIfInsideScreen(RectTransform) && !CheckIfInsideScreen(m_ListnerTargetNodes[j].RectTransform)
                    || transform.localScale == Vector3.zero && m_ListnerTargetNodes[j].transform.localScale == Vector3.zero)
                {
                    m_LineConnectors[j].gameObject.SetActive(false);
                    continue;
                }

                Vector3[] points = new Vector3[RESOLUTION + 1];
                Vector3 startPoint, endPoint, controlPoint1, controlPoint2;

                if (!CheckIfInsideScreen(RectTransform) || transform.localScale == Vector3.zero)
                {
                   startPoint= m_ObjectTarget.componentGameObject.transform.position;
                }
                else
                {
                    startPoint = m_MainCamera.ScreenToWorldPoint(RectTransform.transform.position);
                }
                if (!CheckIfInsideScreen(m_ListnerTargetNodes[j].RectTransform) || m_ListnerTargetNodes[j].transform.localScale == Vector3.zero)
                {
                    endPoint = m_ListnerTargetNodes[j].m_ObjectTarget.componentGameObject.transform.position;
                }
                else
                {
                    endPoint =m_MainCamera.ScreenToWorldPoint(m_ListnerTargetNodes[j].transform.position);
                }

                controlPoint1 = startPoint + new Vector3(0, 2);
                controlPoint2 = endPoint + new Vector3(0, 2);

                for (int i = 0; i <= RESOLUTION; i++)
                {
                    float t = (float)i / RESOLUTION;

                    points[i] = CalculateBezierPoint(startPoint, controlPoint1, controlPoint2, endPoint, t);
                }
                m_LineConnectors[j].gameObject.SetActive(true);
                m_LineConnectors[j].positionCount = points.Length;
                m_LineConnectors[j].SetPositions(points);
                // m_LineConnectors[j].SetLineAlpha(GetFadeValue(distanceToTarget,-1,40));
            }
        }

        private void ClearAllLineRenderers()
        {
            if (m_LineConnectors != null)
            {
                foreach (var line in m_LineConnectors)
                {
                    Destroy(line.gameObject);
                }
                m_LineConnectors.Clear();
            }

        }

        Vector3 CalculateCircularPositionAtIndex(Vector3 targetPosition, float radius, int numPositions, int index)
        {
            float angleIncrement = 360f / numPositions;
            float angle = index * angleIncrement;

            float xOffset = radius * Mathf.Cos(Mathf.Deg2Rad * angle);
            float yOffset = radius * Mathf.Sin(Mathf.Deg2Rad * angle);

            Vector3 offsetPosition = targetPosition + new Vector3(xOffset, yOffset, 0f);
            return offsetPosition;
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

        private float GetFadeValue(float distance, float min, float max)
        {
            return 1- ((distance - min) /( max - min));
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
                return Mathf.Lerp(0.2f, 1.2f, (distance - m_MinScalingDistance) / (m_MaxScalingDistance - m_MinScalingDistance));
            }
        }
    }
}
