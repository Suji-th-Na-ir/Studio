using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILineRenderer : Graphic
{
    public Color lineColor = Color.white;
    public float lineWidth = 2f;

    private List<Vector3> linePoints = new List<Vector3>();

    public float dotSpacing = 25f;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (linePoints.Count < 2)
            return;

        Vector3 prevPoint = linePoints[0];
        float accumulatedDistance = 0f;

        for (int i = 1; i < linePoints.Count; i++)
        {
            Vector3 currentPoint = linePoints[i];
            float segmentLength = Vector3.Distance(prevPoint, currentPoint);

            Vector3 dir = (currentPoint - prevPoint).normalized;
            Vector3 perpendicular = new Vector2(-dir.y, dir.x);

            float segmentDistance = 0f;

            while (segmentDistance < segmentLength)
            {
                float remainingSegmentLength = segmentLength - segmentDistance;
                float drawLength = Mathf.Min(remainingSegmentLength, dotSpacing);

                Vector3 p1 = prevPoint + dir * segmentDistance - perpendicular * (lineWidth * 0.5f);
                Vector3 p2 = prevPoint + dir * segmentDistance + perpendicular * (lineWidth * 0.5f);
                Vector3 p3 = prevPoint + dir * (segmentDistance + drawLength) + perpendicular * (lineWidth * 0.5f);
                Vector3 p4 = prevPoint + dir * (segmentDistance + drawLength) - perpendicular * (lineWidth * 0.5f);

                vh.AddUIVertexQuad(CreateUIVertexQuad(p1, p2, p3, p4, lineColor));

                segmentDistance += drawLength;
                accumulatedDistance += drawLength;

                // Switch to empty space (transparent) if needed
                if (accumulatedDistance >= dotSpacing)
                {
                    accumulatedDistance = 0f;
                    vh.AddUIVertexQuad(CreateUIVertexQuad(p1, p2, p1, p2, Color.clear));
                }
            }

            prevPoint = currentPoint;
        }
    }

    private UIVertex[] CreateUIVertexQuad(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, Color color)
    {
        UIVertex[] quad = new UIVertex[4];

        for (int i = 0; i < 4; i++)
        {
            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = color;
            quad[i] = vertex;
        }

        quad[0].position = p1;
        quad[1].position = p2;
        quad[2].position = p3;
        quad[3].position = p4;

        return quad;
    }

    public void AddPoint(Vector3 point)
    {
        linePoints.Add(point);
        SetVerticesDirty();
    }

    public void AddPoints(List<Vector3> point)
    {
        linePoints.AddRange(point);
        SetVerticesDirty();
    }

    public void ClearPoints()
    {
        linePoints.Clear();
        SetVerticesDirty();
    }
}
