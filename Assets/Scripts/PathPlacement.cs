using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathPlacement : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    private List<Vector3> positions = new();
    public float gridSize = 1f;
    [SerializeField] LayerMask ignoreMask;
    [SerializeField] GameObject pathPole;

    private List<GameObject> poleList = new();

    public void AddPathPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        int layerMask = ~ignoreMask.value;
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, layerMask))
        {
            if(hit.collider.CompareTag("PathPole") || hit.collider.CompareTag("Wall"))
                return;
            // Znajdź punkt na tilemapie
            Vector3 hitPoint = hit.point;

            // Zaokrąglij współrzędne do najbliższego kafelka
            Vector3 gridPosition = new Vector3(
                Mathf.Round(hitPoint.x / gridSize) * gridSize,
                Mathf.Round(hitPoint.y / gridSize) * gridSize ,
                Mathf.Round(hitPoint.z / gridSize) * gridSize
            );
            if (IsPositionOccupied(gridPosition))
            {
                Debug.Log("Obiekt już istnieje na tym kafelku.");
                return;
            }
            // Umieść obiekt na tej pozycji
            positions.Add(gridPosition);
            lineRenderer.positionCount = positions.Count;
            lineRenderer.SetPositions(positions.ToArray());
            poleList.Add(Instantiate(pathPole, gridPosition, Quaternion.identity, transform));
        }
    }

    bool IsPositionOccupied(Vector3 position)
    {
        if(positions.Contains(position))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public void DeletePathPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        int layerMask = ~ignoreMask.value;
        if(Physics.Raycast(ray, out RaycastHit hit, 1000f, layerMask))
        {
            Vector3 hitPoint = hit.point;
            Vector3 gridPosition = new Vector3(
                Mathf.Round(hitPoint.x / gridSize) * gridSize,
                Mathf.Round(hitPoint.y / gridSize) * gridSize,
                Mathf.Round(hitPoint.z / gridSize) * gridSize
            );
            if (IsPositionOccupied(gridPosition))
            {
                var pos = positions.FindIndex(i => i == gridPosition);
                Destroy(poleList[pos]);
                poleList.RemoveAt(pos);
                positions.Remove(gridPosition);
                lineRenderer.positionCount = positions.Count;
                lineRenderer.SetPositions(positions.ToArray());
            
            }
        }
    }
}
