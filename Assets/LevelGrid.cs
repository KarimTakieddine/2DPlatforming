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

public struct IndexCell
{
    const int INDEX_COUNT = 4;

    public int TopLeft;
    public int TopRight;
    public int BottomRight;
    public int BottomLeft;

    public IndexCell(
        int position,
        int offset
    )
    {

        BottomLeft  = position + offset;
        BottomRight = BottomLeft + 1;
        TopRight    = BottomRight + 1;
        TopLeft     = TopRight + 1;
    }
};

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class LevelGrid : MonoBehaviour
{
    public MeshFilter   Filter          { get; private set; }
    public Mesh         Geometry        { get; private set; }
    public Vector2      Origin          { get; private set; }
    public float        HalfLineWidth   { get; private set; }

    public Vector2 Offset;
    public int GridWidth, GridHeight;
    public int CellSize;

    [Range(0.0f, 1.0f)]
    public float LineThickness;

    private void Awake()
    {
        Filter              = GetComponent<MeshFilter>();
        Geometry            = new Mesh();
        Filter.sharedMesh   = Geometry;
    }

    private void Update()
    {
        if (GridWidth == 0 || GridHeight == 0)
        {
            return;
        }

        HalfLineWidth   = LineThickness * 0.5f;
        Origin          = transform.position;
        Origin          += Offset;

        InitializeGridGeometry();
    }

    private void LoadVertexRows(
        int width,
        int height,
        ref int vertexIndex,
        Vector3[] vertices
    )
    {
        float rightPositionX = width * CellSize + HalfLineWidth;

        for (int i = 0; i < height; ++i)
        {
            float bottomPositionY   = i * CellSize - HalfLineWidth;
            float topPositionY      = i * CellSize + HalfLineWidth;

            vertices[vertexIndex++] = Origin + new Vector2(0.0f, bottomPositionY);
            vertices[vertexIndex++] = Origin + new Vector2(rightPositionX, bottomPositionY);
            vertices[vertexIndex++] = Origin + new Vector2(rightPositionX, topPositionY);
            vertices[vertexIndex++] = Origin + new Vector2(0.0f, topPositionY);
        }
    }

    private void LoadVertexColumns(
        int width,
        int height,
        ref int vertexIndex,
        Vector3[] vertices
    )
    {
        float topPositionY = height * CellSize + HalfLineWidth;

        for (int i = 0; i < width; ++i)
        {
            float leftPositionX     = (i * CellSize) - HalfLineWidth;
            float rightPositionX    = (i * CellSize) + HalfLineWidth;

            vertices[vertexIndex++] = Origin + new Vector2(leftPositionX, topPositionY);
            vertices[vertexIndex++] = Origin + new Vector2(leftPositionX, 0.0f);
            vertices[vertexIndex++] = Origin + new Vector2(rightPositionX, 0.0f);
            vertices[vertexIndex++] = Origin + new Vector2(rightPositionX, topPositionY);
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
        LoadVertexRows(width, height, ref vertexIndex, vertices);

        for (int i = 0; i < height; i += 1)
        {
            IndexCell indexCell = new IndexCell(i * 4, offset);

            triangles[triangleIndex++] = indexCell.TopLeft;
            triangles[triangleIndex++] = indexCell.TopRight;
            triangles[triangleIndex++] = indexCell.BottomRight;

            triangles[triangleIndex++] = indexCell.TopLeft;
            triangles[triangleIndex++] = indexCell.BottomRight;
            triangles[triangleIndex++] = indexCell.BottomLeft;
        }
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
        LoadVertexColumns(width, height, ref vertexIndex, vertices);

        for (int i = 0; i < width; i += 1)
        {
            IndexCell indexCell = new IndexCell(i * 4, offset);

            triangles[triangleIndex++] = indexCell.TopLeft;
            triangles[triangleIndex++] = indexCell.TopRight;
            triangles[triangleIndex++] = indexCell.BottomRight;

            triangles[triangleIndex++] = indexCell.TopLeft;
            triangles[triangleIndex++] = indexCell.BottomRight;
            triangles[triangleIndex++] = indexCell.BottomLeft;
        }
    }

    public void InitializeGridGeometry()
    {
        int vertexCountX        = GridWidth + 1;
        int vertexCountY        = GridHeight + 1;
        int totalVertexCount    = (vertexCountX + vertexCountY) * 4;    
        Vector3[] vertices      = new Vector3[totalVertexCount];
        int[] triangles         = new int[(int)(totalVertexCount * 1.5f)];

        int vertexIndex     = 0;
        int triangleIndex   = 0;

        TriangulateVertical(vertexCountX, GridHeight, 0, ref triangleIndex, ref vertexIndex, triangles, vertices);
        TriangulateHorizontal(GridWidth, vertexCountY, vertexIndex, ref triangleIndex, ref vertexIndex, triangles, vertices);

        Geometry.Clear();
        Geometry.vertices   = vertices;
        Geometry.triangles  = triangles;
    }
};