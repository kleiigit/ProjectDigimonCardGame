using UnityEngine;
using ProjectScript.Enums;

public class GridCell : MonoBehaviour
{
    public int gridIndex;
    public bool cellFull = false;
    public GameObject ObjectInCell;
    public PlayerSide owner;
}

