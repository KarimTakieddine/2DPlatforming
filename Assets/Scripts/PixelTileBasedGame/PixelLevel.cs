using UnityEngine;

[ExecuteInEditMode]
public class PixelLevel : MonoBehaviour
{
    [HideInInspector]
    public LevelGrid    LevelGridComponent      { get; private set; }
    public int          AlignedPixelOriginX     { get; private set; }
    public int          AlignedPixelOriginY     { get; private set; }
    public int          AlignedPixelWidth       { get; private set; }
    public int          AlignedPixelHeight      { get; private set; }

    public int          PixelsPerUnit;
    public int          PixelOriginX, PixelOriginY;
    
    public int          PixelWidth, PixelHeight;

    private void ValidateState()
    {
        if (PixelsPerUnit <= 0)
        {
            PixelsPerUnit = 1;
        }

        if (PixelHeight < 0)
        {
            PixelHeight = 0;
        }

        if (PixelWidth < 0)
        {
            PixelWidth = 0;
        }
    }

    private void AlignState()
    {
        AlignedPixelOriginX     = Mathematics.FindClosestMultipleOf(PixelOriginX, PixelsPerUnit);
        AlignedPixelOriginY     = Mathematics.FindClosestMultipleOf(PixelOriginY, PixelsPerUnit);
        AlignedPixelWidth       = Mathematics.FindClosestMultipleOf(PixelWidth, PixelsPerUnit);
        AlignedPixelHeight      = Mathematics.FindClosestMultipleOf(PixelHeight, PixelsPerUnit);
    }

    private void InitializeLevelGridComponent()
    {
        LevelGridComponent = GetComponent<LevelGrid>();

        if (!LevelGridComponent)
        {
            LevelGridComponent = gameObject.AddComponent<LevelGrid>();
        }

        LevelGridComponent.Origin               = new Vector2(AlignedPixelOriginX / PixelsPerUnit, AlignedPixelOriginY / PixelsPerUnit);
        LevelGridComponent.HorizontalCellCount  = AlignedPixelWidth / PixelsPerUnit;
        LevelGridComponent.VerticalCellCount    = AlignedPixelHeight / PixelsPerUnit;
        LevelGridComponent.CellSize             = new Vector2(1.0f, 1.0f);
    }

    private void Awake()
    {
        if (Application.isPlaying)
        {
            ValidateState();
            AlignState();
            InitializeLevelGridComponent();
            PixelTileManager.SnapAllTilesToGrid();
        }
    }

    private void Update ()
    {
        if (!Application.isPlaying)
        {
            ValidateState();
            AlignState();
            InitializeLevelGridComponent();
            LevelGridComponent.Awake();
        }

        LevelGridComponent.Update();
    }
};