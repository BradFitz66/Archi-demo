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


        List<GUIContent> materialPreviews;
        List<GUIContent> tilePreviews;
        List<GUIContent> toolPreviews;

        int selectedTile;

        Vector3 MouseCellWorldPos;
        Vector2Int closestCell;


        //Invisible plane for a plane for the mouse raycast to hit.
        GameObject plane;
        void print(object msg)
        {
            MonoBehaviour.print(msg);
        }


        private void OnEnable()
        {
            archi = (Archi)target;
            lineMaterial = Resources.Load<Material>("Materials/Line");
            grid = archi.grid;
            gridHash = GridUtility.GenerateHash(grid, Color.white);
            _rootElement = new VisualElement();
            _visualTree = Resources.Load<VisualTreeAsset>("UXML/MainWindow");
            materialPreviews = new List<GUIContent>(archi.materials.Count);
            tilePreviews = new List<GUIContent>(archi.Tiles.Count);
            archi.tools = new Tools.Tool[]
            {
                new Brush(Resources.Load<Texture2D>("Icons/brush")),
                new Rectangle(Resources.Load<Texture2D>("Icons/rectangle")),
                new Brush(Resources.Load<Texture2D>("Icons/rectangle")),
            };
            print(archi.tools.Length);//
            toolPreviews = new List<GUIContent>();
            for(int i=0; i<archi.tools.Length; i++)
            {
                toolPreviews.Add(new GUIContent(archi.tools[i].icon));
            }
            print(toolPreviews.Count);
            UpdateMaterialPreviews();
            UpdateTilePreviews();
            selectedTile = archi.selectedTile;
            tiles = archi.tiles;
            MouseCellWorldPos = Vector3.zero;
        }

        private void OnDisable()
        {

            DestroyGrid();
            HandleUtility.AddDefaultControl(-1);
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
                print(toolPreviews.Count);
                archi.selectedTool = GUILayout.Toolbar(archi.selectedTool,
                    toolPreviews.ToArray(),
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
                archi.useTileMaterial = GUILayout.Toggle(archi.useTileMaterial, new GUIContent("Use tile material","If checked, it will leave the tiles material as is when placed"));
                if (!archi.useTileMaterial)
                {
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
                }
                else
                {
                    GUILayout.Label("Using tiles material");
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
                HandleDrop(Event.current);
            };
            tileEditor.onGUIHandler += () =>
            {

                archi.selectedTile = Mathf.Clamp(archi.selectedTile, 0, archi.Tiles.Count-1);
                if (archi.Tiles.Count == 0)
                    return;
                EditorGUILayout.BeginVertical("Box");
                if (!editor && archi.Tiles[archi.selectedTile]!=null)
                    CreateCachedEditor(archi.Tiles[archi.selectedTile].GetComponent<Tile>(),null, ref editor);
                if(editor)
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

            closestCell = GridUtility.ScreenToGrid(Event.current.mousePosition,grid);
            MouseCellWorldPos = grid.CellToWorld((Vector3Int)closestCell);

            if(archi.tools[archi.selectedTool]!=null)
                archi.tools[archi.selectedTool].Preview((Vector3Int)closestCell,grid);
            HandleInput(Event.current);
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
                    archi.tools[archi.selectedTool].MouseDrag(archi.Tiles[selectedTile], new Material[] { }, (Vector3Int)closestCell, archi, erasing);
                    break;
                case EventType.MouseDown:
                    if (e.button != 0)
                        return;
                        archi.tools[archi.selectedTool].MouseDown(archi.Tiles[selectedTile], new Material[] {}, (Vector3Int)closestCell, archi,erasing);
                    break;
                case EventType.MouseUp:
                    if (e.button != 0)
                        return;
                        archi.tools[archi.selectedTool].MouseUp(archi.Tiles[selectedTile], new Material[] {}, (Vector3Int)closestCell, archi,erasing);
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