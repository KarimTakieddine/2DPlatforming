using UnityEngine;
using UnityEditor;

public static class PixelTileBasedMenus
{
    [MenuItem("GameObject/Pixel Tile Based Game/Pixel Level")]
    private static void CreatePixelLevelInstance ()
    {
        GameObject pixelLevelGameObject = new GameObject("Pixel Level");
        pixelLevelGameObject.AddComponent<PixelLevel>();
    }

    [MenuItem("GameObject/Pixel Tile Based Game/Pixel Tile")]
    private static void CreatePixelTileInstance()
    {
        GameObject pixelTileGameObject  = new GameObject("Pixel Tile");
        PixelTile pixelTileComponent    = pixelTileGameObject.AddComponent<PixelTile>();

        PixelLevel firstPixelLevelFound = GameObject.FindObjectOfType<PixelLevel>();

        if (firstPixelLevelFound)
        {
            pixelTileComponent.CurrentPixelLevelInstance = firstPixelLevelFound;
        }
    }
}