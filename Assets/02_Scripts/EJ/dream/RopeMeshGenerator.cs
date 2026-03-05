using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

[RequireComponent(typeof(SplineContainer), typeof(MeshFilter), typeof(MeshRenderer))]
public class RopeMeshGenerator : MonoBehaviour
{
    [Header("Rope Shape Settings")]
    [SerializeField] private float radius = 0.1f;     // 로프 두께
    private int radialSegments = 8;
    private int lengthSegments = 50;
    
    void Start()
    {
        SplineGeneration();
    }

    void SplineGeneration()
    {
        var splineContainer = GetComponent<SplineContainer>();
        var spline = splineContainer.Spline;

        Mesh mesh = new Mesh();
        int vertexCount = (radialSegments + 1) * (lengthSegments + 1);
        Vector3[] vertices = new Vector3[vertexCount];
        Vector3[] normals = new Vector3[vertexCount];
        Vector2[] uvs = new Vector2[vertexCount];
        int[] triangles = new int[radialSegments * lengthSegments * 6];

        for (int i = 0; i <= lengthSegments; i++)
        {
            float t = (float)i / lengthSegments;
            Vector3 center = spline.EvaluatePosition(t);
            float3 tangent3 = spline.EvaluateTangent(t);
            Vector3 tangent = ((Vector3)tangent3).normalized;
            Vector3 normal = Vector3.up;

            if (Vector3.Dot(tangent, normal) > 0.99f)
                normal = Vector3.forward;

            Vector3 bitangent = Vector3.Cross(tangent, normal).normalized;
            normal = Vector3.Cross(bitangent, tangent).normalized;

            for (int j = 0; j <= radialSegments; j++)
            {
                float angle = (float)j / radialSegments * Mathf.PI * 2f;
                Vector3 radialOffset = (Mathf.Cos(angle) * normal + Mathf.Sin(angle) * bitangent) * radius;
                int index = i * (radialSegments + 1) + j;
                vertices[index] = center + radialOffset;
                normals[index] = radialOffset.normalized;
                uvs[index] = new Vector2((float)j / radialSegments, t);
            }
        }

        int triIndex = 0;
        for (int i = 0; i < lengthSegments; i++)
        {
            for (int j = 0; j < radialSegments; j++)
            {
                int a = i * (radialSegments + 1) + j;
                int b = a + radialSegments + 1;

                triangles[triIndex++] = a;
                triangles[triIndex++] = b;
                triangles[triIndex++] = a + 1;

                triangles[triIndex++] = b;
                triangles[triIndex++] = b + 1;
                triangles[triIndex++] = a + 1;
            }
        }

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();

        GetComponent<MeshFilter>().mesh = mesh;

        var renderer = GetComponent<MeshRenderer>();
        if (renderer.sharedMaterial == null)
        {
            renderer.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            renderer.sharedMaterial.color = new Color(0.7f, 0.6f, 0.4f);
        }
    }
}
