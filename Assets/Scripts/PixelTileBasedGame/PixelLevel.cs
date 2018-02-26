using UnityEngine;

[ExecuteInEditMode]
public class PixelLevel : MonoBehaviour
{
    [HideInInspector]
    public LevelGrid    LevelGridComponent      { get; private set; }

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

    private void InitializeLevelGridComponent()
    {
        LevelGridComponent = GetComponent<LevelGrid>();

        if (!LevelGridComponent)
        {
            LevelGridComponent = gameObject.AddComponent<LevelGrid>();
        }

        LevelGridComponent.Origin               = new Vector2(PixelOriginX / PixelsPerUnit, PixelOriginY / PixelsPerUnit);
        LevelGridComponent.HorizontalCellCount  = PixelWidth / PixelsPerUnit;
        LevelGridComponent.VerticalCellCount    = PixelHeight / PixelsPerUnit;
        LevelGridComponent.CellSize             = new Vector2(1.0f, 1.0f);
    }

    private void Awake()
    {
        if (Application.isPlaying)
        {
            ValidateState();
            InitializeLevelGridComponent();
            PixelTileManager.SnapAllTilesToGrid();
        }
    }

    private void Update ()
    {
        if (!Application.isPlaying)
        {
            ValidateState();
            InitializeLevelGridComponent();
            LevelGridComponent.Awake();
        }

        LevelGridComponent.Update();
    }
};