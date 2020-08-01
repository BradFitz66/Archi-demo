using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.ObjectModel;

namespace Archi.Core
{
    [System.Serializable]
    public class TileDictionary : SerializableDictionary<Vector3Int, TileData> {}
    [RequireComponent(typeof(Grid))]
    [ExecuteInEditMode] 
    public class Archi : MonoBehaviour
    {
        [HideInInspector]
        public TileDictionary tiles=new TileDictionary();

        [HideInInspector]
        public GameObject geometry;//
        [HideInInspector]
        public List<Material> materials = new List<Material>();
        public int mHash;
        [HideInInspector]
        public List<GameObject> Tiles = new List<GameObject>();
        public int tHash;

        //Data
        [HideInInspector]
        public int selectedMaterial = 0;
        [HideInInspector]
        public int selectedTool = 0;
        [HideInInspector]
        public int selectedTile = 0;
        [HideInInspector]
        public Grid grid;

        GameObject plane;

        public int GenerateHashList<T>(List<T> list)
        {
            int hash = 0x7ed55d16;
            hash ^= list.Count.GetHashCode();
            hash ^= list.GetHashCode() << 23;
            return hash;
        }



        private void Awake()
        {
            grid = GetComponent<Grid>();
            if (geometry == null)
            {
                geometry = new GameObject("ArchiGeometry");
            }
            mHash= GenerateHashList(materials);
            tHash= GenerateHashList(Tiles);
        }
        private void OnValidate()
        {
            grid = GetComponent<Grid>();
        }
        void Start()
        {
            
        }

        void Update()
        {
            if (geometry == null)
            {
                geometry = new GameObject("ArchiGeometry");
            }
            if (grid == null)
            {
                grid = GetComponent<Grid>();
            }
        }
    }
}