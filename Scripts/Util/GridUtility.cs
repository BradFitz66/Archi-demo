using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Archi.Core.Utils
{
    public static class GridUtility
    {

        public static Mesh GenerateGridMesh(GridLayout gridLayout, Color color)
        {
            switch (gridLayout.cellLayout)
            {
                case GridLayout.CellLayout.Rectangle:
                    int min = 66000 / -32;
                    int max = min * -1;
                    int numCells = max - min;
                    RectInt bounds = new RectInt(min, min, numCells, numCells);

                    return GenerateGridMesh(gridLayout, color, 2f, bounds, MeshTopology.Lines);
            }
            return null;
        }

        public static int GenerateHash(GridLayout layout, Color color)
        {
            int hash = 0x7ed55d16;
            hash ^= layout.cellSize.GetHashCode();
            hash ^= layout.cellLayout.GetHashCode() << 23;
            hash ^= (layout.cellGap.GetHashCode() << 4) + 0x165667b1;
            hash ^= layout.cellSwizzle.GetHashCode() << 7;
            hash ^= color.GetHashCode();
            return hash;
        }
        private static Vector3 GetGridForward(GridLayout gridLayout)
        {
            switch (gridLayout.cellSwizzle)
            {
                case GridLayout.CellSwizzle.XYZ:
                    return gridLayout.transform.forward * -1f;
                case GridLayout.CellSwizzle.XZY:
                    return gridLayout.transform.up * -1f;
                case GridLayout.CellSwizzle.YXZ:
                    return gridLayout.transform.forward;
                case GridLayout.CellSwizzle.YZX:
                    return gridLayout.transform.up;
                case GridLayout.CellSwizzle.ZXY:
                    return gridLayout.transform.right;
                case GridLayout.CellSwizzle.ZYX:
                    return gridLayout.transform.right * -1f;
            }
            return gridLayout.transform.forward * -1f;
        }


        private static Plane GetGridPlane(Grid grid)
        {
            return new Plane(GetGridForward(grid), grid.transform.position);
        }

        public static Vector3Int LocalToGrid(GridLayout gridLayout, Vector3 local)
        {
            return gridLayout.LocalToCell(local);
        }


        public static  Vector2Int ScreenToGrid(Vector2 screenPosition,Grid grid)
        {
            if (grid != null)
            {
                Vector3Int cell = LocalToGrid(grid, ScreenToLocal(grid.transform, screenPosition, GetGridPlane(grid)));
                return new Vector2Int(cell.x, cell.y);
            }
            return Vector2Int.zero;
        }

        public static Vector3 ScreenToLocal(Transform transform, Vector2 screenPosition)
        {
            return ScreenToLocal(transform, screenPosition, new Plane(transform.forward * -1f, transform.position));
        }

        public static Vector3 GetCellCenter(Vector3Int cell, GridLayout grid)
        {
            return grid.CellToWorld(cell) + new Vector3(grid.cellSize.x / 2, 0, grid.cellSize.y / 2);
        }

        public static Vector3 ScreenToLocal(Transform transform, Vector2 screenPosition, Plane plane)
        {
            Ray ray;
            if (Camera.current.orthographic)
            {
                Vector2 screen = EditorGUIUtility.PointsToPixels((screenPosition));
                screen.y = Screen.height - screen.y;
                Vector3 cameraWorldPoint = Camera.current.ScreenToWorldPoint(screen);
                ray = new Ray(cameraWorldPoint, Camera.current.transform.forward);
            }
            else
            {
                ray = HandleUtility.GUIPointToWorldRay(screenPosition);
            }

            float result;
            plane.Raycast(ray, out result);
            Vector3 world = ray.GetPoint(result);
            return transform.InverseTransformPoint(world);
        }

        public static Mesh GenerateGridMesh(GridLayout gridLayout, Color color, float screenPixelSize, RectInt bounds, MeshTopology topology)
        {
            Mesh mesh = new Mesh();
            mesh.hideFlags = HideFlags.HideAndDontSave;

            int vertex = 0;

            int totalVertices = topology == MeshTopology.Quads ?
                8 * (bounds.size.x + bounds.size.y) :
                4 * (bounds.size.x + bounds.size.y);

            Vector3 horizontalPixelOffset = new Vector3(screenPixelSize, 0f, 0f);
            Vector3 verticalPixelOffset = new Vector3(0f, screenPixelSize, 0f);

            Vector3[] vertices = new Vector3[totalVertices];
            Vector2[] uvs2 = new Vector2[totalVertices];

            Vector3 cellStride = gridLayout.cellSize + gridLayout.cellGap;
            Vector3Int minPosition = new Vector3Int(0, bounds.min.y, 0);
            Vector3Int maxPosition = new Vector3Int(0, bounds.max.y, 0);

            Vector3 cellGap = Vector3.zero;
            if (!Mathf.Approximately(cellStride.x, 0f))
            {
                cellGap.x = gridLayout.cellSize.x / cellStride.x;
            }

            for (int x = bounds.min.x; x < bounds.max.x; x++)
            {
                minPosition.x = x;
                maxPosition.x = x;

                vertices[vertex + 0] = gridLayout.CellToLocal(minPosition);
                vertices[vertex + 1] = gridLayout.CellToLocal(maxPosition);
                uvs2[vertex + 0] = Vector2.zero;
                uvs2[vertex + 1] = new Vector2(0f, cellStride.y * bounds.size.y);
                if (topology == MeshTopology.Quads)
                {
                    vertices[vertex + 2] = gridLayout.CellToLocal(maxPosition) + horizontalPixelOffset;
                    vertices[vertex + 3] = gridLayout.CellToLocal(minPosition) + horizontalPixelOffset;
                    uvs2[vertex + 2] = new Vector2(0f, cellStride.y * bounds.size.y);
                    uvs2[vertex + 3] = Vector2.zero;
                }
                vertex += topology == MeshTopology.Quads ? 4 : 2;

                vertices[vertex + 0] = gridLayout.CellToLocalInterpolated(minPosition + cellGap);
                vertices[vertex + 1] = gridLayout.CellToLocalInterpolated(maxPosition + cellGap);
                uvs2[vertex + 0] = Vector2.zero;
                uvs2[vertex + 1] = new Vector2(0f, cellStride.y * bounds.size.y);
                if (topology == MeshTopology.Quads)
                {
                    vertices[vertex + 2] = gridLayout.CellToLocalInterpolated(maxPosition + cellGap) + horizontalPixelOffset;
                    vertices[vertex + 3] = gridLayout.CellToLocalInterpolated(minPosition + cellGap) + horizontalPixelOffset;
                    uvs2[vertex + 2] = new Vector2(0f, cellStride.y * bounds.size.y);
                    uvs2[vertex + 3] = Vector2.zero;
                }
                vertex += topology == MeshTopology.Quads ? 4 : 2;
            }

            minPosition = new Vector3Int(bounds.min.x, 0, 0);
            maxPosition = new Vector3Int(bounds.max.x, 0, 0);
            cellGap = Vector3.zero;
            if (!Mathf.Approximately(cellStride.y, 0f))
            {
                cellGap.y = gridLayout.cellSize.y / cellStride.y;
            }

            for (int y = bounds.min.y; y < bounds.max.y; y++)
            {
                minPosition.y = y;
                maxPosition.y = y;

                vertices[vertex + 0] = gridLayout.CellToLocal(minPosition);
                vertices[vertex + 1] = gridLayout.CellToLocal(maxPosition);
                uvs2[vertex + 0] = Vector2.zero;
                uvs2[vertex + 1] = new Vector2(cellStride.x * bounds.size.x, 0f);
                if (topology == MeshTopology.Quads)
                {
                    vertices[vertex + 2] = gridLayout.CellToLocal(maxPosition) + verticalPixelOffset;
                    vertices[vertex + 3] = gridLayout.CellToLocal(minPosition) + verticalPixelOffset;
                    uvs2[vertex + 2] = new Vector2(cellStride.x * bounds.size.x, 0f);
                    uvs2[vertex + 3] = Vector2.zero;
                }
                vertex += topology == MeshTopology.Quads ? 4 : 2;

                vertices[vertex + 0] = gridLayout.CellToLocalInterpolated(minPosition + cellGap);
                vertices[vertex + 1] = gridLayout.CellToLocalInterpolated(maxPosition + cellGap);
                uvs2[vertex + 0] = Vector2.zero;
                uvs2[vertex + 1] = new Vector2(cellStride.x * bounds.size.x, 0f);
                if (topology == MeshTopology.Quads)
                {
                    vertices[vertex + 2] = gridLayout.CellToLocalInterpolated(maxPosition + cellGap) + verticalPixelOffset;
                    vertices[vertex + 3] = gridLayout.CellToLocalInterpolated(minPosition + cellGap) + verticalPixelOffset;
                    uvs2[vertex + 2] = new Vector2(cellStride.x * bounds.size.x, 0f);
                    uvs2[vertex + 3] = Vector2.zero;
                }
                vertex += topology == MeshTopology.Quads ? 4 : 2;
            }

            var uv0 = new Vector2(50f, 0f);
            var uvs = new Vector2[vertex];
            var indices = new int[vertex];
            var colors = new Color[vertex];
            var normals = new Vector3[totalVertices];     
            var uvs3 = new Vector2[totalVertices];

            for (int i = 0; i < vertex; i++)
            {
                uvs[i] = uv0;
                indices[i] = i;
                colors[i] = color;
                var alternate = i + ((i % 2) == 0 ? 1 : -1);
                normals[i] = vertices[alternate];
                uvs3[i] = uvs2[alternate];
            }

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.uv2 = uvs2;
            mesh.uv3 = uvs3;
            mesh.colors = colors;
            mesh.normals = normals;
            mesh.SetIndices(indices, topology, 0);

            return mesh;
        }
    }
}