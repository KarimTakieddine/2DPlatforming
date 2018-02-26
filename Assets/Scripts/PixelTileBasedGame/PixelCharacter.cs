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
    private float VelocityX, VelocityY, JumpStateTimer;
    private bool isJumping;

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
        VelocityX = PixelVelocityX * Time.deltaTime;

        float durationToPeak    = 2304.0f / PixelVelocityX;
        float initialVelocityY  = 48.0f * 2.0f / durationToPeak;
        float gravity           = -256.0f * 2.0f / (durationToPeak * durationToPeak);

        VelocityY = isJumping ? gravity * JumpStateTimer + initialVelocityY : PixelVelocityY * Time.deltaTime;
    }

    private void SetHorizontalDirectionFlag()
    {
        if (VelocityX > 0.0f)
        {
            PixelDirectionFlag |= PixelDirectionFlags.RIGHT;
            PixelDirectionFlag &= ~PixelDirectionFlags.LEFT;
        }
        else if (VelocityX < 0.0f)
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
        if (VelocityY > 0.0f)
        {
            PixelDirectionFlag |= PixelDirectionFlags.UP;
            PixelDirectionFlag &= ~PixelDirectionFlags.DOWN;
        }
        else if (VelocityY < 0.0f)
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
    
    private void Start()
    {
        PixelTileComponent                      = GetComponent<PixelTile>();
        PixelLevel currentPixelLevelInstance    = PixelTileComponent.CurrentPixelLevelInstance;

        if (!currentPixelLevelInstance)
        {
            return;
        }

        CurrentPixelLevelInstance = currentPixelLevelInstance;
        PixelTileComponent.AlignToPixelLevelGrid();

        int pixelsPerUnit   = currentPixelLevelInstance.PixelsPerUnit;
        PixelPositionX      = currentPixelLevelInstance.PixelOriginX + PixelTileComponent.AlignedRelativePositionX * pixelsPerUnit;
        PixelPositionY      = currentPixelLevelInstance.PixelOriginY + PixelTileComponent.AlignedRelativePositionY * pixelsPerUnit;
        JumpStateTimer      = 0.0f;
        isJumping           = false;
    }

    private void DetectAndResolveCollisions()
    {
        int pixelsPerUnit   = CurrentPixelLevelInstance.PixelsPerUnit;
        int tileSizeX       = PixelTileComponent.TileSizeX;
        int tileSizeY       = PixelTileComponent.TileSizeY;

        for (int j = 0; j < PixelTileObstacleList.Count; ++j)
        {
            PixelTile obstaclePixelTileComponent    = PixelTileObstacleList[j];

            int obstacleMinimumX                    = CurrentPixelLevelInstance.PixelOriginX + obstaclePixelTileComponent.AlignedRelativePositionX * pixelsPerUnit;
            int obstacleMaximumX                    = obstacleMinimumX + obstaclePixelTileComponent.TileSizeX * pixelsPerUnit;
            int obstacleMinimumY                    = CurrentPixelLevelInstance.PixelOriginY + obstaclePixelTileComponent.AlignedRelativePositionY * pixelsPerUnit;
            int obstacleMaximumY                    = obstacleMinimumY + obstaclePixelTileComponent.TileSizeY * pixelsPerUnit;

            int pixelMinimumX                       = PixelPositionX;
            int pixelMaximumX                       = PixelPositionX + tileSizeX * pixelsPerUnit;
            int pixelMinimumY                       = PixelPositionY;
            int pixelMaximumY                       = PixelPositionY + tileSizeY * pixelsPerUnit;
            
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

        if (Input.GetButtonDown("Jump"))
        {
            isJumping       = true;
            JumpStateTimer  = 0.0f;
        }

        ComputeVelocity();

        PixelPositionX += Mathf.RoundToInt(VelocityX);
        PixelPositionY += Mathf.RoundToInt(VelocityY);

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

        JumpStateTimer += Time.deltaTime;
    }
};