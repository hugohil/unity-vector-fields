using UnityEngine;

public class GridCell {
    public Vector3 position;
    public Vector3 vector;
    public int seed;
}

public class VectorFieldGrid : MonoBehaviour {
    public GridCell[,,] grid;

    [Range(1, 50)]
    public int cellX = 3;
    [Range(1, 50)]
    public int cellY = 3;
    [Range(1, 50)]
    public int cellZ = 3;

    [Range(0.1f, 5.0f)]
    public float scale = 1.0f;
    [Range(0.1f, 5.0f)]
    public float speed = 1.0f;

    void Start() {
        InitializeGrid();
    }

    void FixedUpdate() {
        UpdateGridVectors();
    }

    void InitializeGrid() {
        grid = new GridCell[cellX, cellY, cellZ];
        Debug.Log("Total number of cells: " + cellX * cellY * cellZ);

        for (int x = 0; x < cellX; x++) {
            for (int y = 0; y < cellY; y++) {
                for (int z = 0; z < cellZ; z++) {
                    grid[x, y, z] = new GridCell {
                        position = new Vector3(x - cellX / 2, y - cellY / 2, z - cellZ / 2),
                        vector = Vector3.zero,
                        seed = (int) Random.Range(0, 1000000)
                    };
                }
            }
        }
    }

    void UpdateGridVectors() {
        float time = Time.time * speed;
        for (int x = 0; x < cellX; x++) {
            for (int y = 0; y < cellY; y++) {
                for (int z = 0; z < cellZ; z++) {
                    GridCell cell = grid[x, y, z];

                    Vector3 pos = cell.position;
                    int seed = cell.seed;

                    float noiseX = 2.0f * (0.5f - Mathf.PerlinNoise(pos.x * scale + time + seed, pos.y * scale + time + seed));
                    float noiseY = 2.0f * (0.5f - Mathf.PerlinNoise(pos.y * scale + time + seed, pos.z * scale + time + seed));
                    float noiseZ = 2.0f * (0.5f - Mathf.PerlinNoise(pos.z * scale + time + seed, pos.x * scale + time + seed));

                    cell.vector = new Vector3(noiseX, noiseY, noiseZ);
                }
            }
        }
    }

    public Vector3 GetClosestForce (Vector3 position) {
        float minDistance = float.MaxValue;
        Vector3 closestVector = Vector3.zero;

        for (int x = 0; x < cellX; x++) {
            for (int y = 0; y < cellY; y++) {
                for (int z = 0; z < cellZ; z++) {
                    GridCell cell = grid[x, y, z];
                    float distance = Vector3.Distance(position, cell.position);

                    if (distance < minDistance) {
                        minDistance = distance;
                        closestVector = cell.vector;
                    }
                }
            }
        }
        

        return closestVector;
    }

    public bool isOutside (Vector3 position) {
        return position.x < -cellX / 2 || position.x > cellX / 2 ||
               position.y < -cellY / 2 || position.y > cellY / 2 ||
               position.z < -cellZ / 2 || position.z > cellZ / 2;
    }

    void OnDrawGizmos() {
        if (grid == null) return;

        for (int x = 0; x < cellX; x++) {
            for (int y = 0; y < cellY; y++) {
                for (int z = 0; z < cellZ; z++) {
                    DrawVectorGizmo(grid[x, y, z]);
                }
            }
        }
    }

    void DrawVectorGizmo(GridCell cell) {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(cell.position, Vector3.one);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(cell.position, (cell.position + cell.vector) * scale);
    }
}
