using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(LevelGrid))]
public class PixelLevel : MonoBehaviour
{
    public static PixelLevel    GlobalInstance      { get; private set; }
    public LevelGrid            LevelGridComponent  { get; private set; }
    public static uint          GlobalPixelsPerUnit { get; private set; }

    public uint                 PixelsPerUnit;
    public uint                 OriginX, OriginY;
    public uint                 PixelWidth, PixelHeight;

    public static uint FindClosestMultipleOf(
        uint number,
        uint multiple
    )
    {
        for (uint i = number; i > 0; --i)
        {
            if ((i % multiple) == 0)
            {
                return i;
            }
        }

        return 0;
    }

    public static void InitializeGlobalInstance()
    {
        if (!GlobalInstance)
        {
            PixelLevel[] AllPixelLevels = FindObjectsOfType<PixelLevel>();

            if (AllPixelLevels.Length > 1)
            {
                throw new UnityException("Cannot create more than one static PixelLevel!");
            }

            GlobalInstance = AllPixelLevels[0];
        }
    }

	void Awake ()
    {
        InitializeGlobalInstance();
        GlobalPixelsPerUnit             = PixelsPerUnit;
        LevelGridComponent              = GetComponent<LevelGrid>();
        LevelGridComponent.CellSizeX    = 1;
        LevelGridComponent.CellSizeY    = 1;
        LevelGridComponent.InitializeComponents();
    }

	void Update ()
    {
        LevelGridComponent.OriginX              = OriginX;
        LevelGridComponent.OriginY              = OriginY;
        LevelGridComponent.HorizontalCellCount  = FindClosestMultipleOf(PixelWidth, PixelsPerUnit) / PixelsPerUnit;
        LevelGridComponent.VerticalCellCount    = FindClosestMultipleOf(PixelHeight, PixelsPerUnit) / PixelsPerUnit;
        LevelGridComponent.UpdateComponents();
	}
};