using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using Archi.Core.Utils;
using Archi.Core.Components;
using NUnit.Framework.Internal;
using System;
using System.Globalization;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.Linq;

namespace Archi.Core.Editor
{
    [CustomEditor(typeof(Archi))]
    public class ArchiWindow : UnityEditor.Editor
    {
        /// <summary>
        /// Main code for the Archi inspector/window (it started off as an editor window)
        /// </summary>

        TileDictionary tiles;

        //Defining stuff
        VisualElement _rootElement;
        VisualTreeAsset _visualTree;
        Archi archi;

        bool erasing = false;

        Rect dropArea;

        IMGUIContainer Materials;
        IMGUIContainer toolbar;
        IMGUIContainer tilePalette;
        IMGUIContainer tileEditor;


        Grid grid;
        Grid tilePaletteGrid;



        Material lineMaterial;
        Mesh gridMesh;
        Mesh gridMesh2D;

        UnityEditor.Editor editor;
        Tile tile;
        int gridHash;
        int gridHash2D;


        Scene previewScene;
        List<GUIContent> materialPreviews;
        List<GUIContent> tilePreviews;

        int selectedTile;
        GameObject camera;
        Camera camComp;
        //Invisible plane for a plane for the mouse raycast to hit.
        GameObject plane;
        void print(object msg)
        {
            MonoBehaviour.print(msg);
        }

        


        private void OnEnable()
        {
            archi = (Archi)target;
            tile = archi.geometry.GetComponent<Tile>() == null ? archi.geometry.AddComponent<Tile>() : archi.geometry.GetComponent<Tile>();
            lineMaterial = Resources.Load<Material>("Materials/Line");
            grid = archi.grid;
            gridHash = GridUtility.GenerateHash(grid, Color.white);
            _rootElement = new VisualElement();
            _visualTree = Resources.Load<VisualTreeAsset>("UXML/MainWindow");
            materialPreviews = new List<GUIContent>(archi.materials.Count);
            tilePreviews = new List<GUIContent>(archi.Tiles.Count);
            UpdateMaterialPreviews();
            UpdateTilePreviews();
            if (!plane)
            {
                //Create plane only if it doesn't exist (just incase)
                plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
                //Hide it from hierarchy
                plane.hideFlags = HideFlags.HideAndDontSave;
                plane.transform.position = archi.transform.position;
                plane.transform.localScale = new Vector3(1000, 1, 1000);
                //Disable renderer since we only want it for collision with raycasts
                plane.GetComponent<Renderer>().enabled = false;
            }
            selectedTile = archi.selectedTile;
            tiles = archi.tiles;
        }

        private void OnDisable()
        {
            EditorSceneManager.ClosePreviewScene(previewScene);

            DestroyGrid();
            if (plane)
            {

                DestroyImmediate(plane);
            }

        }

        //Drawing
        public override VisualElement CreateInspectorGUI()
        {

            var root = _rootElement;
            root.Clear();
            _visualTree.CloneTree(root);

            Materials = _rootElement.Query<IMGUIContainer>("Materials");
            toolbar = _rootElement.Query<IMGUIContainer>("Toolbar");
            tilePalette = _rootElement.Query<IMGUIContainer>("TilePaletteGrid");
            tileEditor = _rootElement.Query<IMGUIContainer>("TileEditor");
            toolbar.onGUIHandler += () =>
            {
                EditorGUILayout.BeginVertical("Box");
                if (GUILayout.Button("Clear tilemap"))
                {
                    tiles.Clear();
                    archi.transform.ClearChildren();
                }
                GUILayout.Label("Tools");
                archi.selectedTool = GUILayout.Toolbar(archi.selectedTool,
                    new GUIContent[] {
                        new GUIContent(Resources.Load<Texture2D>("Icons/brush")),
                        new GUIContent(Resources.Load<Texture2D>("Icons/erase")),
                        new GUIContent(Resources.Load<Texture2D>("Icons/rectangle"))
                    },
                    GUILayout.Width(34 * 3),
                    GUILayout.Height(34)
                );
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            };
            Materials.onGUIHandler += () =>
            {
                EditorGUILayout.BeginVertical("Box");
                GUILayout.Label("Material");
                EditorGUILayout.BeginHorizontal();
                if (archi.materials.Count > 0)
                    archi.selectedMaterial = GUILayout.Toolbar(archi.selectedMaterial, materialPreviews.ToArray(), GUILayout.Width(34 * materialPreviews.Count), GUILayout.Height(34));
                else
                    GUILayout.Label("No materials. Drag a material onto the inspector to add it.");
                if (archi.materials.Count > 0)
                {
                    if (GUILayout.Button("-", GUILayout.Width(34), GUILayout.Height(34)))
                    {
                        archi.materials.RemoveAt(archi.materials.Count - 1);
                        UpdateMaterialPreviews();
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
                HandleDrop(Event.current);
            };
            tileEditor.onGUIHandler += () =>
            {
                if (archi.Tiles.Count == 0)
                    return;
                EditorGUILayout.BeginVertical("Box");
                if (!editor)
                    CreateCachedEditor(archi.Tiles[archi.selectedTile].GetComponent<Tile>(),null, ref editor);
                editor.OnInspectorGUI();
                EditorGUILayout.EndVertical();

            };
            tilePalette.onGUIHandler += () =>
            {
                EditorGUILayout.BeginVertical("Box");
                GUILayout.Label("Tiles");
                EditorGUILayout.BeginHorizontal();
                if (tilePreviews.Count > 0)
                    archi.selectedTile = GUILayout.Toolbar(archi.selectedTile, tilePreviews.ToArray(), GUILayout.Width(32 * tilePreviews.Count), GUILayout.Height(32));
                else
                    GUILayout.Label("No tiles. Drag and drop a tile (MUST HAVE TILE COMPONENT) prefab onto the inspector to add");

                if (archi.Tiles.Count > 0)
                {
                    if (GUILayout.Button("-",GUILayout.Width(32),GUILayout.Height(32)))
                    {
                        archi.Tiles.RemoveAt(archi.Tiles.Count-1);
                        UpdateTilePreviews();
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            };

            //Update the lists for tile and material previews.
            return root;
        }

        RaycastHit h;
        private void OnSceneGUI()
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Keyboard));
            UnityEditor.Tools.current = Tool.None;
            Draw3DGrid();
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (Physics.Raycast(ray, out h))
            {
                Vector3Int closestCell = grid.WorldToCell(h.point);
                Vector3 cellWorldPos = grid.CellToWorld(closestCell);
                Handles.color = new Color(1, .3f, 0);
                Handles.DrawWireCube(cellWorldPos + new Vector3(grid.cellSize.x / 2, 0, grid.cellSize.y / 2), new Vector3(grid.cellSize.x, 0, grid.cellSize.y));
            }
            HandleInput(Event.current);
        }
        
        Vector3Int CellAtMouse()
        {
            Vector3Int cell = new Vector3Int(-1, -1, -1);
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (Physics.Raycast(ray, out h))
            {
                cell = grid.WorldToCell(h.point);
            }
            return cell;
        }
        Vector3 WorldPosOfCell(Vector3Int cellPos)
        {
            return grid.CellToWorld(cellPos);
        }

        void Draw3DGrid()
        {
            int hash = GridUtility.GenerateHash(grid, Color.white);

            if (hash != gridHash || gridMesh == null)
            {
                DestroyGrid();
                gridMesh = GridUtility.GenerateGridMesh(grid, Color.white);
                gridHash = GridUtility.GenerateHash(grid, Color.white);
            }
            lineMaterial.SetPass(0);
            GL.PushMatrix();
            if (gridMesh.GetTopology(0) == MeshTopology.Lines)
                GL.Begin(GL.LINES);
            else
                GL.Begin(GL.QUADS);

            Graphics.DrawMeshNow(gridMesh, archi.transform.localToWorldMatrix);
            GL.End();
            GL.PopMatrix();
        }
        

        Vector3Int Vector3ToVector3Int(Vector3 v)
        {
            return new Vector3Int(
                Mathf.FloorToInt(v.x),
                Mathf.FloorToInt(v.y),
                Mathf.FloorToInt(v.z)
            );
        }

        void DestroyGrid()
        {
            if (gridMesh == null)
                return;
            DestroyImmediate(gridMesh);
            gridMesh = null;
        }
        void Destroy2DGrid()
        {
            if (gridMesh2D == null)
                return;
            DestroyImmediate(gridMesh2D);
            gridMesh2D = null;
        }

        void HandleInput(Event e)
        {
            switch (e.type)
            {
                case EventType.MouseDrag:
                    if (e.button != 0)
                        return;
                    if (!tiles.ContainsKey(CellAtMouse()) && !erasing)
                    {
                        if (archi.selectedTool == 0)
                        {
                            GameObject TestTile = PrefabUtility.InstantiatePrefab(archi.Tiles[archi.selectedTile],archi.transform) as GameObject;
                            
                            Renderer r = TestTile.GetComponent<Renderer>();
                            r.sharedMaterial = archi.materials[archi.selectedMaterial];

                            //Get the world position of the cell from it's grid coordinates make sure we place at the center of the cell
                            Vector3 cellWorldPos = (WorldPosOfCell(CellAtMouse())) + new Vector3(grid.cellSize.x / 2, 0, grid.cellSize.y / 2);
                            TestTile.transform.position = cellWorldPos;
                            Vector3Int pos = CellAtMouse();
                            //Add tile to tile dictionary with the key being the grid location of the cell it was placed on.
                            tiles.Add(pos, new TileData(TestTile, pos.x, pos.y));
                            Tile t = TestTile.GetComponent<Tile>();
                            t.tilemap = archi;
                            t.gridPosition = pos;
                            t.FillRotationsDictionary();
                            t.UpdateTile();

                        }
                    }
                    else
                    {
                        if (erasing && tiles.ContainsKey(CellAtMouse()))
                        {
                            GameObject obj = tiles[CellAtMouse()].obj;
                            tiles.Remove(CellAtMouse());
                            obj.GetComponent<Tile>().UpdateTile(false, true);
                            DestroyImmediate(obj);
                        }
                    }
                    break;
                case EventType.KeyDown:
                    if (!erasing && e.keyCode == KeyCode.E)
                        erasing = true;
                    break;
                case EventType.KeyUp:
                    if (erasing && e.keyCode == KeyCode.E)
                        erasing = false;
                    break;
            }
        }

        void UpdateMaterialPreviews()
        {
            materialPreviews.Clear();
            for (int i = 0; i < archi.materials.Count; i++)
            {
                materialPreviews.Add(new GUIContent(archi.materials[i].mainTexture, archi.materials[i].name));
            }
        }
        void UpdateTilePreviews()
        {
            tilePreviews.Clear();
            for (int i = 0; i < archi.Tiles.Count; i++)
            {
                tilePreviews.Add(new GUIContent(AssetPreview.GetAssetPreview(archi.Tiles[i]), archi.Tiles[i].name));
            }
        }

        void HandleDrop(Event evt)
        {
            switch (evt.type)
            {
                case EventType.DragUpdated:
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    break;
                case EventType.DragExited:
                    DragAndDrop.AcceptDrag();
                    foreach (UnityEngine.Object dragged_object in DragAndDrop.objectReferences)
                    {
                        if (dragged_object is Material)
                        {
                            archi.materials.Add(dragged_object as Material);
                            UpdateMaterialPreviews();
                        }
                        else if(dragged_object is GameObject)
                        {
                            if (((GameObject)dragged_object).GetComponent<Tile>())
                                archi.Tiles.Add(dragged_object as GameObject);
                            else
                                Debug.LogWarning("Tried to add a prefab to the tile selection toolbar that didn't have a tile script attached to it.");
                            UpdateTilePreviews();
                        }
                    }
                    break;
            }
        }

    }
}