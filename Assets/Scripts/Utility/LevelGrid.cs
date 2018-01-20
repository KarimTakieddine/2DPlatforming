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
    const uint INDEX_COUNT = 4;

    public uint TopLeft;
    public uint TopRight;
    public uint BottomRight;
    public uint BottomLeft;

    public IndexCell(
        uint position,
        uint offset
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
    public MeshFilter   MeshFilterComponent { get; private set; }
    public Mesh         MeshComponent       { get; private set; }
    public Vector2      Origin              { get; private set; }

    public uint         HorizontalCellCount { get; set; }
    public uint         VerticalCellCount   { get; set; }
    public uint         CellSizeX           { get; set; }
    public uint         CellSizeY           { get; set; }

    public uint         OriginX             { get; set; }
    public uint         OriginY             { get; set; }

    public float        HalfLineWidth       { get; private set; }

    [Range(0.0f, 1.0f)]
    public float        LineThickness;

    public void InitializeComponents()
    {
        MeshFilterComponent             = GetComponent<MeshFilter>();
        MeshComponent                   = new Mesh();
        MeshFilterComponent.sharedMesh  = MeshComponent;
    }

    public void UpdateComponents()
    {
        Origin          = new Vector2(OriginX, OriginY);
        HalfLineWidth   = LineThickness * 0.5f;
        InitializeMeshAsGrid();
    }

    /*
        private void Awake()
        {
            InitializeComponents();
        }
    */


    /*
        private void Update()
        {
            UpdateComponents();
        }
    */

    private void LoadVertexRows(
        uint width,
        uint height,
        ref uint vertexIndex,
        Vector3[] vertices
    )
    {
        float rightPositionX = width * CellSizeX + HalfLineWidth;

        for (int i = 0; i < height; ++i)
        {
            float bottomPositionY   = i * CellSizeY - HalfLineWidth;
            float topPositionY      = i * CellSizeY + HalfLineWidth;

            vertices[vertexIndex++] = Origin + new Vector2(-HalfLineWidth, bottomPositionY);
            vertices[vertexIndex++] = Origin + new Vector2(rightPositionX, bottomPositionY);
            vertices[vertexIndex++] = Origin + new Vector2(rightPositionX, topPositionY);
            vertices[vertexIndex++] = Origin + new Vector2(-HalfLineWidth, topPositionY);
        }
    }

    private void LoadVertexColumns(
        uint width,
        uint height,
        ref uint vertexIndex,
        Vector3[] vertices
    )
    {
        float topPositionY = height * CellSizeY + HalfLineWidth;

        for (int i = 0; i < width; ++i)
        {
            float leftPositionX     = (i * CellSizeX) - HalfLineWidth;
            float rightPositionX    = (i * CellSizeX) + HalfLineWidth;

            vertices[vertexIndex++] = Origin + new Vector2(leftPositionX, topPositionY);
            vertices[vertexIndex++] = Origin + new Vector2(leftPositionX, 0.0f);
            vertices[vertexIndex++] = Origin + new Vector2(rightPositionX, 0.0f);
            vertices[vertexIndex++] = Origin + new Vector2(rightPositionX, topPositionY);
        }
    }

    public static void TriangulateIndices(
        uint offset,
        uint stride,
        ref uint triangleIndex,
        int[] triangles
    )
    {
        for (uint i = 0; i < stride; i += 1)
        {
            IndexCell indexCell         = new IndexCell(i * 4, offset);
            triangles[triangleIndex++]  = (int)indexCell.TopLeft;
            triangles[triangleIndex++]  = (int)indexCell.TopRight;
            triangles[triangleIndex++]  = (int)indexCell.BottomRight;
            triangles[triangleIndex++]  = (int)indexCell.TopLeft;
            triangles[triangleIndex++]  = (int)indexCell.BottomRight;
            triangles[triangleIndex++]  = (int)indexCell.BottomLeft;
        }
    }

    private void TriangulateHorizontal(
        uint width,
        uint height,
        uint offset,
        ref uint triangleIndex,
        ref uint vertexIndex,
        int[] triangles,
        Vector3[] vertices
    )
    {
        LoadVertexRows(width, height, ref vertexIndex, vertices);
        TriangulateIndices(offset, height, ref triangleIndex, triangles);
    }

    private void TriangulateVertical(
        uint width,
        uint height,
        uint offset,
        ref uint triangleIndex,
        ref uint vertexIndex,
        int[] triangles,
        Vector3[] vertices
    )
    {
        LoadVertexColumns(width, height, ref vertexIndex, vertices);
        TriangulateIndices(offset, width, ref triangleIndex, triangles);
    }

    public void InitializeMeshAsGrid()
    {
        uint vertexCountX       = HorizontalCellCount + 1;
        uint vertexCountY       = VerticalCellCount + 1;
        uint totalVertexCount   = (vertexCountX + vertexCountY) * 4;    
        Vector3[] vertices      = new Vector3[totalVertexCount];
        int[] triangles         = new int[(int)(totalVertexCount * 1.5f)];

        uint vertexIndex     = 0;
        uint triangleIndex   = 0;

        TriangulateVertical(vertexCountX, VerticalCellCount, 0, ref triangleIndex, ref vertexIndex, triangles, vertices);
        TriangulateHorizontal(HorizontalCellCount, vertexCountY, vertexIndex, ref triangleIndex, ref vertexIndex, triangles, vertices);

        MeshComponent.Clear();
        MeshComponent.vertices   = vertices;
        MeshComponent.triangles  = triangles;
    }
};