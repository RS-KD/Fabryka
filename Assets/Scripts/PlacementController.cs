using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlacementController : MonoBehaviour
{
    PathPlacement pathPlacement;
    [SerializeField] TilePlacement tilePlacement;
    [SerializeField] TMP_Dropdown placementTypeDropdown;
    [SerializeField] TMP_Dropdown pathListDropdown;
    [SerializeField] GameObject pathPrefab;
    public List<GameObject> paths;
    private int pathCount = 1;
    private void Start() 
    {
        NewPath();
    }
    private void Update() 
    {
        switch(placementTypeDropdown.value)
        {
            case 0:
                if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonDown(0))
                {
                    tilePlacement.DestroyObject();
                }
                else if (Input.GetMouseButtonDown(0))
                {
                    tilePlacement.PlaceObject();
                }
                break;
            case 1:
                if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonDown(0))
                {
                    if(paths.Count > 0)
                    {
                        pathPlacement = paths[pathListDropdown.value].GetComponent<PathPlacement>();
                        pathPlacement.DeletePathPosition();
                    }
                }
                else if (Input.GetMouseButtonDown(0))
                {
                    if(paths.Count > 0)
                    {
                        pathPlacement = paths[pathListDropdown.value].GetComponent<PathPlacement>();
                        pathPlacement.AddPathPosition();
                    }
                }
                break;
        }
    }
    public void NewPath()
    {
        paths.Add(Instantiate(pathPrefab));
        pathListDropdown.AddOptions(new List<string>{"Path " + pathCount});
        pathListDropdown.value = paths.Count;
        pathCount += 1;
    }

    public void DeletePath()
    {
        if(paths.Count == 0)
            return;
        else if(paths.Count == 1)
        {
            pathListDropdown.captionText.text = " ";
        }
        Destroy(paths[pathListDropdown.value]);
        paths.Remove(paths[pathListDropdown.value]);
        pathListDropdown.options.RemoveAt(pathListDropdown.value);
    }

    public void DeleteAllPaths()
    {
        foreach(var p in paths)
        {
            Destroy(p);
        }
        pathListDropdown.captionText.text = " ";
        pathListDropdown.ClearOptions();
        paths.Clear();
    }
}
