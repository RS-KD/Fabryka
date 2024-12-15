using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TypeOfPlacedObject : MonoBehaviour
{
    public Type type;
}

public enum Type
{
    OneXOne,
    OneXTwo,
    TwoXTwo
}

