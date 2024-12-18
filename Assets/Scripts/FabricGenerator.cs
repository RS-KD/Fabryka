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
        Rectangle,
        L,
       Plus

    }
    public void changeWidth(string W)
    {
        if(W == "")
        {
            width = 1;
        }
        else
        {
            width = int.Parse(W);
        }
        
    }
    public void changeHeight(string H)
    {
        if (H == "")
        {
            height = 1;
        }
        else
        {
            height = int.Parse(H);
        }
    }
    public void GenerateFactory()
    {
        // Clear previous tiles
        tilemap.ClearAllTiles();

        // Generate factory layout based on selected shape
        switch (factoryShape)
        {
            case ShapeType.Rectangle:
                GenerateRectangle(false, false);
                break;
            case ShapeType.L:
                GenerateLShape(false, false);
                break;
            case ShapeType.Plus:
                GeneratePlusShape(false, false);
                break;

        }
    }

    private void GenerateRectangle(bool addEntrance, bool addExit)
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
        AddEntranceAndExit(addEntrance, addExit);
    }
    private void GenerateLShape(bool addEntrance, bool addExit)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Create walls along the borders
                if (x == 0 || y == 0 ||
                    ((x == width - 1 && y <= height / 2) || (y == height - 1 && x <= width / 2)) ||
                    (x >= width / 2 && y == height / 2) ||  // Horizontal wall forming the L
                    (x == width / 2 && y >= height / 2))    // Vertical wall forming the L
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), wallTile);
                }
                // Create floor tiles in the L-shaped area
                else if (!(x > width / 2 && y > height / 2))  // Exclude bottom-right cut-out area
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), floorTile);
                }
            }
        }

        // Add entrance and exit if requested
        AddEntranceAndExit(addEntrance, addExit);
    }
    private void GeneratePlusShape(bool addEntrance, bool addExit)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Create the plus shape with walls and empty spaces around it
                if (
                    (y >= height / 2 - (height / 5) && y <= height / 2 + 2) ||  // Horizontal line of the plus
                    (x >= width / 2 - (width / 5) && x <= width / 2 + 2)       // Vertical line of the plus
                )
                {
                   
                    var numcut = width / 3 +1;
                    var numcut2 = height / 3;
                    // Walls along the borders of the plus shape
                    if (x == width / 2 - (width / 5) || x == width / 2 + 2 || (y == numcut2&& x <= numcut) || (y == numcut2*2&& x >= numcut*2))
                    {
                        tilemap.SetTile(new Vector3Int(x, y, 0), wallTile);
                    }
                    // Floor inside the plus shape
                    else
                    {
                        tilemap.SetTile(new Vector3Int(x, y, 0), floorTile);
                    }
                }
                else
                {
                    // Empty spaces outside the plus shape
                    tilemap.SetTile(new Vector3Int(x, y, 0), null);
                }
            }
        }

        // Add entrance and exit if requested
        AddEntranceAndExit(addEntrance, addExit);
    }
    private void AddEntranceAndExit(bool addEntrance, bool addExit)
    {
        if (addEntrance)
        {
            tilemap.SetTile(new Vector3Int(0, height / 2, 0), floorTile);
        }
        if (addExit)
        {
            tilemap.SetTile(new Vector3Int(width - 1, height / 2, 0), floorTile);
        }
    }

    public void ClearFactory()
    {
        tilemap.ClearAllTiles();
    }


}
