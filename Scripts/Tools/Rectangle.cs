using Archi.Core.Tools;
using Archi.Core.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class Rectangle : Tool
{

    Vector3 mouseStart;
    Vector3 mouseEnd;

    /// <summary>
    /// Called when MouseDown event is fired. 
    /// </summary>
    /// <param name="tile"> the tile we're placing </param>
    /// <param name="materials"> array of materials to apply to tile </param>
    /// <param name="closestCell"> closest cell to mouse position</param>
    /// <param name="archi"> reference to the Archi script </param>
    public override void MouseDown(GameObject tile, Material[] materials, Vector3Int closestCell, Archi.Core.Archi archi)
    {
        mouseStart = GridUtility.GetCellCenter(closestCell,archi.grid);
        Debug.Log("Mouse down");
    }

    /// <summary>
    /// Called when MouseUp event is fired. 
    /// </summary>
    /// <param name="tile"> the tile we're placing </param>
    /// <param name="materials"> array of materials to apply to tile </param>
    /// <param name="closestCell"> closest cell to mouse position</param>
    /// <param name="archi"> reference to the Archi script </param>
    public override void MouseUp(GameObject tile, Material[] materials, Vector3Int closestCell, Archi.Core.Archi archi) //man I really need a better naming convention...
    {
        Debug.Log("Mouse up");
        mouseEnd = GridUtility.GetCellCenter(closestCell, archi.grid);
        Vector3 size = (mouseStart - mouseEnd);
        Bounds bounds = new Bounds(mouseStart-size/2, vecAbs(size));
        
        int absXMin = Mathf.Abs((int)bounds.min.x);
        int absXMax = Mathf.Abs((int)bounds.max.x);
        int absYMin = Mathf.Abs((int)bounds.min.y);
        int absYMax = Mathf.Abs((int)bounds.max.y);

        for (int x = (int)bounds.min.x; x <= (int)bounds.max.x; x++)
        {

            for (int y = (int)bounds.min.z; y <= (int)bounds.max.z; y++)
            {
                if (x == (int)bounds.min.x || x == (int)bounds.max.x) {
                    Vector3Int cell = archi.grid.WorldToCell(new Vector3(x,0,y));
                    if (!archi.tiles.ContainsKey(cell))
                    {
                        archi.PlaceTile(cell, GridUtility.GetCellCenter(cell, archi.grid), tile);
                    }

                }
                if (y == (int)bounds.min.z || y == (int)bounds.max.z)
                {
                    Vector3Int cell = archi.grid.WorldToCell(new Vector3(x, 0, y));
                    if (!archi.tiles.ContainsKey(cell))
                    {
                        archi.PlaceTile(cell, GridUtility.GetCellCenter(cell, archi.grid), tile);
                    }
                }
            }
        }
    }

    Vector3 vecAbs(Vector3 val)
    {
        return new Vector3(Mathf.Abs(val.x), Mathf.Abs(val.y), Mathf.Abs(val.z));
    }
    /// <summary>
    /// Called when MouseDrag event is fired. 
    /// </summary>
    /// <param name="tile"> the tile we're placing </param>
    /// <param name="materials"> array of materials to apply to tile </param>
    /// <param name="closestCell"> closest cell to mouse position</param>
    /// <param name="archi"> reference to the Archi script </param>
    public override void MouseDrag(GameObject tile, Material[] materials, Vector3Int closestCell, Archi.Core.Archi archi) //man I really need a better naming convention...
    {
    }

    public Rectangle(Texture2D i) : base(i) { }
}
