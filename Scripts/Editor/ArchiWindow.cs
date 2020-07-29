using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityScript.Scripting.Pipeline;
using System;
using System.Linq;
using Archi.Core.Utils;

namespace Archi.Core.Editor
{
    [CustomEditor(typeof(Archi))]
    public class ArchiWindow : UnityEditor.Editor
    {
        /// <summary>
        /// Main code for the Archi inspector/window (it started off as an editor window)
        /// </summary>


        //Defining stuff
        VisualElement _rootElement;
        VisualTreeAsset _visualTree;

        Archi archi;

        Rect dropArea;

        IMGUIContainer Materials;
        IMGUIContainer toolbar;

        Grid grid;

        Material lineMaterial;
        Mesh gridMesh;

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
            gridMesh=GridUtility.GenerateCachedGridMesh(archi.grid, Color.white);
            lineMaterial = Resources.Load<Material>("Materials/Line");
            grid = archi.grid;
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
        }

        private void OnDisable()
        {
            if (plane)
            {
                DestroyImmediate(plane);
            }
        }
        private void OnValidate()
        {
            gridMesh = GridUtility.GenerateCachedGridMesh(archi.grid, Color.white);
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
                GUILayout.Label("Tools");
                archi.selectedTool = GUILayout.Toolbar(archi.selectedTool,
                    new GUIContent[] {
                        new GUIContent(Resources.Load<Texture2D>("Icons/Brush")),
                        new GUIContent(Resources.Load<Texture2D>("Icons/Fill")),
                        new GUIContent(Resources.Load<Texture2D>("Icons/Rectangle"))
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
            };

            print(Materials);
            return root;
        }
        
        RaycastHit h;
        private void OnSceneGUI()
        {
            
            var root = _rootElement;
            if (root != null)
            {
                HandleDrop(Event.current, dropArea);
            }

            if (gridMesh)
            {
                DrawGrid();
            }
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (Physics.Raycast(ray, out h))
            {
                Vector3Int closestCell = grid.WorldToCell(h.point);
                Vector3 cellWorldPos = grid.CellToWorld(closestCell);
                Handles.color = new Color(1,.3f,0);
                Handles.DrawWireCube(cellWorldPos+new Vector3(1,0,1), new Vector3(2, 0, 2));
            }

        }

        void DrawGrid()
        {
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