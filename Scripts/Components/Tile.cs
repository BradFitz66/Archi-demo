using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Net.Security;
using UnityEditor;

namespace Archi.Core.Components {
    [Flags]
    public enum TileBitMask
    {
        None = 0,
        Top = 1,
        Right = 2,
        Bottom = 4,
        Left = 8,
    }

    [System.Serializable]
    public class TileDictionary : SerializableDictionary<int,AutoTileData> {}

    [System.Serializable]
    public struct AutoTileData
    {
        public GameObject tile;
        
        public float rotationDiff;

        public bool ZRotation;
        
        /// <summary>
        /// Create a new autotile
        /// </summary>
        /// <param name="t">GameObject tile</param>
        /// <param name="r">Rotation of the tile</param>
        public AutoTileData(GameObject t, float r,bool z)
        {
            tile = t;
            rotationDiff = r;
            ZRotation = z;
        }
    }

    [ExecuteInEditMode]
    public class Tile : MonoBehaviour
    {
        [HideInInspector]
        public Archi tilemap;
        [HideInInspector]
        public Vector3Int gridPosition=Vector3Int.zero;
        TileBitMask mask;

        
        public TileDictionary rotations;

        TileData[] neighbours;
        AutoTileData defaultMesh;

        void Awake()
        {
            //List of tiles.
            rotations = new TileDictionary();

            //rotations.Add(8, Resources.Load<GameObject>("Prefabs/mesh_wall2Rot"));
            //rotations.Add(2, Resources.Load<GameObject>("Prefabs/mesh_wall2Rot"));
            //rotations.Add(10, Resources.Load<GameObject>("Prefabs/mesh_wall2Rot"));
            //rotations.Add(12, Resources.Load<GameObject>("Prefabs/mesh_wall3"));
            //rotations.Add(6, Resources.Load<GameObject>("Prefabs/mesh_wall3Rot"));
            //rotations.Add(7, Resources.Load<GameObject>("Prefabs/mesh_wall4Rot2"));
            //rotations.Add(13, Resources.Load<GameObject>("Prefabs/mesh_wall4Rot3"));
            //rotations.Add(3, Resources.Load<GameObject>("Prefabs/mesh_wall3Rot2"));
            //rotations.Add(9, Resources.Load<GameObject>("Prefabs/mesh_wall3Rot3"));
            //rotations.Add(11, Resources.Load<GameObject>("Prefabs/mesh_wall4"));
            //rotations.Add(14, Resources.Load<GameObject>("Prefabs/mesh_wall4Rot"));
            //rotations.Add(15, Resources.Load<GameObject>("Prefabs/mesh_wall5"));

            rotations.Add(2, new AutoTileData(Resources.Load<GameObject>("Prefabs/mesh_wall2"), 90,true));
            rotations.Add(3, new AutoTileData(Resources.Load<GameObject>("Prefabs/mesh_wall3"), 180, true));
            rotations.Add(6, new AutoTileData(Resources.Load<GameObject>("Prefabs/mesh_wall3"), 90, true));
            rotations.Add(7, new AutoTileData(Resources.Load<GameObject>("Prefabs/mesh_wall4"), -90, true));
            rotations.Add(8, new AutoTileData(Resources.Load<GameObject>("Prefabs/mesh_wall2"), 90, true));
            rotations.Add(9, new AutoTileData(Resources.Load<GameObject>("Prefabs/mesh_wall3"), -90, true));
            rotations.Add(10, new AutoTileData(Resources.Load<GameObject>("Prefabs/mesh_wall2"), 90,true));
            rotations.Add(11, new AutoTileData(Resources.Load<GameObject>("Prefabs/mesh_wall4"), 0,true));
            rotations.Add(12, new AutoTileData(Resources.Load<GameObject>("Prefabs/mesh_wall3"), 0,true));
            rotations.Add(13, new AutoTileData(Resources.Load<GameObject>("Prefabs/mesh_wall4"), 90,true));
            rotations.Add(14, new AutoTileData(Resources.Load<GameObject>("Prefabs/mesh_wall4"), 180,true));
            rotations.Add(15, new AutoTileData(Resources.Load<GameObject>("Prefabs/mesh_wall5"), 180,true));
            
            defaultMesh = new AutoTileData(Resources.Load<GameObject>("Prefabs/mesh_wall2"),0,true);
        }
        private void OnDestroy()
        {
            
        }
        void GetNeighbours(out TileData[] neighbours)
        {
            neighbours = new TileData[8];
            Vector3Int t = gridPosition + new Vector3Int(0, -1, 0);
            Vector3Int r = gridPosition + new Vector3Int(1, 0, 0);
            Vector3Int b = gridPosition + new Vector3Int(0, 1, 0);
            Vector3Int l = gridPosition + new Vector3Int(-1, 0, 0);
            neighbours[0] = tilemap.tiles.ContainsKey(t) ? tilemap.tiles[t] : null; //Top 
            neighbours[1] = tilemap.tiles.ContainsKey(r) ? tilemap.tiles[r] : null;//Right
            neighbours[2] = tilemap.tiles.ContainsKey(b) ? tilemap.tiles[b] : null;//Bottom
            neighbours[3] = tilemap.tiles.ContainsKey(l) ? tilemap.tiles[l] : null;//Left

        }
        public void UpdateTile(bool beingUpdated = false, bool isUpdater=false)
        {
            GetNeighbours(out neighbours);
            mask = TileBitMask.None;
            for (int i = 0; i < neighbours.Length; i++)
            {
                if (beingUpdated)
                    break;
                if (neighbours[i] != null)
                {
                    if(neighbours[i].obj!=null)
                        neighbours[i].obj.GetComponent<Tile>().UpdateTile(true);
                }
            }
            if (isUpdater)
                return;
            if (neighbours[0] != null)
                mask |= TileBitMask.Top;
            if (neighbours[1] != null)
                mask |= TileBitMask.Right;
            if (neighbours[2] != null)
                mask |= TileBitMask.Bottom;
            if (neighbours[3] != null)
                mask |= TileBitMask.Left;
            GameObject newTile = rotations.ContainsKey((int)mask) ? rotations[(int)mask].tile: defaultMesh.tile;
            float rotationDiff = rotations.ContainsKey((int)mask) ? rotations[(int)mask].rotationDiff: defaultMesh.rotationDiff;
            bool zRotation = rotations.ContainsKey((int)mask) ? rotations[(int)mask].ZRotation: defaultMesh.ZRotation;
            GameObject tile = PrefabUtility.InstantiatePrefab(newTile) as GameObject;

            Tile t = tile.GetComponent<Tile>();
            t.gridPosition = gridPosition;
            t.tilemap = tilemap;
            tile.transform.position = transform.position;
            tile.transform.rotation *= !zRotation ? Quaternion.Euler(0, rotationDiff, 0) : Quaternion.Euler(0,0,rotationDiff) ;
            tile.transform.parent = tilemap.transform;
            tile.GetComponent<Renderer>().sharedMaterial = GetComponent<Renderer>().sharedMaterial;
            t.tilemap.tiles[t.gridPosition].obj = tile;
            t.neighbours = neighbours;
            t.mask = mask;
            tile.hideFlags = hideFlags;
            DestroyImmediate(gameObject);
            
        }
        //hack
        static void drawString(string text, Vector3 worldPos, Color? colour = null)
        {
            UnityEditor.Handles.BeginGUI();
            if (colour.HasValue) GUI.color = colour.Value;
            var view = UnityEditor.SceneView.currentDrawingSceneView;
            Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);
            Vector2 size = GUI.skin.label.CalcSize(new GUIContent(text));
            GUI.Label(new Rect(screenPos.x - (size.x / 2), -screenPos.y + view.position.height + 4, size.x, size.y), text);
            UnityEditor.Handles.EndGUI();
        }        
        private void OnDrawGizmosSelected()
        {
            //drawString(mask.ToString() +" "+ ((int)mask).ToString(), transform.position + new Vector3(0, 2, 0), Color.black);
        }
        private void OnDrawGizmos()
        {
            if (neighbours == null)
                return;
            for (int i = 0; i < neighbours.Length; i++)
            {
                if (neighbours[i] != null && neighbours[i].obj!=null)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawCube(neighbours[i].obj.transform.position, new Vector3(.1f, .1f, .1f));
                    Gizmos.DrawLine(transform.position,neighbours[i].obj.transform.position);
                }
            }
        }
    }
}