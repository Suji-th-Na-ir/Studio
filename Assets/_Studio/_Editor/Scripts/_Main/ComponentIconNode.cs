using System.Collections.Generic;
using RuntimeInspectorNamespace;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;


namespace Terra.Studio
{
    public class ComponentIconNode : MonoBehaviour
    {
        private Camera m_MainCamera;
        private RectTransform m_RectTransform;
        public RectTransform RectTransform { get { return m_RectTransform; } }

        private Image m_ComponentTypeIcon;
        private List<Image> m_BroadcastIcon= new List<Image>();
        private Image m_ListenIcon;
        private const float m_MinScalingDistance = 40.0f; // Minimum distance for scaling up
        private const float m_MaxScalingDistance = 0.0f; // Maximum distance for scaling down
        private const int RESOLUTION = 20;
        private float initialWidth;
        private float initialHeight;
        private GameObject m_LineRenderGO;
        private Sprite m_broadcastSprite;
        private Sprite m_broadcastNoListnerSprite;
        private Sprite m_GameWonBroadcastSprite;
        private Sprite m_ListenSprite;
        RuntimeInspector Inspector;
        
        private bool ISBroadcasting
        {
            get { return m_broadcasatingStrings.Count>0; }
           
        }

        private bool m_isListning = false;
        public bool IsListning
        {
            get { return m_isListning; }
            set
            {
                m_isListning = value;
            }
        }

        public bool m_isBroadcatingGameWon = false;
        public int m_componentIndex = 0;
        public bool isTargetSelected;
        List<LineRenderer> m_LineConnectors;

        [SerializeField]private List<string> m_broadcasatingStrings=new List<string>();
        public List<string> BroadcastingStrings
        { 
            get { return m_broadcasatingStrings; }
            set
            {
                for (int i = 0; i < value.Count; i++)
                {
                    if (!m_broadcasatingStrings.Contains(value[i]))
                        m_broadcasatingStrings.Add(value[i]);
                }
                UpdateBroadcastIcons();
            }
        }

        [SerializeField] private List<string> m_listeningStrings = new List<string>();
        public List<string> ListenStrings
        {
            get { return m_listeningStrings; }
            set
            {
                for (int i = 0; i < value.Count; i++)
                {
                    if (!m_listeningStrings.Contains(value[i]))
                        m_listeningStrings.Add(value[i]);
                }
            }
        }


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

                for (int i = 0; i < value.Count; i++)
                {
                    if (!m_ListnerTargetNodes.Contains(value[i]))
                        m_ListnerTargetNodes.Add(value[i]);
                }
              //  m_ListnerTargetNodes = value;
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

                for (int i = 0; i < value.Count; i++)
                {
                    if (!m_BroadcastTargetNodes.Contains(value[i]))
                        m_BroadcastTargetNodes.Add(value[i]);
                }
                m_BroadcastTargetNodes = value;
            }
        }


        public void RemoveListnerTargets(ComponentIconNode toRemove)
        {  
            if(ListnerTargets!=null)
            m_ListnerTargetNodes.Remove(toRemove);
        }

        public void Setup(ComponentIconsPreset iconPresets,  ComponentDisplayDock displayDock)
        {
            Inspector = FindAnyObjectByType<RuntimeInspector>();
            m_ObjectTarget = displayDock;
            if (!m_MainCamera)
                m_MainCamera = Camera.main;
            if (GetComponent<RectTransform>() == null)
                m_RectTransform = gameObject.AddComponent<RectTransform>();
            if (GetComponent<Image>() == null)
                m_ComponentTypeIcon = gameObject.AddComponent<Image>();
            m_ComponentTypeIcon.rectTransform.sizeDelta = new Vector2(30, 30);
            m_ComponentTypeIcon.sprite = iconPresets.GetIcon(displayDock.componentType);
            m_ComponentTypeIcon.raycastTarget = false;
            initialWidth = 1.5f;
            initialHeight =1.5f;
            var canvas = FindAnyObjectByType<SceneView>();
            m_RectTransform.SetParent(canvas.transform, false);

            //Broadcast
           
            m_broadcastSprite = iconPresets.GetIcon("Broadcast");
            m_broadcastNoListnerSprite = iconPresets.GetIcon("BroadcastNoListner");
            m_GameWonBroadcastSprite = iconPresets.GetIcon("GameWon");

            //Listen
            GameObject gm2 = new GameObject("Listen_Icon");
            var rectTransform2 = gm2.AddComponent<RectTransform>();
            var pointImage2 = gm2.AddComponent<Image>();
            pointImage2.rectTransform.sizeDelta = new Vector2(30, 30);
            pointImage2.raycastTarget = false;
            m_ListenIcon = pointImage2;
            rectTransform2.SetParent(this.transform, false);
            rectTransform2.anchoredPosition = m_RectTransform.anchoredPosition + new Vector2(-10, 10);
            rectTransform2.localScale = new Vector2(initialWidth * 0.5f, initialHeight * 0.5f);
            m_ListenSprite = iconPresets.GetIcon("Listen");
            m_ListenIcon.sprite = m_ListenSprite;
            m_LineRenderGO = EditorOp.Load<GameObject>("Prefabs/Line");
        }


        private void UpdateBroadcastIcons()
        {
            var remaing = BroadcastingStrings.Count - m_BroadcastIcon.Count;

            for (int i = 0; i < remaing; i++)
            {
                GameObject gm = new GameObject("Broadcast_Icon");
                var rectTransform1 = gm.AddComponent<RectTransform>();
                var pointImage1 = gm.AddComponent<Image>();
                pointImage1.rectTransform.sizeDelta = new Vector2(30, 30);
                pointImage1.raycastTarget = false;
                m_BroadcastIcon.Add(pointImage1);
                rectTransform1.SetParent(this.transform, false);
                rectTransform1.localScale = new Vector2(initialWidth * 0.5f, initialHeight * 0.5f);
                PositionAroundCenterImage(rectTransform1,m_BroadcastIcon.IndexOf(pointImage1),m_BroadcastIcon.Count,20,90);
            }
        }

        public void DestroyThisIcon()
        {
            ClearAllLineRenderers();
            Destroy(this.gameObject);
        }

        private void Update()
        {

            if (Inspector.currentPageIndex == 0)
            {
                foreach (var b in m_BroadcastIcon)
                {
                    b.enabled = false;
                }
                m_ComponentTypeIcon.enabled = false;
                RectTransform.localScale = Vector3.zero;
                ClearAllLineRenderers();
                return;
            }
            else
            {
                foreach (var b in m_BroadcastIcon)
                {
                    b.enabled = true;
                }
                m_ComponentTypeIcon.enabled = true;
            }

            if (IsTargetDestroyed())
            {
                return;
            }
            Vector3 effectivePos = m_ObjectTarget.componentGameObject.transform.position;
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
                screenPoint.y > 0 && screenPoint.y <= Screen.height))
            {
                m_RectTransform.localScale = new Vector3(initialWidth * scalingFactor, initialHeight * scalingFactor, 1);
                transform.position = screenPoint;
            }
            else
            {
                transform.localScale = Vector3.zero;
            }

            if (IsListning)
            {
                if (m_BroadcastTargetNodes != null && m_BroadcastTargetNodes.Count > 0)
                {
                    m_ListenIcon.gameObject.SetActive(false);
                }
                else
                {
                    m_ListenIcon.gameObject.SetActive(true);
                }
            }
            else
            {
                m_ListenIcon.gameObject.SetActive(false);
            }

            for (int i = 0; i < m_BroadcastIcon.Count; i++)
            {
                if (i > BroadcastingStrings.Count-1)
                {
                    m_BroadcastIcon[i].gameObject.SetActive(false);
                }
                else
                {
                    if (ISBroadcasting)
                    {
                        if (scalingFactor == 0)
                            m_BroadcastIcon[i].gameObject.SetActive(false);
                        else
                            m_BroadcastIcon[i].gameObject.SetActive(true);
                        if (m_isBroadcatingGameWon)
                        {
                            m_BroadcastIcon[i].sprite = m_GameWonBroadcastSprite;
                        }
                        else if (m_LineConnectors != null && m_ListnerTargetNodes?.FindAll(a => a.ListenStrings.Contains(BroadcastingStrings[i])).Count != 0)
                        {
                            m_BroadcastIcon[i].sprite = m_broadcastSprite;
                        }
                        else
                        {
                            m_BroadcastIcon[i].sprite = m_broadcastNoListnerSprite;
                        }
                    }
                    else
                    {
                        m_BroadcastIcon[i].gameObject.SetActive(false);
                        
                    }
                }
            }
            

            if (m_ListnerTargetNodes == null)
                return;

            if (m_LineConnectors == null)
                m_LineConnectors = new List<LineRenderer>();

            

            while (m_LineConnectors.Count < m_ListnerTargetNodes.Count)
            {

                GameObject gm = Instantiate(m_LineRenderGO);
                gm.transform.SetParent(this.transform);
              
                var uiconnector = gm.GetComponent<LineRenderer>();
               
                m_LineConnectors.Add(uiconnector);
                gm.name = $"Line_{m_ListnerTargetNodes[m_LineConnectors.IndexOf(uiconnector)]}";

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
           
            if (scalingFactor == 0 || !isTargetSelected)
            {
                for (int i = 0; i < m_LineConnectors.Count; i++)
                {
                    m_LineConnectors[i].gameObject.SetActive(false);
                }           
                return;
            }

            int lineConnectorIndex = 0;
            for (int k = 0; k < m_broadcasatingStrings.Count; k++)
            {
                var allTargets = m_ListnerTargetNodes?.FindAll(a => a.ListenStrings.Contains(m_broadcasatingStrings[k]));
                for (int j = 0; j < allTargets.Count; j++)
                {
                    if (allTargets[j] == this)
                        continue;
                    if (allTargets[j] == null || !CheckIfInsideScreen(RectTransform) && !CheckIfInsideScreen(allTargets[j].RectTransform)
                        || transform.localScale == Vector3.zero && allTargets[j].transform.localScale == Vector3.zero || !allTargets[j].isTargetSelected)
                    {
                        m_LineConnectors[lineConnectorIndex].gameObject.SetActive(false);
                        continue;
                    }

                    Vector3[] points = new Vector3[RESOLUTION + 1];
                    Vector3 startPoint, endPoint, controlPoint1, controlPoint2;

                    if (!CheckIfInsideScreen(RectTransform) || transform.localScale == Vector3.zero)
                    {
                        startPoint = m_ObjectTarget.componentGameObject.transform.position;
                    }
                    else
                    {
                        startPoint = m_MainCamera.ScreenToWorldPoint(m_BroadcastIcon[k].transform.position);
                    }
                    if (!CheckIfInsideScreen(allTargets[j].RectTransform) || allTargets[j].transform.localScale == Vector3.zero)
                    {
                        endPoint = allTargets[j].m_ObjectTarget.componentGameObject.transform.position;
                    }
                    else
                    {
                        endPoint = m_MainCamera.ScreenToWorldPoint(allTargets[j].m_ListenIcon.transform.position);
                    }

                    controlPoint1 = startPoint + new Vector3(0, 2);
                    controlPoint2 = endPoint + new Vector3(0, 2);

                    for (int i = 0; i <= RESOLUTION; i++)
                    {
                        float t = (float)i / RESOLUTION;

                        points[i] = CalculateBezierPoint(startPoint, controlPoint1, controlPoint2, endPoint, t);
                    }
                    m_LineConnectors[lineConnectorIndex].gameObject.SetActive(true);
                   
                    m_LineConnectors[lineConnectorIndex].positionCount = points.Length;
                    m_LineConnectors[lineConnectorIndex].SetPositions(points);

                    Color c = Color.white;
                    c.a = GetFadeValue(distanceToTarget, -1, 40);
                    c.a = Mathf.Clamp(c.a, 0.0f, 1.0f);
                    m_LineConnectors[lineConnectorIndex].material.SetColor("_GoodColor", c);
                    lineConnectorIndex++;

                }
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

        void PositionAroundCenterImage(RectTransform imageToPosition, int index, int totalImages,float radius, float totalAngle)
        {
            float angleIncrement = totalAngle / totalImages;
            float startingAngle = Mathf.PI / 4.0f; // 60 degrees in radians

            // Calculate the angle in radians, starting from the top and moving clockwise.
            float angle = startingAngle - index * angleIncrement;

            float xOffset = radius * Mathf.Cos(angle);
            float yOffset = radius * Mathf.Sin(angle);
            Vector2 offsetPosition = new Vector2(xOffset, yOffset);
            imageToPosition.anchoredPosition = offsetPosition;
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
