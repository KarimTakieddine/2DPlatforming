using UnityEngine;
using System.Collections.Generic;
using System.Linq;

//Perhaps optimize into a structure of ushorts as flag bits instead?

enum PixelDirectionFlags
{
    NONE    = 0x00,  //0b00000000
    LEFT    = 0x01,  //0b00000001
    RIGHT   = 0x02,  //0b00000010
    UP      = 0x04,  //0b00000100
    DOWN    = 0x08   //0b00001000
};

[RequireComponent(typeof(PixelTile))]
public class PixelCharacter : MonoBehaviour
{
    public static List<PixelTile>   PixelTileObstacleList       { get; private set; }
    public PixelLevel               CurrentPixelLevelInstance   { get; private set; }
    public PixelTile                PixelTileComponent          { get; private set; }
    public int                      PixelPositionX              { get; private set; }
    public int                      PixelPositionY              { get; private set; }
    public int                      CurrentPixelTileCount       { get; private set; }
    
    private PixelDirectionFlags     PixelDirectionFlag;

    public int PixelVelocityX, PixelVelocityY;

    protected virtual void InitializePixelTileObstacleList()
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

    protected virtual void ComputeVelocity()
    {

    }

    private void SetHorizontalDirectionFlag()
    {
        if (PixelVelocityX > 0)
        {
            PixelDirectionFlag |= PixelDirectionFlags.RIGHT;
            PixelDirectionFlag &= ~PixelDirectionFlags.LEFT;
        }
        else if (PixelVelocityX < 0)
        {
            PixelDirectionFlag |= PixelDirectionFlags.LEFT;
            PixelDirectionFlag &= ~PixelDirectionFlags.RIGHT;
        }
        else
        {
            //Neutral

            PixelDirectionFlag &= ~PixelDirectionFlags.LEFT;
            PixelDirectionFlag &= ~PixelDirectionFlags.RIGHT;
        }
    }

    private void SetVerticalDirectionFlag()
    {
        if (PixelVelocityY > 0)
        {
            PixelDirectionFlag |= PixelDirectionFlags.UP;
            PixelDirectionFlag &= ~PixelDirectionFlags.DOWN;
        }
        else if (PixelVelocityY < 0)
        {
            PixelDirectionFlag |= PixelDirectionFlags.DOWN;
            PixelDirectionFlag &= ~PixelDirectionFlags.UP;
        }
        else
        {
            //Neutral

            PixelDirectionFlag &= ~PixelDirectionFlags.UP;
            PixelDirectionFlag &= ~PixelDirectionFlags.DOWN;
        }
    }

    private void SetDirectionFlags()
    {
        SetHorizontalDirectionFlag();
        SetVerticalDirectionFlag();
    }

    private void GetPredictedPixelPositions(
        ref int predictedPositionX,
        ref int predictedPositionY,
        int pixelPositionX,
        int pixelPositionY,
        int tileSizeX,
        int tileSizeY,
        int pixelsPerUnit
    )
    {
        if ((PixelDirectionFlag & PixelDirectionFlags.LEFT) == PixelDirectionFlags.LEFT)
        {
            predictedPositionX = pixelPositionX;
        }
        else if ((PixelDirectionFlag & PixelDirectionFlags.RIGHT) == PixelDirectionFlags.RIGHT)
        {
            predictedPositionX = pixelPositionX + tileSizeX * pixelsPerUnit;
        }

        if ((PixelDirectionFlag & PixelDirectionFlags.UP) == PixelDirectionFlags.UP)
        {
            predictedPositionY = pixelPositionY + tileSizeY * pixelsPerUnit;
        }

        if ((PixelDirectionFlag & PixelDirectionFlags.DOWN) == PixelDirectionFlags.DOWN)
        {
            predictedPositionY = pixelPositionY;
        }
    }

    private void GetObstaclePixelPositions(
        ref int obstaclePositionX,
        ref int obstaclePositionY,
        int pixelPositionX,
        int pixelPositionY,
        int tileSizeX,
        int tileSizeY,
        int pixelsPerUnit
    )
    {
        if ((PixelDirectionFlag & PixelDirectionFlags.LEFT) == PixelDirectionFlags.LEFT)
        {
            obstaclePositionX = pixelPositionX + tileSizeX * pixelsPerUnit;
        }
        else if ((PixelDirectionFlag & PixelDirectionFlags.RIGHT) == PixelDirectionFlags.RIGHT)
        {
            obstaclePositionX = pixelPositionX;
        }

        if ((PixelDirectionFlag & PixelDirectionFlags.UP) == PixelDirectionFlags.UP)
        {
            obstaclePositionY = pixelPositionY;
        }

        if ((PixelDirectionFlag & PixelDirectionFlags.DOWN) == PixelDirectionFlags.DOWN)
        {
            obstaclePositionY = pixelPositionY + tileSizeY * pixelsPerUnit;
        }
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

        int pixelsPerUnit   = currentPixelLevelInstance.PixelsPerUnit;
        PixelPositionX      = currentPixelLevelInstance.AlignedPixelOriginX + PixelTileComponent.AlignedRelativePositionX * pixelsPerUnit;
        PixelPositionY      = currentPixelLevelInstance.AlignedPixelOriginY + PixelTileComponent.AlignedRelativePositionY * pixelsPerUnit;
    }

    private void DetectAndResolveCollisions()
    {
        int pixelsPerUnit       = CurrentPixelLevelInstance.PixelsPerUnit;
        int predictedPositionX  = 0;
        int predictedPositionY  = 0;
        int tileSizeX           = PixelTileComponent.TileSizeX;
        int tileSizeY           = PixelTileComponent.TileSizeY;

        for (int j = 0; j < PixelTileObstacleList.Count; ++j)
        {
            PixelTile obstaclePixelTileComponent    = PixelTileObstacleList[j];
            GetPredictedPixelPositions(ref predictedPositionX, ref predictedPositionY, PixelPositionX, PixelPositionY, tileSizeX, tileSizeY, pixelsPerUnit);

            int obstacleMinimumX                    = CurrentPixelLevelInstance.AlignedPixelOriginX + obstaclePixelTileComponent.AlignedRelativePositionX * pixelsPerUnit;
            int obstacleMaximumX                    = obstacleMinimumX + obstaclePixelTileComponent.TileSizeX * pixelsPerUnit;
            int obstacleMinimumY                    = CurrentPixelLevelInstance.AlignedPixelOriginY + obstaclePixelTileComponent.AlignedRelativePositionY * pixelsPerUnit;
            int obstacleMaximumY                    = obstacleMinimumY + obstaclePixelTileComponent.TileSizeY * pixelsPerUnit;

            int pixelMinimumX                       = PixelPositionX;
            int pixelMaximumX                       = PixelPositionX + tileSizeX * pixelsPerUnit;
            int pixelMinimumY                       = PixelPositionY;
            int pixelMaximumY                       = PixelPositionY + tileSizeY * pixelsPerUnit;

            /*
            int obstaclePixelPositionX              = CurrentPixelLevelInstance.AlignedPixelOriginX + obstaclePixelTileComponent.AlignedRelativePositionX * pixelsPerUnit;
            int obstaclePixelPositionY              = CurrentPixelLevelInstance.AlignedPixelOriginY + obstaclePixelTileComponent.AlignedRelativePositionY * pixelsPerUnit;
            int obstaclePositionX                   = 0;
            int obstaclePositionY                   = 0;
            */
            
            if (
                obstacleMinimumY > pixelMaximumY   ||
                pixelMinimumY > obstacleMaximumY   ||
                obstacleMinimumX > pixelMaximumX   ||
                pixelMinimumX > obstacleMaximumX
            )
            {
                continue;
            }

            int firstPixelDifferenceX    = pixelMaximumX - obstacleMinimumX;
            int secondPixelDifferenceX   = obstacleMaximumX - pixelMinimumX;
            int firstPixelDifferenceY    = pixelMaximumY - obstacleMinimumY;
            int secondPixelDifferenceY   = obstacleMaximumY - pixelMinimumY;

            int[] differences       = new int[4] { firstPixelDifferenceX, secondPixelDifferenceX, firstPixelDifferenceY, secondPixelDifferenceY };
            int differenceMinimum   = differences.Min();

            if (differenceMinimum == differences[0])
            {
                PixelPositionX -= differences[0];
            }
            if (differenceMinimum == differences[1])
            {
                PixelPositionX += differences[1];
            }
            if (differenceMinimum == differences[2])
            {
                PixelPositionY -= differences[2];
            }
            if (differenceMinimum == differences[3])
            {
                PixelPositionY += differences[3];
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
        SetDirectionFlags();
        DetectAndResolveCollisions();

        int pixelsPerUnit   = currentPixelLevelInstance.PixelsPerUnit;
        transform.position  = Vector3.MoveTowards(
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