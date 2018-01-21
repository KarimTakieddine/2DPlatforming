using UnityEngine;

public enum LevelFlags
{
    NONE        = 0x00000000,
    OBSTACLE    = 0x00000001,
    CHARACTER   = 0x00000002
};

[RequireComponent(typeof(SpriteRenderer))]
public class PixelTile : MonoBehaviour
{
    public PixelLevel           CurrentPixelLevelInstance;
    public SpriteRenderer       SpriteRendererComponent     { get; private set; }
    public Sprite               SpriteComponent             { get; private set; }

    public uint                 AlignedRelativePositionX    { get; private set; }
    public uint                 AlignedRelativePositionY    { get; private set; }
    public uint                 TileSizeX, TileSizeY;
    public LevelFlags           LevelFlags;

    public void AlignToPixelLevelGrid()
    {
        SpriteRendererComponent = GetComponent<SpriteRenderer>();
        SpriteComponent         = SpriteRendererComponent.sprite;

        if (!SpriteComponent)
        {
            return;
        }

        if (!CurrentPixelLevelInstance)
        {
            return;
        }

        Texture2D spriteTexture     = SpriteComponent.texture;
        float spritePixelsPerUnit   = SpriteComponent.pixelsPerUnit;

        transform.localScale        = new Vector3(
            TileSizeX * spritePixelsPerUnit / spriteTexture.width,
            TileSizeY * spritePixelsPerUnit / spriteTexture.height
        );

        float halfTileWidth         = 0.5f * TileSizeX;
        float halfTileHeight        = 0.5f * TileSizeY;

        Vector3 position            = transform.position;
        int alignedAbsPositionX     = Mathf.RoundToInt(position.x - halfTileWidth);
        int alignedAbsPositionY     = Mathf.RoundToInt(position.y - halfTileHeight);
        uint levelPixelsPerUnit     = CurrentPixelLevelInstance.PixelsPerUnit;
        uint levelAlignedOriginX    = CurrentPixelLevelInstance.AlignedPixelOriginX / levelPixelsPerUnit;
        uint levelAlignedOriginY    = CurrentPixelLevelInstance.AlignedPixelOriginY / levelPixelsPerUnit;
        int alignedRelPositionX     = alignedAbsPositionX - (int)levelAlignedOriginX;
        int alignedRelPositionY     = alignedAbsPositionY - (int)levelAlignedOriginY;
        AlignedRelativePositionX    = (alignedRelPositionX < 0 ? 0 : (uint)alignedRelPositionX);
        AlignedRelativePositionY    = (alignedRelPositionY < 0 ? 0 : (uint)alignedRelPositionY);

        transform.position          = new Vector3(
            levelAlignedOriginX + AlignedRelativePositionX + halfTileWidth,
            levelAlignedOriginY + AlignedRelativePositionY + halfTileHeight
        );
    }
};