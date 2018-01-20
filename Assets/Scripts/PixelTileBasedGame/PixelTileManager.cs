using UnityEngine;

public class PixelTileManager : MonoBehaviour
{
	public static void SnapAllTilesToGrid()
    {
        PixelTile[] AllPixelTiles   = FindObjectsOfType<PixelTile>();
        int pixelTileCount          = AllPixelTiles.Length;

        if (pixelTileCount == 0)
        {
            return;
        }

        for (int i = 0; i < pixelTileCount; ++i)
        {
            PixelTile pixelTileComponent    = AllPixelTiles[i];
            Transform pixelTileTransform    = pixelTileComponent.transform;
            Vector3 position                = pixelTileTransform.position;

            int levelOffsetX                = Mathf.RoundToInt(position.x - PixelLevel.GlobalInstance.OriginX - 0.5f);
            int levelOffsetY                = Mathf.RoundToInt(position.y - PixelLevel.GlobalInstance.OriginY - 0.5f);
            pixelTileTransform.position     = new Vector2((levelOffsetX < 0 ? 0 : levelOffsetX) + 0.5f, (levelOffsetY < 0 ? 0 : levelOffsetY) + 0.5f);
        }
	}
};