using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Flags]
public enum InputSourceFlags
{
    NONE        = 0,
    JOYSTICK    = 1,
    KEYBOARD    = 1 << 1
};

[System.Flags]
public enum CollisionNormalFlags
{
    NONE        = 0,
    LEFT_WALL   = 1,
    RIGHT_WALL  = 1 << 1,
    CEILING     = 1 << 2,
    GROUND      = 1 << 3
};

[RequireComponent(typeof(PixelTile))]
public class PixelCharacter : MonoBehaviour
{
    public static List<PixelTile>                   PixelTileObstacleList       { get; private set; }
    public Dictionary<int, CollisionNormalFlags>    CollisionNormalStateMap     { get; protected set; }
    public PixelLevel                               CurrentPixelLevelInstance   { get; private set; }
    public PixelTile                                PixelTileComponent          { get; private set; }
    public InputSourceFlags                         InputSourceFlag             { get; private set; }
    public CollisionNormalFlags                     CollisionNormalFlag         { get; protected set;}
    public int                                      PixelPositionX              { get; private set; }
    public int                                      PixelPositionY              { get; private set; }
    public int                                      CurrentPixelTileCount       { get; private set; }
    
    public int PixelVelocityX, PixelVelocityY;
    private float VelocityX, PreviousVelocityX, VelocityY, JumpStateTimer;
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

    private void SetInputSourceFlag(
        bool isKeyPressed,
        Vector2 josytickInput
    )
    {
        if (josytickInput.magnitude != 0.0f)
        {
            if ((InputSourceFlag & InputSourceFlags.KEYBOARD) == InputSourceFlags.KEYBOARD)
            {
                InputSourceFlag &= ~InputSourceFlags.KEYBOARD;
            }

            InputSourceFlag |= InputSourceFlags.JOYSTICK;
        }
        if (isKeyPressed)
        {
            if ((InputSourceFlag & InputSourceFlags.JOYSTICK) == InputSourceFlags.JOYSTICK)
            {
                InputSourceFlag &= ~InputSourceFlags.JOYSTICK;
            }

            InputSourceFlag |= InputSourceFlags.KEYBOARD;
        }
    }

    private int GetInputMultiplier(Vector2 joystickInput)
    {
        float inputX = joystickInput.x;
        return inputX == 0 ? 0 : (inputX < 0.0f ? -1 : 1);
    }

    protected virtual void ComputeVelocity(
        ref bool isKeyPressed,
        Vector2 josytickInput
    )
    {
        int keyMultiplier = 0;

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            keyMultiplier   = -1;
            isKeyPressed    = true;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            keyMultiplier   = 1;
            isKeyPressed    = true;
        }

        VelocityX = (isJumping ? PreviousVelocityX : (isKeyPressed ? keyMultiplier : GetInputMultiplier(josytickInput)) * PixelVelocityX) * Time.deltaTime;

        float durationToPeak    = 2304.0f / PixelVelocityX;
        float initialVelocityY  = 48.0f * 2.0f / durationToPeak;
        float gravity           = -256.0f * 2.0f / (durationToPeak * durationToPeak);

        VelocityY = isJumping ? gravity * JumpStateTimer + initialVelocityY : PixelVelocityY * Time.deltaTime;

        if (VelocityX < 0.0f)
        {
            PreviousVelocityX = -PixelVelocityX;
        }
        else if (VelocityX > 0.0f)
        {
            PreviousVelocityX = PixelVelocityX;
        }
        else
        {
            PreviousVelocityX = 0.0f;
        }
    }

    protected void Start()
    {
        PixelTileComponent                      = GetComponent<PixelTile>();
        PixelLevel currentPixelLevelInstance    = PixelTileComponent.CurrentPixelLevelInstance;

        if (!currentPixelLevelInstance)
        {
            return;
        }

        CollisionNormalStateMap   = new Dictionary<int, CollisionNormalFlags>();
        CurrentPixelLevelInstance = currentPixelLevelInstance;
        PixelTileComponent.AlignToPixelLevelGrid();

        int pixelsPerUnit   = currentPixelLevelInstance.PixelsPerUnit;
        PixelPositionX      = currentPixelLevelInstance.PixelOriginX + PixelTileComponent.AlignedRelativePositionX * pixelsPerUnit;
        PixelPositionY      = currentPixelLevelInstance.PixelOriginY + PixelTileComponent.AlignedRelativePositionY * pixelsPerUnit;
        PreviousVelocityX   = PixelVelocityX;
        CollisionNormalFlag = CollisionNormalFlags.NONE;
        InputSourceFlag     = InputSourceFlags.NONE;
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
                OnObstacleExited(obstaclePixelTileComponent.GetInstanceID());
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
                OnObstacleCollided(obstaclePixelTileComponent.GetInstanceID(), CollisionNormalFlags.RIGHT_WALL);
                PixelPositionX -= differences[0];
            }
            if (differenceMinimum == differences[1])
            {
                OnObstacleCollided(obstaclePixelTileComponent.GetInstanceID(), CollisionNormalFlags.LEFT_WALL);
                PixelPositionX += differences[1];
            }
            if (differenceMinimum == differences[2])
            {
                OnObstacleCollided(obstaclePixelTileComponent.GetInstanceID(), CollisionNormalFlags.CEILING);
                PixelPositionY -= differences[2];
            }
            if (differenceMinimum == differences[3])
            {
                isJumping = false;
                OnObstacleCollided(obstaclePixelTileComponent.GetInstanceID(), CollisionNormalFlags.GROUND);
                PixelPositionY += differences[3];
            }
        }
    }

    protected virtual void OnObstacleCollided(
        int objectId,
        CollisionNormalFlags normalFlag
    )
    {
        if (!CollisionNormalStateMap.ContainsKey(objectId))
        {
            CollisionNormalFlag |= normalFlag;
            CollisionNormalStateMap.Add(objectId, normalFlag);
        }
    }

    protected virtual void OnObstacleExited(int objectId)
    {
        if (CollisionNormalStateMap.ContainsKey(objectId))
        {
            CollisionNormalFlag &= ~CollisionNormalStateMap[objectId];
            CollisionNormalStateMap.Remove(objectId);
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

        if (Input.GetButtonDown("Jump") && ((CollisionNormalFlag & CollisionNormalFlags.GROUND) == CollisionNormalFlags.GROUND))
        {
            isJumping       = true;
            JumpStateTimer  = 0.0f;
        }

        Vector2 joystickInput   = Joystick.GetSmoothInput(0.125f, "Horizontal", "Vertical");
        bool isKeyPressed       = false;

        ComputeVelocity(ref isKeyPressed, joystickInput);
        SetInputSourceFlag(isKeyPressed, joystickInput);

        Debug.Log(InputSourceFlag);

        PixelPositionX += Mathf.RoundToInt(VelocityX);
        PixelPositionY += Mathf.RoundToInt(VelocityY);

        InitializePixelTileObstacleList();
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