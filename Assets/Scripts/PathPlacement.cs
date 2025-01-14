using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.AI.Navigation;
using UnityEngine.AI;
using UnityEngine.UIElements;


public class PathPlacement : MonoBehaviour
{
    [SerializeField] public LineRenderer lineRenderer;
    private List<Vector3> positions = new();
    private List<GameObject> poleList = new();
    public float gridSize = 1f;
    [SerializeField] private float heightOffset = 1f;
    [SerializeField] LayerMask ignoreMask;
    [SerializeField] GameObject pathPole;
    [SerializeField] private Tilemap tilemap; // Reference to the tilemap
    [SerializeField] private TileBase validTile; // Tile type that represents valid placement areas
    [SerializeField] private NavMeshSurface navMeshSurface ; // NavMeshSurface in the scene
    private NavMeshPath currentPath;
    private List<Vector3> optimizedPoints; // Store the optimized path points

    private void Start()
    {

        tilemap = GameObject.FindGameObjectWithTag("Tilemap").GetComponent<Tilemap>();
        navMeshSurface = GameObject.FindGameObjectWithTag("Tilemap").GetComponent<NavMeshSurface>();
        lineRenderer.material = new Material(Shader.Find("Unlit/ProfileAnalyzerShader"));
        
        Gradient gradient = new Gradient();
        Color col = Random.ColorHSV();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(col, 0.0f), new GradientColorKey(col, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 1.0f), new GradientAlphaKey(1.0f, 1.0f) }
        );
        lineRenderer.colorGradient = gradient;
    }
    public void AddPathPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        int layerMask = ~ignoreMask.value;

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, layerMask))
        {
            if (hit.collider.CompareTag("PathPole") || hit.collider.CompareTag("Wall"))
                return;

            // Snap to the nearest tile
            Vector3 gridPosition = new Vector3(
                Mathf.Round(hit.point.x / gridSize) * gridSize,
                Mathf.Round(hit.point.y / gridSize) * gridSize,
                Mathf.Round(hit.point.z / gridSize) * gridSize
            );

            if (IsPositionOccupied(gridPosition))
            {
                Debug.Log("Position already occupied.");
                return;
            }

            // Add the position
            positions.Add(gridPosition);
            Debug.Log($"Added position: {gridPosition}");
            lineRenderer.positionCount = positions.Count;
            lineRenderer.SetPositions(positions.ToArray());

            // Instantiate a pole
            poleList.Add(Instantiate(pathPole, gridPosition, Quaternion.identity, transform));

            // Generate the path if there are at least two points
            if (positions.Count > 1)
            {
                GeneratePath(positions[0], positions[positions.Count-1]); // First to last position
            }
        }

    }

    private void GeneratePath(Vector3 startPoint, Vector3 endPoint)
    {
        navMeshSurface.BuildNavMesh();

        // Ensure start and end points are valid NavMesh positions
        if (NavMesh.SamplePosition(startPoint, out NavMeshHit startHit, 1.0f, NavMesh.AllAreas) &&
            NavMesh.SamplePosition(endPoint, out NavMeshHit endHit, 1.0f, NavMesh.AllAreas))
        {
            NavMeshPath navPath = new NavMeshPath();
            if (NavMesh.CalculatePath(startHit.position, endHit.position, NavMesh.AllAreas, navPath))
            {
                if (navPath.status == NavMeshPathStatus.PathComplete)
                {
                    DrawPath(navPath);
                }
                else
                {
                    Debug.LogWarning("Path found, but incomplete.");
                }
            }
            else
            {
                Debug.LogWarning("Failed to calculate path.");
            }
        }
        else
        {
            Debug.LogWarning("Start or end point is not on the NavMesh.");
        }
    }

    private void DrawPath(NavMeshPath navPath)
    {
        // Clear existing poles and line
        ClearPreviousPath();

        // Update LineRenderer with NavMesh corners
        for (int i = 0; i < navPath.corners.Length - 1; i++)
        {
            Debug.DrawLine(navPath.corners[i], navPath.corners[i + 1], Color.red, 200f);
        }
        List<Vector3> snappedCorners = new();
        // Place poles at each corner
        foreach (Vector3 corner_B in navPath.corners)
        {
            Debug.Log(corner_B);
            Vector3 corner = new Vector3(Mathf.Round(corner_B.x), Mathf.Round(corner_B.y), Mathf.Round(corner_B.z));
            Debug.Log(new Vector3(Mathf.Round(corner.x), Mathf.Round(corner.y), Mathf.Round(corner.z)));
            Vector3Int cellPosition = tilemap.WorldToCell(corner);
            Debug.Log(cellPosition);
            Vector3 alignedPosition = tilemap.GetCellCenterWorld(cellPosition);
            alignedPosition.y += heightOffset;
            snappedCorners.Add(alignedPosition);
            
            

        }
        for (int i = 0; i < snappedCorners.Count - 1; i++)
        {
            Debug.DrawLine(snappedCorners[i], snappedCorners[i + 1], Color.yellow, 200f);
        }
        snappedCorners = Add90DegreeTurns(snappedCorners);
        for (int i = 0; i < snappedCorners.Count - 1; i++)
        {
            Debug.DrawLine(snappedCorners[i], snappedCorners[i + 1], Color.blue, 200f);
        }
        
        lineRenderer.positionCount = snappedCorners.Count;
        lineRenderer.SetPositions(snappedCorners.ToArray());
        foreach (Vector3 corner in snappedCorners)
        {
            // Instantiate a pole at the corner
            GameObject pole = Instantiate(pathPole, corner, Quaternion.identity, transform);
            poleList.Add(pole);
        }
    }
    private List<Vector3> Add90DegreeTurns(List<Vector3> corners)
    {
        List<Vector3> adjustedCorners = new();

        for (int i = 0; i < corners.Count - 1; i++)
        {
            Vector3 current = corners[i];
            Vector3 next = corners[i + 1];

            // Always add the current point
            if (i == 0 || IsPointValid(current))
            {
                adjustedCorners.Add(current);
            }

            // Check if the points are diagonal (different X and Z coordinates)
            if (current.x != next.x && current.z != next.z)
            {
                // Insert an intermediate point to make a 90° turn
                Vector3 intermediate = new Vector3(next.x, current.y, current.z);

                // Validate path from current to intermediate
                if (!IsPathValid(current, intermediate))
                {
                    intermediate = GenerateValidTurnPoint(current, next);
                }

                // Add the intermediate point if valid
                if (IsPointValid(intermediate) && IsPathValid(current, intermediate))
                {
                    adjustedCorners.Add(intermediate);
                }
            }
        }

        // Always add the last point
        Vector3 last = corners[^1];
        if (IsPointValid(last))
        {
            adjustedCorners.Add(last);
        }

        return adjustedCorners;
    }
    private bool IsPathValid(Vector3 start, Vector3 end)
    {
        navMeshSurface.BuildNavMesh();
        NavMeshPath path = new NavMeshPath();
        if (NavMesh.CalculatePath(start, end, NavMesh.AllAreas, path))
        {
            // Check if the path is complete and does not intersect cutouts
            return path.status == NavMeshPathStatus.PathComplete;
        }
        return false;
    }

    private bool IsPointValid(Vector3 point)
    {
        // Check if the point is on the NavMesh
        return NavMesh.SamplePosition(point, out _, 0.5f, NavMesh.AllAreas);
    }

    private Vector3 GenerateValidTurnPoint(Vector3 current, Vector3 next)
    {
        // Try moving away from the invalid area in four directions
        Vector3[] directions =
        {
        new Vector3(current.x, current.y, next.z), // Turn X, then Z
        new Vector3(next.x, current.y, current.z), // Turn Z, then X
        new Vector3(current.x - (next.x - current.x), current.y, current.z), // Opposite X
        new Vector3(current.x, current.y, current.z - (next.z - current.z))  // Opposite Z
    };

        foreach (Vector3 direction in directions)
        {
            if (IsPointValid(direction) && IsPathValid(current, direction))
            {
                return direction;
            }
        }

        // Fallback: Return the original point (this should rarely happen)
        return current;
    }
    private bool IsPositionOccupied(Vector3 position)
    {
        return positions.Contains(position);
    }
    
    private void ClearPreviousPath()
    {
        // Remove existing poles
        foreach (GameObject pole in poleList)
        {
            Destroy(pole);
        }
        poleList.Clear();
        positions.Remove(positions[positions.Count-1]);
        // Clear the LineRenderer
        lineRenderer.positionCount = 0;
    }

}
