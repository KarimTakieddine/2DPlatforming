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
    public MeshFilter   MeshFilterComponent { get; private set; }
    public Mesh         MeshComponent       { get; private set; }

    public Vector2      Origin;
    public Vector2      CellSize;
    public int          HorizontalCellCount, VerticalCellCount;

    [Range(0.025f, 1.0f)]
    public float LineThickness;
    public float HalfLineWidth              { get; private set; }

    private void ValidateState()
    {
        if (HorizontalCellCount < 0)
        {
            HorizontalCellCount = 0;
        }

        if (VerticalCellCount < 0)
        {
            VerticalCellCount = 0;
        }
    }
    
    public void InitializeComponents()
    {
        MeshFilterComponent             = GetComponent<MeshFilter>();
        MeshComponent                   = new Mesh();
        MeshFilterComponent.sharedMesh  = MeshComponent;
    }

    public void UpdateComponents()
    {
        HalfLineWidth = LineThickness * 0.5f;
        InitializeMeshAsGrid();
    }

    public void Awake()
    {
        InitializeComponents();
        ValidateState();
    }

    public void Update()
    {
        UpdateComponents();
    }

    private void LoadVertexRows(
        int width,
        int height,
        ref int vertexIndex,
        Vector3[] vertices
    )
    {
        float rightPositionX    = width * CellSize.x + HalfLineWidth;
        float verticalCellSize  = CellSize.y;

        for (int i = 0; i < height; ++i)
        {
            float bottomPositionY   = i * verticalCellSize - HalfLineWidth;
            float topPositionY      = i * verticalCellSize + HalfLineWidth;

            vertices[vertexIndex++] = Origin + new Vector2(-HalfLineWidth, bottomPositionY);
            vertices[vertexIndex++] = Origin + new Vector2(rightPositionX, bottomPositionY);
            vertices[vertexIndex++] = Origin + new Vector2(rightPositionX, topPositionY);
            vertices[vertexIndex++] = Origin + new Vector2(-HalfLineWidth, topPositionY);
        }
    }

    private void LoadVertexColumns(
        int width,
        int height,
        ref int vertexIndex,
        Vector3[] vertices
    )
    {
        float topPositionY          = height * CellSize.y + HalfLineWidth;
        float horizontalCellSize    = CellSize.x;

        for (int i = 0; i < width; ++i)
        {
            float leftPositionX     = (i * horizontalCellSize) - HalfLineWidth;
            float rightPositionX    = (i * horizontalCellSize) + HalfLineWidth;

            vertices[vertexIndex++] = Origin + new Vector2(leftPositionX, topPositionY);
            vertices[vertexIndex++] = Origin + new Vector2(leftPositionX, 0.0f);
            vertices[vertexIndex++] = Origin + new Vector2(rightPositionX, 0.0f);
            vertices[vertexIndex++] = Origin + new Vector2(rightPositionX, topPositionY);
        }
    }

    public static void TriangulateIndices(
        int offset,
        int stride,
        ref int triangleIndex,
        int[] triangles
    )
    {
        for (int i = 0; i < stride; i += 1)
        {
            IndexCell indexCell         = new IndexCell(i * 4, offset);
            triangles[triangleIndex++]  = indexCell.TopLeft;
            triangles[triangleIndex++]  = indexCell.TopRight;
            triangles[triangleIndex++]  = indexCell.BottomRight;
            triangles[triangleIndex++]  = indexCell.TopLeft;
            triangles[triangleIndex++]  = indexCell.BottomRight;
            triangles[triangleIndex++]  = indexCell.BottomLeft;
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
        TriangulateIndices(offset, height, ref triangleIndex, triangles);
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
        TriangulateIndices(offset, width, ref triangleIndex, triangles);
    }

    public void InitializeMeshAsGrid()
    {
        int vertexCountX        = HorizontalCellCount + 1;
        int vertexCountY        = VerticalCellCount + 1;
        int totalVertexCount    = (vertexCountX + vertexCountY) * 4;    
        Vector3[] vertices      = new Vector3[totalVertexCount];
        int[] triangles         = new int[(int)(totalVertexCount * 1.5f)];

        int vertexIndex     = 0;
        int triangleIndex   = 0;

        TriangulateVertical(vertexCountX, VerticalCellCount, 0, ref triangleIndex, ref vertexIndex, triangles, vertices);
        TriangulateHorizontal(HorizontalCellCount, vertexCountY, vertexIndex, ref triangleIndex, ref vertexIndex, triangles, vertices);

        MeshComponent.Clear();
        MeshComponent.vertices   = vertices;
        MeshComponent.triangles  = triangles;
    }
};