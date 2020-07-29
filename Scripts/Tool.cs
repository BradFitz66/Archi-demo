using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Archi.Core.Tools
{
    public abstract class Tool 
    {
        public abstract void Paint(GameObject tile, Material[] materials,Bounds paintArea);

    }
}