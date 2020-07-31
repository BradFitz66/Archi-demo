using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Archi.Core
{
    [System.Serializable]
    public class TileDictionary : SerializableDictionary<Vector3Int, TileData> {}
    [RequireComponent(typeof(Grid))]
    [ExecuteInEditMode] 
    public class Archi : MonoBehaviour, ISerializationCallbackReceiver
    {
        [HideInInspector]
        public TileDictionary tiles=new TileDictionary();
        List<Vector3Int> positions;
        List<TileData> data;

        [HideInInspector]
        public GameObject geometry;//
        [HideInInspector]
        public List<Material> materials = new List<Material>();

        //Data
        [HideInInspector]
        public int selectedMaterial = 0;
        [HideInInspector]
        public int selectedTool = 0;
        [HideInInspector]
        public Grid grid;

        GameObject plane;

        public void OnBeforeSerialize()
        {
            //positions.Clear();
            //data.Clear();
            //foreach (var kvp in tiles)
            //{
            //    positions.Add(kvp.Key);
            //    data.Add(kvp.Value);
            //}
            

        }
        public void OnAfterDeserialize()
        {
            //tiles = new Dictionary<Vector3Int, TileData>();

            //for (int i = 0; i != System.Math.Min(positions.Count, data.Count); i++)
            //    tiles.Add(positions[i], data[i]);
        }

        private void Awake()
        {
            grid = GetComponent<Grid>();
            if (geometry == null)
            {
                geometry = new GameObject("ArchiGeometry");
            }
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