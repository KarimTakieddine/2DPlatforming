using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(PixelTile))]
public class PixelCharacter : MonoBehaviour
{
    public static List<PixelTile>   PixelTileObstacleList       { get; private set; }
    public PixelLevel               CurrentPixelLevelInstance   { get; private set; }
    public PixelTile                PixelTileComponent          { get; private set; }
    public int                      PixelPositionX              { get; private set; }
    public int                      PixelPositionY              { get; private set; }
    public int                      CurrentPixelTileCount       { get; private set; }

    public int PixelVelocityX, PixelVelocityY;

    public void InitializePixelTileObstacleList()
    {
        if (PixelTileObstacleList == null)
        {
            PixelTileObstacleList = new List<PixelTile>();
        }

        PixelTile[] allPixelTiles   = FindObjectsOfType<PixelTile>();
        int pixelTileCount          = allPixelTiles.Length;

        if (pixelTileCount == CurrentPixelTileCount)
        {
            return;
        }

        PixelTileObstacleList.Clear();

        for (int i = 0; i < pixelTileCount; ++i)
        {
            PixelTile pixelTileComponent = allPixelTiles[i];

            if ((pixelTileComponent.LevelFlags & LevelFlags.OBSTACLE) == LevelFlags.OBSTACLE)
            {
                PixelTileObstacleList.Add(pixelTileComponent);
            }
        }

        CurrentPixelTileCount = pixelTileCount;
    }

    private void Start()
    {
        PixelTileComponent                      = GetComponent<PixelTile>();
        PixelLevel currentPixelLevelInstance    = PixelTileComponent.CurrentPixelLevelInstance;

        if (!currentPixelLevelInstance)
        {
            return;
        }

        CurrentPixelLevelInstance   = currentPixelLevelInstance;
        transform.position          = Vector2.zero;
        PixelTileComponent.AlignToPixelLevelGrid();

        uint pixelsPerUnit  = currentPixelLevelInstance.PixelsPerUnit;
        PixelPositionX      = (int)(currentPixelLevelInstance.AlignedPixelOriginX + (PixelTileComponent.AlignedRelativePositionX * pixelsPerUnit));
        PixelPositionY      = (int)(currentPixelLevelInstance.AlignedPixelOriginY + (PixelTileComponent.AlignedRelativePositionY * pixelsPerUnit));
    }

    private void DetectAndResolveCollisions()
    {
        int integerPixelsPerUnit = (int)CurrentPixelLevelInstance.PixelsPerUnit;

        for (int j = 0; j < PixelTileObstacleList.Count; ++j)
        {
            PixelTile obstaclePixelTileComponent    = PixelTileObstacleList[j];
            int predictedPixelPositionX             = PixelPositionX + (int)PixelTileComponent.TileSizeX * integerPixelsPerUnit;
            int predictedPixelPositionY             = PixelPositionY + (int)PixelTileComponent.TileSizeY * integerPixelsPerUnit;
            int obstaclePixelPositionX              = (int)CurrentPixelLevelInstance.AlignedPixelOriginX + (int)obstaclePixelTileComponent.AlignedRelativePositionX * integerPixelsPerUnit;
            int obstaclePixelPositionY              = (int)CurrentPixelLevelInstance.AlignedPixelOriginY + (int)obstaclePixelTileComponent.AlignedRelativePositionY * integerPixelsPerUnit;

            if (
                obstaclePixelPositionY > predictedPixelPositionY                                                        ||
                PixelPositionY > obstaclePixelPositionY + obstaclePixelTileComponent.TileSizeY * integerPixelsPerUnit   ||
                obstaclePixelPositionX > predictedPixelPositionX                                                        ||
                PixelPositionX > obstaclePixelPositionX + obstaclePixelTileComponent.TileSizeX * integerPixelsPerUnit
            )
            {
                continue;
            }

            int pixelDifferenceX    = predictedPixelPositionX - obstaclePixelPositionX;
            int pixelDifferenceY    = predictedPixelPositionY - obstaclePixelPositionY;
            int differenceMinimum   = Mathf.Min(pixelDifferenceX, pixelDifferenceY);

            if (differenceMinimum == pixelDifferenceX)
            {
                PixelPositionX -= pixelDifferenceX;
            }
            else if (differenceMinimum == pixelDifferenceY)
            {
                PixelPositionY -= pixelDifferenceY;
            }
        }
    }

    private void Update()
    {
        PixelLevel currentPixelLevelInstance = PixelTileComponent.CurrentPixelLevelInstance;

        if (!currentPixelLevelInstance)
        {
            return;
        }

        CurrentPixelLevelInstance = currentPixelLevelInstance;

        int currentPixelPositionX = PixelPositionX;
        int currentPixelPositionY = PixelPositionY;

        PixelPositionY += Mathf.RoundToInt(PixelVelocityY * Time.deltaTime);
        PixelPositionX += Mathf.RoundToInt(PixelVelocityX * Time.deltaTime);

        InitializePixelTileObstacleList();
        DetectAndResolveCollisions();

        uint pixelsPerUnit = currentPixelLevelInstance.PixelsPerUnit;
        transform.position = Vector3.MoveTowards(
            new Vector3(
                ((float)currentPixelPositionX / pixelsPerUnit) + 0.5f * PixelTileComponent.TileSizeX,
                ((float)currentPixelPositionY / pixelsPerUnit) + 0.5f * PixelTileComponent.TileSizeY
            ),
            new Vector3(
            ((float)PixelPositionX / pixelsPerUnit) + 0.5f * PixelTileComponent.TileSizeX,
            ((float)PixelPositionY / pixelsPerUnit) + 0.5f * PixelTileComponent.TileSizeY
            ),
            Time.deltaTime
        );
    }
};