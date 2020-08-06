using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Archi.Core.Tools
{
    public abstract class Tool 
    {
        public Texture2D icon;
        public abstract void MouseDown(GameObject tile, Material[] materials, Vector3Int closestCell, Archi archi);
        public abstract void MouseUp(GameObject tile, Material[] materials, Vector3Int closestCell, Archi archi);
        public abstract void MouseDrag(GameObject tile, Material[] materials, Vector3Int closestCell, Archi archi);
        
        protected Tool(Texture2D i)
        {
            icon = i;
        }
    }
}