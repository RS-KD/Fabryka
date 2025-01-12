using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class OptimizeMachines : MonoBehaviour
{
    [SerializeField] private List<GameObject> floor = new();
    [SerializeField] private List<GameObject> machines = new();
    [SerializeField] private TilePlacement tilePlacement;

    [SerializeField] private GameObject oneXOne;
    [SerializeField] private GameObject oneXTwo;
    [SerializeField] private GameObject twoXTwo;

    [SerializeField] TMP_Dropdown numberDropdown;

    private IEnumerator Opt()
    {
        List<(GameObject, PlacableType, Vector3, List<(Type, float, int)>)> newMachinesList = new();
        var gridSize = tilePlacement.gridSize;
        foreach (var machine in machines)
        {
            var _type = machine.GetComponent<TypeOfPlacedObject>();
            var newMachinePrefab = oneXOne;
            var placableType = PlacableType.Cube;
            var offset = Vector3.zero;

            switch (_type.type)
            {
                case Type.OneXTwo:
                    placableType = PlacableType.Rectangle;
                    newMachinePrefab = oneXTwo;
                    offset = new Vector3(0, 0, gridSize / 2);
                    break;
                case Type.TwoXTwo:
                    placableType = PlacableType.B_Cube;
                    newMachinePrefab = twoXTwo;
                    offset = new Vector3(gridSize / 2, 0, gridSize / 2);
                    break;
            }
            var neighbors = machine.GetComponent<ObjectNeighbors>().GetNeighbors(10f);
            var neighborList = new List<(Type, float, int)>();
            foreach(var n in neighbors)
            {
                var neighborType = n.GetComponent<TypeOfPlacedObject>().type;
                var neighborDistance = Vector3.Distance(machine.transform.position, n.transform.position);
                var neighborIndex = n.GetComponent<ObjectIndex>().index;
                neighborList.Add((neighborType, neighborDistance, neighborIndex));
            }

            newMachinesList.Add((newMachinePrefab, placableType, offset, neighborList));
            Destroy(machine);
        }

        // Resetowanie siatki
        tilePlacement.DestroyAllObejcts();
        machines.Clear();
        yield return new WaitForFixedUpdate();

        foreach(var machineTuple in newMachinesList)
        {
            foreach(var tile in floor)
            {
                var newPos = new Vector3(tile.transform.position.x, tile.transform.position.y + 1, tile.transform.position.z) + machineTuple.Item3;
                float gridSpaceBetween = 4f;
                switch(numberDropdown.value)
                {
                    case 0:
                        gridSpaceBetween = 4f;
                        break;
                    case 1:
                        gridSpaceBetween = 8f;
                        break;
                    case 2:
                        gridSpaceBetween = 12f;
                        break;
                    case 3:
                        gridSpaceBetween = 16f;
                        break;

                }

                if(!tilePlacement.IsPositionOccupied(newPos) && !tilePlacement.IsAreaOccupied(newPos, machineTuple.Item2, gridSpaceBetween))
                {
                    var newMachine = Instantiate(machineTuple.Item1, newPos, Quaternion.identity);
                    tilePlacement.tiles.Add(newMachine);
                    break;
                }
            }
        }
    }

    public void Optimize()
    {
        GetFloor();
        GetAllMachines();
        StartCoroutine(Opt());
    }

    private void GetFloor()
    {
        floor = GameObject.FindGameObjectsWithTag("Floor").ToList();
    }

    private void GetAllMachines()
    {
        machines = GameObject.FindGameObjectsWithTag("PlacedObject").ToList();
    }
}