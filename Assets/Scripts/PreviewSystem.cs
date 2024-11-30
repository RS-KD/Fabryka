using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PreviewSystem : MonoBehaviour
{
    [SerializeField]
    private float previewYOffset = 0.06f;
    [SerializeField]
    private GameObject cellIndicator;
    private GameObject previewObject;
    [SerializeField]
    private Material previewMaterialPref;
    private Material previewMaterialInst;
    private void Start()
    {
        previewMaterialInst = new Material(previewMaterialPref);
        cellIndicator.SetActive(false);
    }
    public void StartSPrev(GameObject prefab , Vector2Int size)
    {
        previewObject = Instantiate(prefab);
        Prepareprev(previewObject);
        PrepareCursor(size);
    }

    private void PrepareCursor(Vector2Int size)
    {
        if(size.x > 0 || size.y > 0)
        {
            cellIndicator.transform.localScale = new Vector3(size.x, 1, size.y);
            cellIndicator.GetComponent<Renderer>().material.mainTextureScale = size;
        }
    }

    private void Prepareprev(GameObject previewObject)
    {
        Renderer[] renderers = previewObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers) 
        {
            Material[] materials = renderer.materials;
            for (int i = 0; i < materials.Length; i++) 
            {
                materials[i] = previewMaterialInst;
            }
            renderer.materials = materials;
        }
    }
}
