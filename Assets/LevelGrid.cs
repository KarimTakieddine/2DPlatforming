using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// It is an error to define a default (parameterless) constructor for a struct.
/// It is also an error to initialize an instance field in a struct body.
/// 
/// One may initialize externally accessible struct members only by using a
/// parameterized constructor, the implicit, default constructor, an object
/// initializer, or by accessing the members individually after the struct
/// is declared.
/// 
/// </summary>

public struct Cell
{
    public int TopLeft;
    public int TopRight;
    public int BottomRight;
    public int BottomLeft;

    public Cell(
        int stride,
        int count,
        int position,
        int offset
    )
    {
        TopLeft     = count * stride + position + offset;
        TopRight    = TopLeft + 1;
        BottomLeft  = TopLeft + stride;
        BottomRight = BottomLeft + 1;
    }
};

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
                    Cell cell = new Cell(width + 1, i, j, offset);

                    triangles[triangleIndex++] = cell.TopLeft;
                    triangles[triangleIndex++] = cell.TopRight;
                    triangles[triangleIndex++] = cell.BottomRight;

                    triangles[triangleIndex++] = cell.TopLeft;
                    triangles[triangleIndex++] = cell.BottomRight;
                    triangles[triangleIndex++] = cell.BottomLeft;
                }
            }
        }
        else
        {
            for (int i = 0; i < height * 2; i += 2)
            {
                for (int j = 0; j < width; ++j)
                {
                    Cell cell = new Cell(width + 1, i, j, offset);

                    triangles[triangleIndex++] = cell.BottomLeft;
                    triangles[triangleIndex++] = cell.BottomRight;
                    triangles[triangleIndex++] = cell.TopRight;

                    triangles[triangleIndex++] = cell.BottomLeft;
                    triangles[triangleIndex++] = cell.TopRight;
                    triangles[triangleIndex++] = cell.TopLeft;
                }
            }
        }
    }

    private void LoadVertexRows(
        int strideY,
        int strideX,
        ref int vertexIndex,
        Vector3[] vertices
    )
    {
        for (int i = 0; i < strideY; ++i)
        {
            // First row

            for (int j = 0; j < strideX; ++j)
            {
                vertices[vertexIndex++] = Origin + new Vector3(j * CellSize, i * CellSize);
            }

            // Second row, separated vertically by line thickness only

            for (int j = 0; j < strideX; ++j)
            {
                vertices[vertexIndex++] = Origin + new Vector3(j * CellSize, (i * CellSize) + LineThickness);
            }
        }
    }

    private void LoadVertexColumns(
        int strideY,
        int strideX,
        ref int vertexIndex,
        Vector3[] vertices
    )
    {
        for (int i = 0; i < strideX; ++i)
        {
            // First column

            for (int j = 0; j < strideY; ++j)
            {
                vertices[vertexIndex++] = Origin + new Vector3(i * CellSize, j * CellSize);
            }

            //Second column, separated horizontally by line thickness only

            for (int j = 0; j < strideY; ++j)
            {
                vertices[vertexIndex++] = Origin + new Vector3((i * CellSize) + LineThickness, j * CellSize);
            }
        }
    }

    private void TriangulateHorizontal(
        int width,
        int height,
        int offset,
        ref int triangleIndex,
        ref int vertexIndex,
        int[] triangles,
        Vector3[] vertices
    )
    {
        LoadVertexRows(height + 1, width + 1, ref vertexIndex, vertices);
        TriangulateLines(false, width, height + 1, offset, ref triangleIndex, triangles);
    }

    private void TriangulateVertical(
        int width,
        int height,
        int offset,
        ref int triangleIndex,
        ref int vertexIndex,
        int[] triangles,
        Vector3[] vertices
    )
    {
        LoadVertexColumns(height + 1, width + 1, ref vertexIndex, vertices);
        TriangulateLines(true, height, width + 1, offset, ref triangleIndex, triangles);
    }

    public void InitializeGridGeometry()
    {
        // Count one more vertex than the cell count in each direction

        // 2 rows * 2 columns * (cell_count_x + 1) * (cell_count_y + 1) + top_right_corner

        Vector3[] vertices  = new Vector3[(GridWidth + 1) * (GridHeight + 1) * 4 + 1];
        int[] triangles     = new int[GridWidth * GridHeight * 12 + GridWidth * 6 + GridHeight * 6 + 6];
        int vertexIndex     = 0;
        int triangleIndex   = 0;

        TriangulateHorizontal(GridWidth, GridHeight, vertexIndex, ref triangleIndex, ref vertexIndex, triangles, vertices);

        int topLeftIndex = vertexIndex - 1;

        TriangulateVertical(GridWidth, GridHeight, vertexIndex, ref triangleIndex, ref vertexIndex, triangles, vertices);

        int topRightIndex       = vertexIndex;
        int bottomRightIndex    = topRightIndex - 1;
        int bottomLeftIndex     = bottomRightIndex - GridWidth - 1;
        vertices[topRightIndex] = new Vector3((GridWidth * CellSize) + LineThickness, (GridHeight * CellSize) + LineThickness);

        triangles[triangleIndex++] = topLeftIndex;
        triangles[triangleIndex++] = topRightIndex;
        triangles[triangleIndex++] = bottomRightIndex;

        triangles[triangleIndex++] = topLeftIndex;
        triangles[triangleIndex++] = bottomRightIndex;
        triangles[triangleIndex++] = bottomLeftIndex;

        Debug.Log(triangleIndex);

        Geometry.Clear();
        Geometry.vertices   = vertices;
        Geometry.triangles  = triangles;
    }
};