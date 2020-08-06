using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.ObjectModel;
using Archi.Core.Components;
using UnityEditor;

namespace Archi.Core
{
    [System.Serializable]
    public class TileDictionary : SerializableDictionary<Vector3Int, TileData> {}
    [RequireComponent(typeof(Grid))]
    [ExecuteInEditMode]
    public class Archi : MonoBehaviour
    {

        public TileDictionary tiles;

        public List<Material> materials;
        public int mHash;
        public List<GameObject> Tiles;
        public int tHash;

        //Data (I store data in this script because Unity will serialize it and save it)

        public int selectedMaterial = 0;

        public int selectedTool = 0;
        public int selectedTile = 0;
        public Grid grid;

        public bool useTileMaterial = false;

        public Tools.Tool[] tools=new Tools.Tool[] { };


        void Awake()
        {

            if (tiles==null)
            {
                print("Creating new tiles dictionary because it's null");
                tiles = new TileDictionary();
            }
            if (materials == null)
            {
                materials = new List<Material>();
            }
            if (Tiles == null)
            {
                Tiles = new List<GameObject>();
            }
            if (grid == null)
            {
                grid = GetComponent<Grid>();
            }
            print(tiles.Count);
        }

        

        public void PlaceTile(Vector3Int pos,Vector3 worldPos, GameObject tile,bool UpdateOnPlace=true, bool onlyUpdateNeighbours=false)
        {
            if (Application.isPlaying || !Application.isEditor)
                return;
            GameObject TestTile = PrefabUtility.InstantiatePrefab(tile, transform) as GameObject;

            if (!useTileMaterial)
            {
                print("!!!");
                Renderer r = TestTile.GetComponent<Renderer>();
                r.sharedMaterial = materials[selectedMaterial];
            }
            //Get the world position of the cell from it's grid coordinates make sure we place at the center of the cell

            //Add tile to tile dictionary with the key being the grid location of the cell it was placed on.
            tiles.Add(pos, new TileData(TestTile, pos.x, pos.y));
            Tile t = TestTile.GetComponent<Tile>();
            t.enabled = false;
            t.tilemap = this;
            t.transform.parent = transform;
            t.transform.position = worldPos;
            t.gridPosition = pos;
            t.enabled = true;
        }

        public void RemoveTile(Vector3Int cellPos)
        {
            if (Application.isPlaying || !Application.isEditor)
                return;
            if (tiles.ContainsKey(cellPos))
            {
                TileData tile = tiles[cellPos];
                tiles.Remove(cellPos);
                //tile.obj.GetComponent<Tile>().UpdateTile();

                DestroyImmediate(tile.obj);
            }
        }


        

        void Update()
        {
            if (Application.isPlaying || !Application.isEditor)
                return;
            if (grid == null)
            {
                grid = GetComponent<Grid>();
            }
        }
    }
}