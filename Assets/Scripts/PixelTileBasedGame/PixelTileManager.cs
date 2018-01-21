using UnityEngine;

public class PixelTileManager : MonoBehaviour
{
	public static void SnapAllTilesToGrid()
    {
        PixelTile[] AllPixelTiles   = FindObjectsOfType<PixelTile>();
        int pixelTileCount          = AllPixelTiles.Length;

        for (int i = 0; i < pixelTileCount; ++i)
        {
            AllPixelTiles[i].AlignToPixelLevelGrid();
        }
	}
};