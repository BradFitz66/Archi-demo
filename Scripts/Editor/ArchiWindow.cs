using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using Archi.Core.Utils;
using Archi.Core.Components;
using NUnit.Framework.Internal;
using System;

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

        Grid grid;

        Material lineMaterial;
        Mesh gridMesh;

        UnityEditor.Editor editor;
        Tile tile;
        int gridHash;


        GUIContent[] materialPreviews;

        //Invisible plane for a plane for the mouse raycast to hit.
        GameObject plane;        
        void print(object msg)
        {
            MonoBehaviour.print(msg);
        }

        


        private void OnEnable()
        {
            archi = (Archi)target;
            tile = archi.geometry.GetComponent<Tile>()==null ? archi.geometry.AddComponent<Tile>() : archi.geometry.GetComponent<Tile>();
            lineMaterial = Resources.Load<Material>("Materials/Line");
            grid = archi.grid;
            gridHash = GridUtility.GenerateHash(grid, Color.white);
            _rootElement = new VisualElement();
            _visualTree = Resources.Load<VisualTreeAsset>("UXML/MainWindow");
            materialPreviews = new GUIContent[archi.materials.Count];
            for (int i = 0; i < materialPreviews.Length; i++)
            {
                materialPreviews[i] = new GUIContent(archi.materials[i].mainTexture, archi.materials[i].name);
            }
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
            tiles = archi.tiles;
        }
        
        private void OnDisable()
        {//
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
            };
            Materials.onGUIHandler += () =>
            {
                dropArea = GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                EditorGUILayout.BeginVertical("Box");
                GUILayout.Label("Material");
                archi.selectedMaterial = GUILayout.Toolbar(archi.selectedMaterial,
                    materialPreviews,
                    GUILayout.Width(34 * materialPreviews.Length),
                    GUILayout.Height(34)
                );
                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel += 5;
                if (!editor)
                    UnityEditor.Editor.CreateCachedEditor(tile, null, ref editor);
                editor.OnInspectorGUI();
                EditorGUI.indentLevel -= 5;
            };

            return root;
        }
        
        RaycastHit h;
        private void OnSceneGUI()
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Keyboard));
            UnityEditor.Tools.current = Tool.None;
            var root = _rootElement;
            if (root != null)
            {
                HandleDrop(Event.current, dropArea);
            }

            DrawGrid();
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (Physics.Raycast(ray, out h))
            {
                Vector3Int closestCell = grid.WorldToCell(h.point);
                Vector3 cellWorldPos = grid.CellToWorld(closestCell);
                Handles.color = new Color(1, .3f, 0);
                Handles.DrawWireCube(cellWorldPos + new Vector3(grid.cellSize.x/2, 0, grid.cellSize.y/2), new Vector3(grid.cellSize.x, 0, grid.cellSize.y));
            }
            HandleInput(Event.current);
        }

        Vector3Int CellAtMouse()
        {
            Vector3Int cell = new Vector3Int(-1,-1,-1);
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

        void DrawGrid()
        {
            int hash = GridUtility.GenerateHash(grid, Color.white);

            if(hash != gridHash || gridMesh==null)
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
                            GameObject TestTile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            Vector3 cellWorldPos = (WorldPosOfCell(CellAtMouse()))+new Vector3(grid.cellSize.x/2,0,grid.cellSize.y/2);
                            TestTile.transform.position = cellWorldPos;
                            Vector3Int pos = CellAtMouse();
                            tiles.Add(pos, new TileData(TestTile, pos.x, pos.y));
                            Tile t = TestTile.AddComponent<Tile>();
                            t.tilemap = archi;
                            t.gridPosition = pos;
                            t.UpdateTile();
                            //Just incase.
                            if(TestTile)
                                TestTile.transform.parent = archi.transform;
                        }
                    }
                    else
                    {   
                        if (erasing && tiles.ContainsKey(CellAtMouse()))
                        {
                            GameObject obj = tiles[CellAtMouse()].obj;
                            tiles.Remove(CellAtMouse());
                            obj.GetComponent<Tile>().UpdateTile(false,true);
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

        void HandleDrop(Event evt, Rect drop_area)
        {
            switch (evt.type)
            {
                case EventType.DragUpdated:
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    break;
                case EventType.DragExited:
                    if (!drop_area.Contains(evt.mousePosition))
                        return;
                    DragAndDrop.AcceptDrag();
                    foreach (UnityEngine.Object dragged_object in DragAndDrop.objectReferences)
                    {
                        if (dragged_object is Material)
                        {
                            archi.materials.Add(dragged_object as Material);
                        }
                    }
                    break;
            }
        }

    }
}