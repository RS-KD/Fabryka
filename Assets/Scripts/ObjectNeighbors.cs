using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectNeighbors : MonoBehaviour
{
    
    public List<GameObject> neighbors = new();

    public List<GameObject> GetNeighbors(float radius)
    {
        neighbors.Clear();
        var colliders = Physics.OverlapSphere(transform.position, radius);
        foreach (var collider in colliders)
        {
            if (collider.gameObject != gameObject && collider.CompareTag("PlacedObject"))
            {
                neighbors.Add(collider.gameObject);
            }
        }
        return neighbors;
    }
}
