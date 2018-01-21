using UnityEngine;

[ExecuteInEditMode]
public class PixelLevel : MonoBehaviour
{
    [HideInInspector]
    public LevelGrid            LevelGridComponent      { get; private set; }
    public uint                 AlignedPixelOriginX     { get; private set; }
    public uint                 AlignedPixelOriginY     { get; private set; }
    public uint                 AlignedPixelWidth       { get; private set; }
    public uint                 AlignedPixelHeight      { get; private set; }

    public uint                 PixelsPerUnit;
    public uint                 PixelOriginX, PixelOriginY;
    
    public uint                 PixelWidth, PixelHeight;

    private void AlignState()
    {
        if (PixelsPerUnit == 0)
        {
            PixelsPerUnit = 1;
        }

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
        LevelGridComponent.HorizontalCellCount  = (int)(AlignedPixelWidth / PixelsPerUnit);
        LevelGridComponent.VerticalCellCount    = (int)(AlignedPixelHeight / PixelsPerUnit);
        LevelGridComponent.CellSize             = new Vector2(1.0f, 1.0f);
    }

    private void Awake()
    {
        if (Application.isPlaying)
        {
            AlignState();
            InitializeLevelGridComponent();
        }
    }

    private void Update ()
    {
        if (!Application.isPlaying)
        {
            AlignState();
            InitializeLevelGridComponent();
            LevelGridComponent.Awake();
        }

        LevelGridComponent.Update();
    }
};