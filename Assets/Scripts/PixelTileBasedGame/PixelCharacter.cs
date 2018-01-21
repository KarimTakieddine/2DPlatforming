using UnityEngine;

[RequireComponent(typeof(PixelTile))]
public class PixelCharacter : MonoBehaviour
{
    public PixelTile    PixelTileComponent  { get; private set; }
    public int          PixelPositionX      { get; private set; }
    public int          PixelPositionY      { get; private set; }

    public int PixelVelocityX, PixelVelocityY;

    private void Start()
    {
        PixelTileComponent                      = GetComponent<PixelTile>();
        PixelLevel currentPixelLevelInstance    = PixelTileComponent.CurrentPixelLevelInstance;

        if (!currentPixelLevelInstance)
        {
            return;
        }

        transform.position = Vector2.zero;
        PixelTileComponent.AlignToPixelLevelGrid();

        uint pixelsPerUnit  = currentPixelLevelInstance.PixelsPerUnit;
        PixelPositionX      = (int)(currentPixelLevelInstance.AlignedPixelOriginX + (PixelTileComponent.AlignedRelativePositionX * pixelsPerUnit));
        PixelPositionY      = (int)(currentPixelLevelInstance.AlignedPixelOriginY + (PixelTileComponent.AlignedRelativePositionY * pixelsPerUnit));
    }

    private void Update()
    {
        PixelLevel currentPixelLevelInstance = PixelTileComponent.CurrentPixelLevelInstance;

        if (!currentPixelLevelInstance)
        {
            return;
        }

        PixelPositionX += PixelVelocityX;

        uint pixelsPerUnit = currentPixelLevelInstance.PixelsPerUnit;
        transform.position = new Vector2(
            ((float)PixelPositionX / pixelsPerUnit) + 0.5f * PixelTileComponent.TileSizeX,
            ((float)PixelPositionY / pixelsPerUnit) + 0.5f * PixelTileComponent.TileSizeY
       );
    }
};