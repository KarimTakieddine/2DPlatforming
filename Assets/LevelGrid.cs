using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class LevelGrid : MonoBehaviour
{
    public MeshFilter   Filter      { get; private set; }
    public Mesh         Geometry    { get; private set; }

    public Vector3 Origin;
    public int GridWidth, GridHeight;
    public int CellSize;

    [Range(0.0f, 1.0f)]
    public float LineThickness;

	private void Update()
    {
        Filter      = GetComponent<MeshFilter>();
        Geometry    = new Mesh();
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

    public void InitializeGridGeometry()
    {
        int widthPlusOne    = GridWidth + 1;
        int heightPlusOne   = GridHeight + 1;
        Vector3[] vertices  = new Vector3[widthPlusOne * heightPlusOne * 4 + 1];
        int[] triangles     = new int[GridWidth * GridHeight * 6 * 4];
        int vertexIndex     = 0;
        int triangleIndex   = 0;


        for (int i = 0; i < heightPlusOne; ++i)
        {
            for (int j = 0; j < widthPlusOne; ++j)
            {
                vertices[vertexIndex++] = new Vector3(j, i);
            }

            for (int j = 0; j < widthPlusOne; ++j)
            {
                vertices[vertexIndex++] = new Vector3(j, i + LineThickness);
            }
        }

        for (int i = 0; i < heightPlusOne * 2; i += 2)
        {
            for (int j  = 0; j < GridWidth; ++j)
            {
                int topLeftIndex            = i * widthPlusOne + j;
                int topRightIndex           = topLeftIndex + 1;
                int bottomLeftIndex         = topLeftIndex + widthPlusOne;
                int bottomRightIndex        = bottomLeftIndex + 1;

                triangles[triangleIndex++]  = bottomLeftIndex;
                triangles[triangleIndex++]  = bottomRightIndex;
                triangles[triangleIndex++]  = topRightIndex;

                triangles[triangleIndex++]  = bottomLeftIndex;
                triangles[triangleIndex++]  = topRightIndex;
                triangles[triangleIndex++]  = topLeftIndex;
            }
        }

        int horizontalMaximum = vertexIndex;

        Debug.Log(horizontalMaximum);

        for (int i = 0; i < widthPlusOne; ++i)
        {
            for (int j = 0; j < heightPlusOne; ++j)
            {
                vertices[vertexIndex++] = new Vector3(i, j);
            }

            for (int j = 0; j < heightPlusOne; ++j)
            {
                vertices[vertexIndex++] = new Vector3(i + LineThickness, j);
            }
        }

        for (int i = 0; i < widthPlusOne * 2; i += 2)
        {
            for (int j = 0; j < GridHeight; ++j)
            {
                
                int bottomLeftIndex = horizontalMaximum + (i * heightPlusOne) + j;
                int bottomRightIndex = bottomLeftIndex + heightPlusOne;
                int topLeftIndex = bottomLeftIndex + 1;
                int topRightIndex = topLeftIndex + heightPlusOne;

                triangles[triangleIndex++] = bottomLeftIndex;
                triangles[triangleIndex++] = topLeftIndex;
                triangles[triangleIndex++] = topRightIndex;

                triangles[triangleIndex++] = bottomLeftIndex;
                triangles[triangleIndex++] = topRightIndex;
                triangles[triangleIndex++] = bottomRightIndex;
            }
        }

        int lastBottomRightIndex    = vertexIndex - 1;
        int lastTopLeftIndex        = horizontalMaximum - 1;
        int lastBottomLeftIndex     = lastTopLeftIndex - widthPlusOne;
        int lastTopRightIndex       = vertexIndex++;

        triangles[triangleIndex++] = lastBottomLeftIndex;
        triangles[triangleIndex++] = lastTopLeftIndex;
        triangles[triangleIndex++] = lastTopRightIndex;

        triangles[triangleIndex++] = lastBottomLeftIndex;
        triangles[triangleIndex++] = lastTopRightIndex;
        triangles[triangleIndex++] = lastBottomRightIndex;

        vertices[lastTopRightIndex] = new Vector3(GridWidth + LineThickness, GridHeight + LineThickness);

        Geometry.Clear();
        Geometry.vertices   = vertices;
        Geometry.triangles  = triangles;
    }
};