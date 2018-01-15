using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class LevelGrid : MonoBehaviour
{
    public MeshFilter   Filter      { get; private set; }
    public Mesh         Geometry    { get; private set; }
    public Vector3      Origin      { get; private set; }

    public int GridWidth, GridHeight;
    public int CellSize;

    [Range(0.0f, 1.0f)]
    public float LineThickness;

    private void Awake()
    {
        Filter      = GetComponent<MeshFilter>();
        Geometry    = new Mesh();
    }

    private void Update()
    {
        Origin = transform.position;
        InitializeGridGeometry();
        Filter.sharedMesh = Geometry;
    }

    public void InitializeSurfaceGeometry()
    {
        int widthPlusOne    = GridWidth + 1;
        int heightPlusOne   = GridHeight + 1;
        Vector3[] vertices  = new Vector3[widthPlusOne * heightPlusOne];

        for (int i = 0; i < heightPlusOne; ++i)
        {
            for (int j = 0; j < widthPlusOne; ++j)
            {
                vertices[i * widthPlusOne + j] = Origin + new Vector3(j, i);
            }
        }

        int[] triangles     = new int[GridWidth * GridHeight * 6];
        int triangleIndex   = 0;

        for (int i = 0; i < GridHeight; ++i)
        {
            for (int j  = 0; j < GridWidth; ++j)
            {
                int topLeftIndex            = i * widthPlusOne + j;
                int topRightIndex           = topLeftIndex + 1;
                int bottomRightIndex        = topRightIndex + widthPlusOne;
                int bottomLeftIndex         = bottomRightIndex - 1;

                triangles[triangleIndex++]  = bottomLeftIndex;
                triangles[triangleIndex++]  = bottomRightIndex;
                triangles[triangleIndex++]  = topRightIndex;

                triangles[triangleIndex++]  = bottomLeftIndex;
                triangles[triangleIndex++]  = topRightIndex;
                triangles[triangleIndex++]  = topLeftIndex;
            }
        }

        Geometry.Clear();
        Geometry.vertices   = vertices;
        Geometry.triangles  = triangles;
    }

    public static void TriangulateLines(
        bool clockwise,
        int width,
        int height,
        int offset,
        ref int triangleIndex,
        int[] triangles
    )
    {
        if (clockwise)
        {
            for (int i = 0; i < height * 2; i += 2)
            {
                for (int j = 0; j < width; ++j)
                {
                    int topLeftIndex        = i * (width + 1) + j + offset;
                    int topRightIndex       = topLeftIndex + 1;
                    int bottomLeftIndex     = topLeftIndex + width + 1;
                    int bottomRightIndex    = bottomLeftIndex + 1;

                    triangles[triangleIndex++] = bottomLeftIndex;
                    triangles[triangleIndex++] = bottomRightIndex;
                    triangles[triangleIndex++] = topRightIndex;

                    triangles[triangleIndex++] = bottomLeftIndex;
                    triangles[triangleIndex++] = topRightIndex;
                    triangles[triangleIndex++] = topLeftIndex;
                }
            }
        }
        else
        {
            for (int i = 0; i < height * 2; i += 2)
            {
                for (int j = 0; j < width; ++j)
                {
                    int bottomLeftIndex     = i * (width + 1) + j + offset;
                    int bottomRightIndex    = bottomLeftIndex + width + 1;
                    int topLeftIndex        = bottomLeftIndex + 1;
                    int topRightIndex       = topLeftIndex + width + 1;

                    triangles[triangleIndex++] = bottomLeftIndex;
                    triangles[triangleIndex++] = topLeftIndex;
                    triangles[triangleIndex++] = topRightIndex;

                    triangles[triangleIndex++] = bottomLeftIndex;
                    triangles[triangleIndex++] = topRightIndex;
                    triangles[triangleIndex++] = bottomRightIndex;
                }
            }
        }
    }

    private void TriangulateHorizontal(
        int widthPlusOne,
        int heightPlusOne,
        ref int triangleIndex,
        ref int vertexIndex,
        int[] triangles,
        Vector3[] vertices
    )
    {
        for (int i = 0; i < heightPlusOne; ++i)
        {
            // First row

            for (int j = 0; j < widthPlusOne; ++j)
            {
                vertices[vertexIndex++] = Origin + new Vector3(j * CellSize, i * CellSize);
            }

            // Second row, separated vertically by line thickness only

            for (int j = 0; j < widthPlusOne; ++j)
            {
                vertices[vertexIndex++] = Origin + new Vector3(j * CellSize, (i * CellSize) + LineThickness);
            }
        }

        TriangulateLines(true, GridWidth, heightPlusOne, 0, ref triangleIndex, triangles);
    }

    private void TriangulateVertical(
        int widthPlusOne,
        int heightPlusOne,
        ref int triangleIndex,
        ref int vertexIndex,
        int[] triangles,
        Vector3[] vertices
    )
    {
        int horizontalMaximum = vertexIndex;

        for (int i = 0; i < widthPlusOne; ++i)
        {
            // First column

            for (int j = 0; j < heightPlusOne; ++j)
            {
                vertices[vertexIndex++] = new Vector3(i * CellSize, j * CellSize);
            }

            //Second column, separated horizontally by line thickness only

            for (int j = 0; j < heightPlusOne; ++j)
            {
                vertices[vertexIndex++] = new Vector3((i * CellSize) + LineThickness, j * CellSize);
            }
        }

        TriangulateLines(false, GridHeight, widthPlusOne, horizontalMaximum, ref triangleIndex, triangles);
    }

    public void InitializeGridGeometry()
    {
        // Count one more vertex than the cell count in each direction

        int widthPlusOne    = GridWidth + 1;
        int heightPlusOne   = GridHeight + 1;

        // 2 rows * 2 columns * (cell_count_x + 1) * (cell_count_y + 1) + top_right_corner

        Vector3[] vertices  = new Vector3[widthPlusOne * heightPlusOne * 4 + 1];

        // 2 triangles per cell * 2 * 6 indices * cell_count_x * cell_count_y

        int[] triangles     = new int[GridWidth * GridHeight * 6 * 4];
        int vertexIndex     = 0;
        int triangleIndex   = 0;

        TriangulateHorizontal(widthPlusOne, heightPlusOne, ref triangleIndex, ref vertexIndex, triangles, vertices);
        int horizontalVertexCount = vertexIndex;
        TriangulateVertical(widthPlusOne, heightPlusOne, ref triangleIndex, ref vertexIndex, triangles, vertices);

        int lastBottomRightIndex    = vertexIndex - 1;
        int lastTopLeftIndex        = horizontalVertexCount - 1;
        int lastBottomLeftIndex     = lastTopLeftIndex - widthPlusOne;
        int lastTopRightIndex       = vertexIndex++;

        triangles[triangleIndex++] = lastBottomLeftIndex;
        triangles[triangleIndex++] = lastTopLeftIndex;
        triangles[triangleIndex++] = lastTopRightIndex;

        triangles[triangleIndex++] = lastBottomLeftIndex;
        triangles[triangleIndex++] = lastTopRightIndex;
        triangles[triangleIndex++] = lastBottomRightIndex;

        vertices[lastTopRightIndex] = new Vector3((GridWidth * CellSize) + LineThickness, (GridHeight * CellSize) + LineThickness);

        Geometry.Clear();
        Geometry.vertices   = vertices;
        Geometry.triangles  = triangles;
    }
};