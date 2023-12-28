using UnityEngine;
using System.Collections.Generic;

public class MassSpringCloth : MonoBehaviour {
    class MassPoint {
        public Vector3 position;
        public Vector3 velocity;
        public bool isConstrained;

        public MassPoint(Vector3 pos, bool _isConstrained = false) {
            position = pos;
            velocity = Vector3.zero;
            isConstrained = _isConstrained;
        }
    }

    class Spring {
        public MassPoint pointA;
        public MassPoint pointB;
        public float restLength;
        public float maxLength;

        public Spring(MassPoint a, MassPoint b) {
            pointA = a;
            pointB = b;
            restLength = Vector3.Distance(a.position, b.position);
        }
    }

    List<MassPoint> massPoints = new List<MassPoint>();
    List<Spring> springs = new List<Spring>();

    public int width = 10;
    public int height = 10;
    private Mesh mesh;

    public VectorFieldGrid vectorField;

    [Range(0.1f, 2.0f)]
    public float elasticity = 1.0f;

    [Range(0.1f, 2.0f)]
    public float stiffness = 1.0f;

    public Vector3 gravity = new Vector3(0, -9.81f, 0);

    void Start() {
        mesh = CreatePlaneMesh(width, height);

        InitializeMassPoints();
        InitializeSprings();
    }

    void Update() {
        Integrate();
        UpdateMesh();
    }
    
    void FixedUpdate() {
        ApplyForces();
    }

    Mesh CreatePlaneMesh(int width, int height) {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[width * height];
        float halfWidth = width / 2.0f;
        float halfHeight = height / 2.0f;

        for (int i = 0, y = 0; y < height; y++) {
            for (int x = 0; x < width; x++, i++) {
                vertices[i] = new Vector3(x - halfWidth, 0, y - halfHeight);
            }
        }

        int[] triangles = new int[(width - 1) * (height - 1) * 6];
        for (int ti = 0, vi = 0, y = 0; y < height - 1; y++, vi++) {
            for (int x = 0; x < width - 1; x++, ti += 6, vi++) {
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + width;
                triangles[ti + 5] = vi + width + 1;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;

        return mesh;
    }

    void InitializeMassPoints() {
        Vector3[] vertices = mesh.vertices;

        for (int i = 0; i < width * height; i++) {
            Vector3 worldPos = transform.TransformPoint(vertices[i]);
            bool isConstrained = (i < width);
            massPoints.Add(new MassPoint(worldPos, isConstrained));
        }
    }

    void InitializeSprings() {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                int index = x * height + y;
                MassPoint currentPoint = massPoints[index];

                if (x < width - 1) {
                    MassPoint rightPoint = massPoints[index + height];
                    springs.Add(new Spring(currentPoint, rightPoint));
                }

                if (y < height - 1) {
                    MassPoint downPoint = massPoints[index + 1];
                    springs.Add(new Spring(currentPoint, downPoint));
                }

                if (x < width - 1 && y < height - 1) {
                    MassPoint diagonalPoint = massPoints[index + height + 1];
                    springs.Add(new Spring(currentPoint, diagonalPoint));
                }
            }
        }

        foreach (var spring in springs) {
            spring.maxLength = spring.restLength * elasticity;
        }
    }

    void ApplyForces() {
        foreach (var point in massPoints) {
            point.velocity += gravity * Time.deltaTime;

            if (!vectorField.isOutside(point.position)) {
                point.velocity += vectorField.GetClosestForce(point.position) * Time.deltaTime;
            }
        }

        foreach (var spring in springs) {
            Vector3 springForce = CalculateSpringForce(spring);
            spring.pointA.velocity += springForce * Time.deltaTime;
            spring.pointB.velocity -= springForce * Time.deltaTime;
        }
    }

    Vector3 CalculateSpringForce(Spring spring) {
        Vector3 springVector = spring.pointB.position - spring.pointA.position;
        float currentLength = springVector.magnitude;

        if (currentLength > spring.maxLength) {
            currentLength = spring.maxLength;
        }

        float stretch = currentLength - spring.restLength;
        Vector3 forceDirection = springVector.normalized;

        return forceDirection * stretch * stiffness;
    }

    void Integrate() {
        foreach (var point in massPoints) {
            if (point.isConstrained) continue;
            point.position += point.velocity * Time.deltaTime;
        }
    }

    void UpdateMesh() {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = new Vector3[massPoints.Count];

        for (int i = 0; i < massPoints.Count; i++) {
            vertices[i] = transform.InverseTransformPoint(massPoints[i].position);
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
    }

}
