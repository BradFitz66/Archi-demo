using Archi.Core.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Archi.Core.Utils;
public class Brush : Tool
{
    /// <summary>
    /// Called when MouseDown event is fired. 
    /// </summary>
    /// <param name="tile"> the tile we're placing </param>
    /// <param name="materials"> array of materials to apply to tile </param>
    /// <param name="closestCell"> closest cell to mouse position</param>
    /// <param name="archi"> reference to the Archi script </param>
    public override void MouseDown(GameObject tile, Material[] materials, Vector3Int closestCell, Archi.Core.Archi archi,bool erase)
    {
    }

    /// <summary>
    /// Called when MouseUp event is fired. 
    /// </summary>
    /// <param name="tile"> the tile we're placing </param>
    /// <param name="materials"> array of materials to apply to tile </param>
    /// <param name="closestCell"> closest cell to mouse position</param>
    /// <param name="archi"> reference to the Archi script </param>
    public override void MouseUp(GameObject tile, Material[] materials, Vector3Int closestCell, Archi.Core.Archi archi,bool erase) 
    {
        if (!archi.tiles.ContainsKey(closestCell) && !erase)
            archi.PlaceTile(closestCell, GridUtility.GetCellCenter(closestCell,archi.grid), tile);
        else if (erase && archi.tiles.ContainsKey(closestCell))
            archi.RemoveTile(closestCell);
    }
    /// <summary>
    /// Called when MouseDrag event is fired. 
    /// </summary>
    /// <param name="tile"> the tile we're placing </param>
    /// <param name="materials"> array of materials to apply to tile </param>
    /// <param name="closestCell"> closest cell to mouse position</param>
    /// <param name="archi"> reference to the Archi script </param>
    public override void MouseDrag(GameObject tile, Material[] materials, Vector3Int closestCell, Archi.Core.Archi archi, bool erase) 
    {
        if (!archi.tiles.ContainsKey(closestCell) && !erase)
            archi.PlaceTile(closestCell, GridUtility.GetCellCenter(closestCell, archi.grid), tile);
        else if (erase && archi.tiles.ContainsKey(closestCell))
            archi.RemoveTile(closestCell);
    }

    public Brush(Texture2D i) : base(i) {}
    
}
