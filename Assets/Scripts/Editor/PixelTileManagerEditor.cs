using System.Collections;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PixelTileManager))]
public class PixelTileManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PixelTileManager instance = (PixelTileManager)target;

        if (GUILayout.Button("Snap all Tiles to Grid"))
        {
            PixelTileManager.SnapAllTilesToGrid();
        }
    }
};