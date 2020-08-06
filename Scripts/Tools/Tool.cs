using Archi.Core.Settings;
using Archi.Core.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Archi.Core.Tools
{
    public abstract class Tool 
    {
        public Texture2D icon;
        public abstract void MouseDown(GameObject tile, Material[] materials, Vector3Int closestCell, Archi archi,bool erase);
        public abstract void MouseUp(GameObject tile, Material[] materials, Vector3Int closestCell, Archi archi,bool erase);
        public abstract void MouseDrag(GameObject tile, Material[] materials, Vector3Int closestCell, Archi archi, bool erase);
        
        public virtual void Preview(Vector3Int closestCell, GridLayout grid){
            Handles.color = MyCustomSettings.GetSerializedSettings().FindProperty("handleColor").colorValue;

            Handles.DrawWireCube(GridUtility.GetCellCenter(closestCell,grid),new Vector3(grid.cellSize.x,0,grid.cellSize.y));
        }

        public Tool(Texture2D i)
        {
            icon = i;
        }
    }
}