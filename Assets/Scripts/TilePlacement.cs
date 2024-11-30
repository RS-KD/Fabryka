using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilePlacement : MonoBehaviour
{
    public GameObject objectToPlace; // Prefab obiektu do umieszczenia
    public float gridSize = 1f; // Wielkość pojedynczego kafelka na tilemapie
    private List<GameObject> tiles = new();

    public void DestroyAllObejcts()
    {
        foreach(var t in tiles)
        {
            Destroy(t);
        }
        tiles.Clear();
    }

    public void DestroyObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(ray, out RaycastHit hit))
        {
            Collider[] colliders = Physics.OverlapSphere(hit.point, 0.1f);
            foreach (var collider in colliders)
            {
                if (collider.gameObject.CompareTag("PlacedObject"))
                {
                    Destroy(collider.gameObject);
                }
            }
        }
    }

    public void PlaceObject()
    {
        // Rzutuj promień z kamery w miejsce kliknięcia myszą
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if(hit.collider.CompareTag("PlacedObject") || hit.collider.CompareTag("Wall"))
                return;
            // Znajdź punkt na tilemapie
            Vector3 hitPoint = hit.point;

            // Zaokrąglij współrzędne do najbliższego kafelka
            Vector3 gridPosition = new Vector3(
                Mathf.Round(hitPoint.x / gridSize) * gridSize,
                Mathf.Round(hitPoint.y / gridSize) * gridSize + 1f,
                Mathf.Round(hitPoint.z / gridSize) * gridSize
            );
            if (IsPositionOccupied(gridPosition))
            {
                Debug.Log("Obiekt już istnieje na tym kafelku.");
                return;
            }
            // Umieść obiekt na tej pozycji
            tiles.Add(Instantiate(objectToPlace, gridPosition, Quaternion.identity));
        }
    }
    bool IsPositionOccupied(Vector3 position)
    {
        // Użyj OverlapSphere, aby znaleźć obiekty w pobliżu pozycji
        Collider[] colliders = Physics.OverlapSphere(position, 0.1f);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("PlacedObject"))
            {
                return true; // Znaleziono obiekt z odpowiednim tagiem
            }
        }
        return false;
    }
    // public Tilemap tilemap; // Tilemapa bazowa
    // public GameObject objectToPlace; // Prefab obiektu do umieszczenia
    // public float gridOffsetY = 0f; // Wysokość obiektu nad tilemapą (jeśli potrzebne)


    // private void Update()
    // {
    //     if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonDown(0))
    //     {
    //         Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    //         if(Physics.Raycast(ray, out RaycastHit hit))
    //         {
    //             Collider[] colliders = Physics.OverlapSphere(hit.point, 0.1f);
    //             foreach (var collider in colliders)
    //             {
    //                 if (collider.gameObject.CompareTag("PlacedObject"))
    //                 {
    //                     Destroy(collider.gameObject);
    //                 }
    //             }
    //         }
    //     }
    //     else if (Input.GetMouseButtonDown(0)) // Lewy przycisk myszy
    //     {
    //         PlaceObjectOnTilemap();
    //     }
    // }
    
    // private void PlaceObjectOnTilemap()
    // {
    //     // Rzutuj promień z kamery w miejsce kliknięcia myszą
    //     Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    //     if (Physics.Raycast(ray, out RaycastHit hit))
    //     {
    //         // Znajdź pozycję w tilemapie
    //         Vector3 hitPoint = hit.point;
    //         Vector3Int gridPosition = tilemap.WorldToCell(hitPoint);

    //         // Zamień pozycję siatki na światową
    //         Vector3 worldPosition = tilemap.CellToWorld(gridPosition);
    //         worldPosition.y += gridOffsetY;

    //         // Sprawdź, czy obiekt już istnieje na tym kafelku (opcjonalnie)
    //         if (IsObjectAlreadyPlaced(gridPosition))
    //         {
    //             Debug.Log("Obiekt już istnieje na tym kafelku.");
    //             return;
    //         }

    //         // Umieść obiekt na tilemapie
    //         Instantiate(objectToPlace, worldPosition, Quaternion.identity, tilemap.transform);
    //     }
    // }

    // private bool IsObjectAlreadyPlaced(Vector3Int gridPosition)
    // {
    //     // Przykładowe sprawdzenie - np. tag obiektów prefabrykowanych
    //     Vector3 worldPosition = tilemap.CellToWorld(gridPosition);
    //     Collider[] colliders = Physics.OverlapSphere(worldPosition, 0.1f); // Promień detekcji
    //     foreach (var collider in colliders)
    //     {
    //         if (collider.gameObject.CompareTag("PlacedObject"))
    //         {
    //             return true;
    //         }
    //     }
    //     return false;
    // }
}
