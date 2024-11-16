using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FabricGenerator : MonoBehaviour
{

    [Header("Tilemap and Tiles")]
    public Tilemap tilemap; // Assign your Tilemap in the inspector
    public TileBase floorTile; // Floor tile
    public TileBase wallTile; // Wall tile


    [Header("Factory Size")]
    public int width = 10; // Default width
    public int height = 10; // Default height
    public ShapeType factoryShape = ShapeType.Rectangle; // Shape selection
    public enum ShapeType
    {
        Rectangle
      
    }

    public void GenerateFactory()
    {
        // Clear previous tiles
        tilemap.ClearAllTiles();

        // Generate factory layout based on selected shape
        switch (factoryShape)
        {
            case ShapeType.Rectangle:
                GenerateRectangle();
                break;
 
        }
    }

    private void GenerateRectangle()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height ; y++)
            {
                // Border walls
                if (x == 0 || y == 0 || x == width - 1 || y == height - 1)
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), wallTile);
                }
                else
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), floorTile);
                }
            }
        }
    }



    
    public void ClearFactory()
    {
        tilemap.ClearAllTiles();
    }


}
