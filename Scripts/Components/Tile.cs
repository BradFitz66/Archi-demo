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
    public class TileDictionary : SerializableDictionary<int,GameObject> {}

    [ExecuteInEditMode]
    public class Tile : MonoBehaviour
    {
        public Archi tilemap;
        public Vector3Int gridPosition=Vector3Int.zero;
        TileBitMask mask;

        TileDictionary rotations;

        TileData[] neighbours;
        GameObject defaultMesh;

        void Awake()
        {
            //List of tiles.s
            rotations = new TileDictionary();

            rotations.Add(8, Resources.Load<GameObject>("Prefabs/mesh_wall2Rot"));
            rotations.Add(2, Resources.Load<GameObject>("Prefabs/mesh_wall2Rot"));
            rotations.Add(10, Resources.Load<GameObject>("Prefabs/mesh_wall2Rot"));
            rotations.Add(12, Resources.Load<GameObject>("Prefabs/mesh_wall3"));
            rotations.Add(6, Resources.Load<GameObject>("Prefabs/mesh_wall3Rot"));
            rotations.Add(7, Resources.Load<GameObject>("Prefabs/mesh_wall4Rot2"));
            rotations.Add(13, Resources.Load<GameObject>("Prefabs/mesh_wall4Rot3"));
            rotations.Add(3, Resources.Load<GameObject>("Prefabs/mesh_wall3Rot2"));
            rotations.Add(9, Resources.Load<GameObject>("Prefabs/mesh_wall3Rot3"));
            rotations.Add(11, Resources.Load<GameObject>("Prefabs/mesh_wall4"));
            rotations.Add(14, Resources.Load<GameObject>("Prefabs/mesh_wall4Rot"));
            rotations.Add(15, Resources.Load<GameObject>("Prefabs/mesh_wall5"));
            
            defaultMesh =Resources.Load<GameObject>("Prefabs/mesh_wall2");
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
            print(rotations);
            GameObject newTile = rotations.ContainsKey((int)mask) ? rotations[(int)mask] : defaultMesh;
            GameObject tile = PrefabUtility.InstantiatePrefab(newTile) as GameObject;

            Tile t = tile.AddComponent<Tile>();
            t.gridPosition = gridPosition;
            t.tilemap = tilemap;
            tile.transform.position = transform.position;
            tile.transform.parent = tilemap.transform;
            t.tilemap.tiles[t.gridPosition].obj = tile;
            t.neighbours = neighbours;
            t.mask = mask;
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