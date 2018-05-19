using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PCG;

/// <summary>
/// This class attaches the Cell script to Unity to access the information.
/// </summary>
public class Tile : MonoBehaviour
{
    private Cell _cell; public Cell Cell { get { return _cell; } }

    public void Init(Cell cell)
    {
        _cell = cell;
    }
}
