using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.Tilemaps;
using static FabricGenerator;
public enum PlacableType
{
    Cube,
    B_Cube,
    Rectangle,

}
public class TilePlacement : MonoBehaviour
{
    public GameObject []objectsToPlace; // Prefab obiektu do umieszczenia
    public GameObject cellindicator;
    public PlacableType ObjectyShape = PlacableType.Cube; // Shape selection
    public CameraController CamCont;
    public Grid grid;
    public float gridSize = 1f; // Wielkość pojedynczego kafelka na tilemapie

    public List<GameObject> tiles = new();

    public void DestroyAllObejcts()
    {
        foreach(var t in tiles)
        {
            Destroy(t);
        }
        tiles.Clear();

    }

    public void changeTypeObject(Int32 obj)
    {        
       ObjectyShape = (PlacableType)obj;
    } 
    void Update()
    {
        IndicatorMove();
       
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
                    tiles.Remove(collider.gameObject);
                    Destroy(collider.gameObject);
                    
                }
            }
        }
    }
    private void IndicatorMove()
    {
        Vector3 mousePos = CamCont.GetSelectedMapPosition();

        Vector3Int gridPos = grid.WorldToCell(mousePos);

        cellindicator.transform.localPosition = grid.CellToWorld(gridPos) + new Vector3(0f, 0.51f, 0f);
    }

    public void PlaceObject()
    {
        // Rzutuj promień z kamery w miejsce kliknięcia myszą
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if(hit.collider.CompareTag("PlacedObject") || hit.collider.CompareTag("Wall"))
                return;
                 /* (ObjectyShape)
                 {
                     case PlacableType.Cube:
                         break;
                     case PlacableType.B_Cube:
                         break;
                     case PlacableType.Rectangle:
                         break;
                     default:
                         break;
                 }*/
                // Znajdź punkt na tilemapie
            Vector3 hitPoint = hit.point;

            // Zaokrąglij współrzędne do najbliższego kafelka
            Vector3 gridPosition = new Vector3(
                Mathf.Round(hitPoint.x / gridSize) * gridSize,
                Mathf.Round(hitPoint.y / gridSize) * gridSize,
                Mathf.Round(hitPoint.z / gridSize) * gridSize
            );
            if (IsPositionOccupied(gridPosition))
            {
                Debug.Log("Obiekt już istnieje na tym kafelku.");
                return;
            }
            // Select and place the object based on its type
            GameObject prefabToPlace = null;
            Vector3 offset = Vector3.zero;

            switch (ObjectyShape)
            {
                case PlacableType.Cube:
                    prefabToPlace = objectsToPlace[0];
                    offset = Vector3.zero; // No offset for a 1x1 cube
                    break;

                case PlacableType.B_Cube:
                    prefabToPlace = objectsToPlace[1];
                    offset = new Vector3(gridSize / 2, 0, gridSize / 2); // Align to 2x2 grid
                    break;

                case PlacableType.Rectangle:
                    prefabToPlace = objectsToPlace[2];
                    offset = new Vector3(0, 0, gridSize / 2); // Align to 2x1 grid
                    break;

                default:
                    Debug.LogError("Invalid PlacableType selected!");
                    return;
            }

            Vector3 placementPosition = gridPosition + offset;

            if (IsAreaOccupied(placementPosition, ObjectyShape))
            {
                Debug.Log("Nie można umieścić obiektu. Obszar jest zajęty.");
                return;
            }
            Debug.LogError(placementPosition);
            // Instantiate the object
            tiles.Add(Instantiate(prefabToPlace, placementPosition, Quaternion.identity));

        }

    }
    public bool IsAreaOccupied(Vector3 position, PlacableType type, float sizeMod = 1f)
    {
        Vector3 checkSize = Vector3.one * gridSize; // Default size for Cube
        
        if (type == PlacableType.B_Cube)
            checkSize = new Vector3(gridSize * 2, 0, gridSize * 2); // 2x2 size

        if (type == PlacableType.Rectangle)
            checkSize = new Vector3(gridSize, 0, gridSize * 2); // 2x1 size
        
        Collider[] colliders = Physics.OverlapBox(position, new Vector3(checkSize.x + sizeMod, checkSize.y, checkSize.z + sizeMod) / 4 , Quaternion.identity);

        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("PlacedObject"))
            {
                Debug.Log(collider.name);
                return true; // Znaleziono obiekt z odpowiednim tagiem
            }
        }
        return false;
    }
   
    public bool IsPositionOccupied(Vector3 position)
    {
        // Użyj OverlapSphere, aby znaleźć obiekty w pobliżu pozycji
        Collider[] colliders = Physics.OverlapSphere(position, 0.1f);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("PlacedObject"))
            {
                Debug.Log(collider.name);
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
