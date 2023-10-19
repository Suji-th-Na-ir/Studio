using System.Collections.Generic;
using System.Linq;
using PlayShifu.Terra;
using RuntimeInspectorNamespace;
using UnityEngine;

namespace Terra.Studio
{
    public class ComponentIconNode : MonoBehaviour
    {
        private Camera m_MainCamera;
        private RectTransform m_RectTransform;
        public RectTransform RectTransform { get { return m_RectTransform; } }


        private const float m_MinScalingDistance = 40.0f; // Minimum distance for scaling up
        private const float m_MaxScalingDistance = 0.0f; // Maximum distance for scaling down
        private const int RESOLUTION = 25;
        private float initialWidth;
        private float initialHeight;
        private GameObject m_LineRenderGO;

        RuntimeInspector Inspector;



        private Icon iconPrefab;

        private Icon behaviourIcon;
        private List<Icon> broadcasticons = new List<Icon>();
        private Icon listenIcon;

        public Transform ListenIconTransform { get { return listenIcon.transform; } }

        private bool ISBroadcasting
        {
            get { return m_broadcasatingStrings.Count > 0; }
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
        public bool m_isBroadcatingGameLoose = false;
        public int m_componentIndex = 0;
        public bool isTargetSelected;
        List<LineRenderer> m_LineConnectors;

        [SerializeField] private List<string> m_broadcasatingStrings = new List<string>();
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
            if (ListnerTargets != null)
                m_ListnerTargetNodes.Remove(toRemove);
        }

        public void Setup(ComponentDisplayDock displayDock)
        {
            Inspector = EditorOp.Resolve<RuntimeInspector>();
            m_ObjectTarget = displayDock;

            if (!m_MainCamera)
                m_MainCamera = Camera.main;
            iconPrefab = EditorOp.Load<Icon>("Prefabs/BehaviourIcon");

            m_RectTransform = gameObject.AddComponent<RectTransform>();

            //Setting up behaviour Icon
            behaviourIcon = Instantiate(iconPrefab);
            behaviourIcon.Setup(new Vector2(13, 13), "Circle", displayDock.ComponentType, transform, false);

            initialWidth = 1.5f;
            initialHeight = 1.5f;

            //Setting Up listner Icon
            listenIcon = Instantiate(iconPrefab);
            listenIcon.name = "Listen_Icon";
            listenIcon.Setup(new Vector2(8, 8), "OutlineCircle", "Listen", transform, false);
            PositionAroundCenterImage(listenIcon.RectTransform, 0, 1, 12, 90, false);

            m_LineRenderGO = EditorOp.Load<GameObject>("Prefabs/Line");
        }


        private void UpdateBroadcastIcons()
        {
            var remaing = BroadcastingStrings.Count - broadcasticons.Count;
            for (int i = 0; i < remaing; i++)
            {
                var newIcon = Instantiate(iconPrefab);
                newIcon.Setup(new Vector2(8, 8), "OutlineCircle", "Broadcast", transform, false);
                newIcon.name = $"BroadcatIcon_{i}";
                broadcasticons.Add(newIcon);
                PositionAroundCenterImage(newIcon.RectTransform, broadcasticons.IndexOf(newIcon), broadcasticons.Count, 12, 90, true);
            }
        }

        public void DestroyThisIcon()
        {
            m_broadcasatingStrings.Clear();
            m_listeningStrings.Clear();
            broadcasticons.Clear();
            ClearAllLineRenderers();
            Destroy(this.gameObject);
        }

        private void Update()
        {
            if (!Inspector)
            {
                return;
            }
            if (Inspector.currentPageIndex == 0)
            {
                foreach (var b in broadcasticons)
                {
                    b.Hide();
                }
                behaviourIcon.Hide();
                RectTransform.localScale = Vector3.zero;
                ClearAllLineRenderers();
                return;
            }
            else
            {
                foreach (var b in broadcasticons)
                {
                    b.Show();
                }
                behaviourIcon.Show();
            }

            if (IsTargetDestroyed())
            {
                return;
            }
            Vector3 effectivePos = m_ObjectTarget.ComponentGameObject.transform.position;
            var objectScreenPos = m_MainCamera.WorldToViewportPoint(effectivePos + Vector3.up * 0.3f);
            objectScreenPos = m_MainCamera.ViewportToScreenPoint(objectScreenPos);
            var offset = m_MainCamera.WorldToViewportPoint(effectivePos + Vector3.up);
            offset = m_MainCamera.ViewportToScreenPoint(offset);
            var radius = (objectScreenPos - offset).magnitude;
            Vector3 screenPoint = CalculateCircularPositionAtIndex(objectScreenPos, radius, 3, m_componentIndex);
            float distanceToTarget = Vector3.Distance(m_ObjectTarget.ComponentGameObject.transform.position, m_MainCamera.transform.position);
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
                listenIcon.Show();
                if (BroadcastingStrings == null && BroadcastingStrings.Count == 0)
                {
                    listenIcon.SetIconImage("NoOneBroadcasting");
                    listenIcon.SetBackgroundColor(Helper.GetColorFromHex("#FF413B"));
                }

                bool broadcaterPresent = false;
                for (int j = 0; j < ListenStrings.Count; j++)
                {
                    var allbrodcasters = m_BroadcastTargetNodes?.FindAll(a => a.BroadcastingStrings.Contains(m_listeningStrings[j]));
                    if (allbrodcasters.Count > 0)
                    {
                        broadcaterPresent = true;
                        break;
                    }
                }
                if (broadcaterPresent)
                {
                    listenIcon.SetIconImage("Listen");
                    listenIcon.SetBackgroundColor(Color.white);
                }
                else
                {
                    listenIcon.SetIconImage("NoOneBroadcasting");
                    listenIcon.SetBackgroundColor(Helper.GetColorFromHex("#FF413B"));
                }

            }
            else
            {
                listenIcon.Hide();
            }

            for (int i = 0; i < broadcasticons.Count; i++)
            {
                if (i > BroadcastingStrings.Count - 1)
                {
                    broadcasticons[i].Hide();
                }
                else
                {
                    if (ISBroadcasting)
                    {
                        if (scalingFactor == 0)
                            broadcasticons[i].Hide();
                        else
                            broadcasticons[i].Show();

                        if (m_isBroadcatingGameWon)
                        {
                            broadcasticons[i].SetIconImage("GameWon");
                            broadcasticons[i].SetBackgroundColor(Helper.GetColorFromHex("#A0B042"));
                        }
                        else if (m_isBroadcatingGameLoose)
                        {
                            broadcasticons[i].SetIconImage("GameLoose");
                            broadcasticons[i].SetBackgroundColor(Helper.GetColorFromHex("#FF5C01"));
                        }
                        else if (m_LineConnectors != null && m_ListnerTargetNodes?.FindAll(a => a.ListenStrings.Contains(BroadcastingStrings[i])).Count != 0)
                        {
                            broadcasticons[i].SetIconImage("Broadcast");
                            broadcasticons[i].SetBackgroundColor(Helper.GetColorFromHex("#FFFFFF"));
                        }
                        else
                        {
                            broadcasticons[i].SetIconImage("BroadcastNoListner");
                            broadcasticons[i].SetBackgroundColor(Helper.GetColorFromHex("#FF413B"));
                        }
                    }
                    else
                    {
                        broadcasticons[i].Hide();
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
                        lineConnectorIndex++;
                        continue;
                    }

                    Vector3[] points = new Vector3[RESOLUTION + 1];
                    Vector3 startPoint, endPoint;

                    if (!CheckIfInsideScreen(RectTransform) || transform.localScale == Vector3.zero)
                    {
                        startPoint = m_ObjectTarget.ComponentGameObject.transform.position;
                    }
                    else
                    {
                        startPoint = m_MainCamera.ScreenToWorldPoint(broadcasticons[k].transform.position);
                    }
                    if (!CheckIfInsideScreen(allTargets[j].RectTransform) || allTargets[j].transform.localScale == Vector3.zero)
                    {
                        endPoint = allTargets[j].m_ObjectTarget.ComponentGameObject.transform.position;
                    }
                    else
                    {
                        endPoint = m_MainCamera.ScreenToWorldPoint(allTargets[j].ListenIconTransform.position);
                    }

                    Vector3 controlPoint = (startPoint + endPoint) / 2f;
                    controlPoint += Vector3.up * Vector3.Distance(startPoint, endPoint) / 2f;

                    for (int i = 0; i <= RESOLUTION; i++)
                    {
                        float t = (float)i / RESOLUTION;

                        points[i] = CalculateBezierPoint(startPoint, controlPoint, endPoint, t);
                    }
                    m_LineConnectors[lineConnectorIndex].gameObject.SetActive(true);

                    m_LineConnectors[lineConnectorIndex].positionCount = points.Length;
                    m_LineConnectors[lineConnectorIndex].SetPositions(points);


                    Color c = Color.white;
                    if (!EditorOp.Resolve<SelectionHandler>().GetSelectedObjects().Contains(m_ObjectTarget.ComponentGameObject)
                        && !EditorOp.Resolve<SelectionHandler>().GetSelectedObjects().Contains(allTargets[j].m_ObjectTarget.ComponentGameObject))
                    {
                        c.a = GetFadeValue(distanceToTarget, -1, 40);
                        c.a = Mathf.Clamp(c.a, 0.0f, 1.0f);

                    }
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

        void PositionAroundCenterImage(RectTransform imageToPosition, int index, int totalImages, float radius, float totalAngle, bool clockwise)
        {
            float angleIncrement = totalAngle / totalImages;
            float angle = 45.0f - index * angleIncrement;
            angle = (angle + 360.0f) % 360.0f;

            angle = angle * Mathf.Deg2Rad;

            Vector2 offsetPosition = new Vector2(clockwise ? Mathf.Cos(angle) : -Mathf.Cos(angle), Mathf.Sin(angle)).normalized * radius;
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

        private Vector3 CalculateBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;

            Vector3 p = (uu * p0) + (2 * u * t * p1) + (tt * p2);

            return p;
        }

        private bool IsTargetDestroyed()
        {
            return !m_ObjectTarget.ComponentGameObject || (m_ObjectTarget.ComponentGameObject && !m_ObjectTarget.ComponentGameObject.activeSelf);
        }

        private float GetFadeValue(float distance, float min, float max)
        {
            return 1 - ((distance - min) / (max - min));
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
