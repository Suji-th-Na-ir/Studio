using System.Collections.Generic;
using UnityEngine;

namespace Terra.Studio
{
    public class VisulisationLineGenerator
    {
        private Mesh mesh;
        public Mesh Mesh { get { return mesh; } }
        public VisulisationLineGenerator(Transform _transform, float length, float radius, float arrowHeadLength, float arrowHeadRadius, int segments = 36, bool createBothArrowHead = false)
        {
            var cylinder = GenerateCylinder(length, radius, segments);
            var arrow = GenerateArrowHead(length, arrowHeadLength, arrowHeadRadius, segments, true);
            Mesh combinedMesh = new Mesh();

            if (createBothArrowHead)
            {
                var newArrow = GenerateArrowHead(length, arrowHeadLength, arrowHeadRadius, segments, false);
                combinedMesh.CombineMeshes(new CombineInstance[] {
                new CombineInstance { mesh = cylinder, transform = _transform.localToWorldMatrix },
                new CombineInstance { mesh = arrow, transform = _transform.localToWorldMatrix },
                new CombineInstance { mesh = newArrow, transform = _transform.localToWorldMatrix }
            });
            }
            else
            {
                combinedMesh.CombineMeshes(new CombineInstance[] {
                new CombineInstance { mesh = cylinder, transform = _transform.localToWorldMatrix },
                new CombineInstance { mesh = arrow, transform = _transform.localToWorldMatrix }
            });
            }
            mesh = combinedMesh;
        }

        private Mesh GenerateCylinder(float length, float radius, int segments)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();

            // Generate the cylinder body
            for (int i = 0; i <= segments; i++)
            {
                float angle = 2 * Mathf.PI * i / segments;
                float x = Mathf.Cos(angle) * radius;
                float y = Mathf.Sin(angle) * radius;
                vertices.Add(new Vector3(x, y, 0));
                vertices.Add(new Vector3(x, y, length));
            }

            for (int i = 0; i < segments; i++)
            {
                int baseIndex = i * 2;
                int nextBaseIndex = ((i + 1) % segments) * 2;

                triangles.Add(baseIndex);
                triangles.Add(nextBaseIndex + 1);
                triangles.Add(baseIndex + 1);

                triangles.Add(baseIndex);
                triangles.Add(nextBaseIndex);
                triangles.Add(nextBaseIndex + 1);
            }

            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();

            return mesh;
        }



        private Mesh GenerateArrowHead(float length, float arrowHeadLength, float arrowHeadRadius, int segments, bool front)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();

            var tipZ = front ? length + arrowHeadLength : -arrowHeadLength;
            length = front ? length : 0.0f;
            Vector3 tip = new Vector3(0, 0, tipZ);
            int tipIndex = vertices.Count;
            vertices.Add(tip);

            for (int i = 0; i < segments; i++)
            {
                float angle = 2 * Mathf.PI * i / segments;
                float x = Mathf.Cos(angle) * arrowHeadRadius;
                float y = Mathf.Sin(angle) * arrowHeadRadius;
                vertices.Add(new Vector3(x, y, length));
            }

            // Add the center vertex of the base
            int centerVertexIndex = vertices.Count;
            vertices.Add(new Vector3(0, 0, length));

            // Add triangles for the sides
            for (int i = tipIndex + 1; i < centerVertexIndex - 1; i++)
            {
                triangles.Add(tipIndex);
                triangles.Add(i);
                triangles.Add(i + 1);
            }

            triangles.Add(tipIndex);
            triangles.Add(centerVertexIndex - 1);
            triangles.Add(tipIndex + 1);

            // Add triangles for the base
            int j;
            for (j = tipIndex + 1; j < centerVertexIndex - 1; j++)
            {
                triangles.Add(centerVertexIndex);
                triangles.Add(j + 1);
                triangles.Add(j);
            }

            // Add the last triangle for the base
            triangles.Add(centerVertexIndex);
            triangles.Add(tipIndex + 1);
            triangles.Add(centerVertexIndex - 1);


            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();

            return mesh;
        }

    }
}