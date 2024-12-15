using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class OptimizeMachines : MonoBehaviour
{
    [SerializeField] private List<GameObject> floor = new();
    [SerializeField] private List<GameObject> machines = new();
    [SerializeField] private TilePlacement tilePlacement;

    [SerializeField] private GameObject oneXOne;
    [SerializeField] private GameObject oneXTwo;
    [SerializeField] private GameObject twoXTwo;

    private IEnumerator Opt()
    {
        List<(GameObject, PlacableType, Vector3)> newMachinesList = new();
        var gridSize = tilePlacement.gridSize;
        foreach(var machine in machines)
        {
            var _type = machine.GetComponent<TypeOfPlacedObject>();
            var newMachinePrefab = oneXOne;
            var placableType = PlacableType.Cube;
            var offset = Vector3.zero;
            switch(_type.type)
            {
                case Type.OneXTwo:
                    placableType = PlacableType.Rectangle;
                    newMachinePrefab = oneXTwo;
                    offset = new Vector3(0, 0, gridSize / 2); // Align to 2x1 grid
                    break;
                case Type.TwoXTwo:
                    placableType = PlacableType.B_Cube;
                    newMachinePrefab = twoXTwo;
                    offset = new Vector3(gridSize / 2, 0, gridSize / 2); // Align to 2x2 grid
                    break;
            }
            Debug.LogWarning(newMachinePrefab);
            newMachinesList.Add((newMachinePrefab, placableType, offset));
            Destroy(machine);
        }
        tilePlacement.DestroyAllObejcts();
        machines.Clear();

        Debug.LogError(tilePlacement.tiles.Count);
        yield return new WaitForFixedUpdate();
        foreach(var machineTuple in newMachinesList)
        {
            foreach(var tile in floor)
            {
                var newPos = new Vector3(tile.transform.position.x, tile.transform.position.y + 1, tile.transform.position.z) + machineTuple.Item3;
                if(!tilePlacement.IsPositionOccupied(newPos) && !tilePlacement.IsAreaOccupied(newPos, machineTuple.Item2, 16f))
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
