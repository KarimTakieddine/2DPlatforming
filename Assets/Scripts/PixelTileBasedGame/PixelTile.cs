using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PixelTile : MonoBehaviour
{
    public SpriteRenderer       SpriteRendererComponent { get; private set; }
    public Sprite               SpriteComponent         { get; private set; }

    public uint                 TileSizeX, TileSizeY;

    private void Awake()
    {
        SpriteRendererComponent = GetComponent<SpriteRenderer>();
        SpriteComponent         = SpriteRendererComponent.sprite;
    }

    private void Update()
    {
        if (!SpriteComponent)
        {
            throw new UnityException("No Sprite2D instance assigned to PixelTime: " + name);
        }

        if (SpriteComponent.pixelsPerUnit != (float)PixelLevel.GlobalPixelsPerUnit)
        {
            throw new UnityException(
                "Assigned Sprite2D instance's \"Pixels per Unit settings\" are not a multiple of the global value" +
                PixelLevel.GlobalPixelsPerUnit +
                " for PixelTile: " + name
            );
        }

        transform.localScale = new Vector2(TileSizeX, TileSizeY);
    }
};