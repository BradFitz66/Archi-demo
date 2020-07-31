using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Archi.Core
{
    [System.Serializable]
    public class TileData
    {
        public int x, y = 0;
        public GameObject obj = null;

        public TileData(GameObject gobj, int xPos, int yPos)
        {
            obj = gobj;
            x = xPos;
            y = yPos;
        }
    }//
}
