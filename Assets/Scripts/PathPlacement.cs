using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.AI.Navigation;
using UnityEngine.AI;
using UnityEngine.UIElements;
using UnityEngine.WSA;


public class PathPlacement : MonoBehaviour
{
    [SerializeField] public LineRenderer lineRenderer;
    private List<Vector3> positions = new();
    private List<GameObject> poleList = new();
    private List<GameObject> TileList = new ();
    public float gridSize = 1f;
    [SerializeField] private float heightOffset = 1f;
    [SerializeField] LayerMask ignoreMask;
    [SerializeField] GameObject pathPole;
    [SerializeField] private Tilemap tilemap; 
    [SerializeField] private TileBase validTile; 
    [SerializeField] private NavMeshSurface navMeshSurface ;
    [SerializeField] GameObject pathTilePrefab;
    private NavMeshPath currentPath;
    private List<Vector3> optimizedPoints; 
    Color col;
    private void Start()
    {

        tilemap = GameObject.FindGameObjectWithTag("Tilemap").GetComponent<Tilemap>();
        navMeshSurface = GameObject.FindGameObjectWithTag("Tilemap").GetComponent<NavMeshSurface>();
        lineRenderer.material = new Material(Shader.Find("Unlit/ProfileAnalyzerShader"));

        Gradient gradient = new Gradient();
        col = Random.ColorHSV();
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

            positions.Add(gridPosition);
            Debug.Log($"Added position: {gridPosition}");
            lineRenderer.positionCount = positions.Count;
            lineRenderer.SetPositions(positions.ToArray());

            poleList.Add(Instantiate(pathPole, gridPosition, Quaternion.identity, transform));

            
            if (positions.Count > 1)
            {
                GeneratePath(positions[0], positions[positions.Count-1]); 
            }
        }

    }

    private void GeneratePath(Vector3 startPoint, Vector3 endPoint)
    {
        navMeshSurface.BuildNavMesh();

        
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
        
        ClearPreviousPath();

    
        for (int i = 0; i < navPath.corners.Length - 1; i++)
        {
            Debug.DrawLine(navPath.corners[i], navPath.corners[i + 1], Color.red, 200f);
        }
        List<Vector3> snappedCorners = new();
       
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
        PlaceTilesAlongPath(snappedCorners);
        for (int i = 0; i < snappedCorners.Count - 1; i++)
        {
            Debug.DrawLine(snappedCorners[i], snappedCorners[i + 1], Color.blue, 200f);
        }
        
        lineRenderer.positionCount = snappedCorners.Count;
        lineRenderer.SetPositions(snappedCorners.ToArray());
        foreach (Vector3 corner in snappedCorners)
        {
            
            GameObject pole = Instantiate(pathPole, corner, Quaternion.identity, transform);
            pole.GetComponent<Renderer>().material.color = col;
            poleList.Add(pole);
        }
    }
    private void PlaceTilesAlongPath(List<Vector3> path)
    {
        for (int i = 0; i < path.Count - 1; i++)
        {
            Vector3 start = path[i];
            Vector3 end = path[i + 1];
            Vector3 direction = (end - start).normalized;
            
            foreach (Vector3 tilePosition in GetTilePositions(start, end))
            {
                Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
                GameObject Tile = Instantiate(pathTilePrefab, tilePosition, rotation, transform);
                Tile.GetComponent<MeshRenderer>().materials[1].color = col;
             
                if (i > 0 && tilePosition == start)
                {
                    
                    Vector3 adjustment = -direction * tilemap.cellSize.x * -0.32f;
                    Tile.transform.position += adjustment;
                }
                if (i < path.Count - 2 && tilePosition == end)
                {
                   
                    Vector3 adjustment = direction * tilemap.cellSize.x * -0.80f;
                    Tile.transform.position += adjustment;
                }
                TileList.Add(Tile);
            }
        }
    }

    private IEnumerable<Vector3> GetTilePositions(Vector3 start, Vector3 end)
    {
        List<Vector3> tilePositions = new();

        
        Vector3 direction = (end - start).normalized;

       
        float distance = Vector3.Distance(start, end);
        int numTiles = Mathf.CeilToInt(distance / tilemap.cellSize.x);

        for (int i = 0; i <= numTiles; i++)
        {
            Vector3 tilePosition = start + direction * i * tilemap.cellSize.x;
            tilePositions.Add(new Vector3(
                Mathf.Round(tilePosition.x),
                Mathf.Round(tilePosition.y),
                Mathf.Round(tilePosition.z)
            ));
        }

        return tilePositions;
    }

    private void ClearPreviousPath()
    {
       
        foreach (GameObject pole in poleList)
        {
            Destroy(pole);
        }
        foreach (GameObject child in TileList)
        {
            Destroy(child);
        }
        poleList.Clear();

        
        lineRenderer.positionCount = 0;

        
        
    }
    private List<Vector3> Add90DegreeTurns(List<Vector3> corners)
    {
        List<Vector3> adjustedCorners = new();

        for (int i = 0; i < corners.Count - 1; i++)
        {
            Vector3 current = corners[i];
            Vector3 next = corners[i + 1];

            
            if (i == 0 || IsPointValid(current))
            {
                adjustedCorners.Add(current);
            }

            
            if (current.x != next.x && current.z != next.z)
            {
              
                Vector3 intermediate = new Vector3(next.x, current.y, current.z);

                
                if (!IsPathValid(current, intermediate))
                {
                    intermediate = GenerateValidTurnPoint(current, next);
                }

                
                if (IsPointValid(intermediate) && IsPathValid(current, intermediate))
                {
                    adjustedCorners.Add(intermediate);
                }
            }
        }

        
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
            
            return path.status == NavMeshPathStatus.PathComplete;
        }
        return false;
    }

    private bool IsPointValid(Vector3 point)
    {
     
        return NavMesh.SamplePosition(point, out _, 0.5f, NavMesh.AllAreas);
    }

    private Vector3 GenerateValidTurnPoint(Vector3 current, Vector3 next)
    {
     
        Vector3[] directions =
        {
        new Vector3(current.x, current.y, next.z), 
        new Vector3(next.x, current.y, current.z), 
        new Vector3(current.x - (next.x - current.x), current.y, current.z), 
        new Vector3(current.x, current.y, current.z - (next.z - current.z))  
    };

        foreach (Vector3 direction in directions)
        {
            if (IsPointValid(direction) && IsPathValid(current, direction))
            {
                return direction;
            }
        }

    
        return current;
    }
    private bool IsPositionOccupied(Vector3 position)
    {
        return positions.Contains(position);
    }
    
    
}
