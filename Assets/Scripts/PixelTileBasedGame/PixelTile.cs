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

    public int                 AlignedRelativePositionX     { get; private set; }
    public int                 AlignedRelativePositionY     { get; private set; }
    public int                 TileSizeX, TileSizeY;
    public LevelFlags          LevelFlags;

    //In order to avoid casting when performing integer operations on position

    private void ValidateState()
    {
        if (TileSizeX < 0)
        {
            TileSizeX = 0;
        }
        if (TileSizeY < 0)
        {
            TileSizeY = 0;
        }
    }

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
        int levelPixelsPerUnit      = CurrentPixelLevelInstance.PixelsPerUnit;
        int levelAlignedOriginX     = CurrentPixelLevelInstance.PixelOriginX / levelPixelsPerUnit;
        int levelAlignedOriginY     = CurrentPixelLevelInstance.PixelOriginY / levelPixelsPerUnit;
        int alignedRelPositionX     = alignedAbsPositionX - levelAlignedOriginX;
        int alignedRelPositionY     = alignedAbsPositionY - levelAlignedOriginY;
        AlignedRelativePositionX    = (alignedRelPositionX < 0 ? 0 : alignedRelPositionX);
        AlignedRelativePositionY    = (alignedRelPositionY < 0 ? 0 : alignedRelPositionY);

        transform.position          = new Vector3(
            levelAlignedOriginX + AlignedRelativePositionX + halfTileWidth,
            levelAlignedOriginY + AlignedRelativePositionY + halfTileHeight
        );
    }

    private void Awake()
    {
        ValidateState();
    }
};