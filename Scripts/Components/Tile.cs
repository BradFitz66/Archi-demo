using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Net.Security;
using UnityEditor;
using UnityEngine.UI;
using Malee.List;

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
        [Tooltip("The tile prefab. This must have a Tile component attached to it")]
        public GameObject tile;
        
        [Tooltip("Rotation difference. This is so you can reuse the same tile prefab and just rotate it a certain amount of degrees. Useful for corner pieces.")]
        public float rotationDiff;

        [Tooltip("Should the tile rotate on the Z axis instead of Y (useful for if you mess up a blender export)")]
        public bool ZRotation;

        [Tooltip("The tile mask. For example, if the tile is a corner piece, this should be set to either (Right+Bottom), (Right+Top), (Left+Top), (Left+Bottom) ")]
        public TileBitMask tileMask;
        
        /// <summary>
        /// Create a new autotile
        /// </summary>
        /// <param name="t">GameObject tile</param>
        /// <param name="r">Rotation of the tile</param>
        /// <param name="z">Should we rotate by Z instead of Y? (for when you fuck up blender exporting, like I did)</param>
        /// <param name="tM">Should we rotate by Z instead of Y? (for when you fuck up blender exporting, like I did)</param>
        public AutoTileData(GameObject t, float r,bool z, TileBitMask tM=TileBitMask.None)
        {
            tile = t;
            rotationDiff = r;
            ZRotation = z;
            tileMask= tM;
        }
    }
    [System.Serializable]
    public class AutoTileDataList : ReorderableArray<AutoTileData>{}
    [ExecuteInEditMode]
    public class Tile : MonoBehaviour
    {
        [HideInInspector]
        public Archi tilemap;
        [HideInInspector]
        public Vector3Int gridPosition=Vector3Int.zero;
        TileBitMask mask;

        
        TileDictionary rotations;


        [Reorderable(surrogateType = typeof(AutoTileData), surrogateProperty = "objectProperty")]
        public AutoTileDataList rotationsList;
        
        TileData[] neighbours;
        [Tooltip("The default tile that will be placed if no rule can be found. THIS SHOULD NOT BE THE SAME AS YOUR 'MAIN' TILE (the one that contains the rule info)")]
        public AutoTileData defaultTile;

        void Awake()
        {
            rotations = new TileDictionary();

            defaultTile = new AutoTileData(Resources.Load<GameObject>("Prefabs/mesh_wall"),0,true);
        }
        //We need to be able to control when we add the stuff from rotationsList into rotations.
        public void FillRotationsDictionary()
        {
            for (int i = 0; i < rotationsList.Count; i++)
            {
                /*Ugly hack. For some reason, selecting all of the flags in unity's enum flag field drawer causes (int)mask to become -1
                    
                Apparently this is 'correct' but isn't what I want, since in code doing the same thing via:

                mask = TileBitMask.Left | TileBitMask.Right | TileBitMask.Bottom | TileBitMask.Top; and doing (int)mask will give me 15, causing us to not be able
                to get the correct tile from the dictionary. I just hardcode it so if the tileMask is -1, then just set it to be 15. Ugly, but it works.
                 */
                rotations.Add((int)rotationsList[i].tileMask!= -1 ? (int)rotationsList[i].tileMask : 15, rotationsList[i]);
            }

        }
        private void OnDestroy()
        {
            
        }
        void GetNeighbours(out TileData[] neighbours)
        {
            neighbours = new TileData[8];
            Vector3Int t = gridPosition + new Vector3Int(0, 1, 0);
            Vector3Int r = gridPosition + new Vector3Int(1, 0, 0);
            Vector3Int b = gridPosition + new Vector3Int(0, -1, 0);
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
            //This is for updating neighbours.
            for (int i = 0; i < neighbours.Length; i++)
            {
                //Stops stackoverflow.
                if (beingUpdated)
                    break;
                if (neighbours[i] != null)
                {
                    if(neighbours[i].obj!=null)
                        neighbours[i].obj.GetComponent<Tile>().UpdateTile(true);
                }
            }
            //Don't continue if we're an updater tile(a tile that updates others
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



            GameObject newTile = rotations.ContainsKey((int)mask) ? rotations[(int)mask].tile: defaultTile.tile;
            float rotationDiff = rotations.ContainsKey((int)mask) ? rotations[(int)mask].rotationDiff: defaultTile.rotationDiff;
            bool zRotation = rotations.ContainsKey((int)mask) ? rotations[(int)mask].ZRotation: defaultTile.ZRotation;

            //Replacing tile with new one
            
            GameObject tile = PrefabUtility.InstantiatePrefab(newTile) as GameObject;
            if (tile)
            {
                Tile t = tile.GetComponent<Tile>();
                tile.transform.position = transform.position;
                tile.transform.rotation *= !zRotation ? Quaternion.Euler(0, rotationDiff, 0) : Quaternion.Euler(0, 0, rotationDiff);
                tile.transform.parent = tilemap.transform;
                tile.GetComponent<Renderer>().sharedMaterial = GetComponent<Renderer>().sharedMaterial;
                if (t)
                {
                    t.gridPosition = gridPosition;
                    t.tilemap = tilemap;
                    t.tilemap.tiles[t.gridPosition].obj = tile;
                    t.neighbours = neighbours;
                    t.mask = mask;
                    t.rotationsList = rotationsList;
                    t.FillRotationsDictionary();
                }
                tile.hideFlags = hideFlags;
                DestroyImmediate(gameObject);
            }
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
            drawString(((int)mask).ToString(), transform.position + new Vector3(0, 2, 0), Color.black);
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