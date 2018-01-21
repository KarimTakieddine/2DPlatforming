using UnityEngine;

[ExecuteInEditMode]
public class PixelLevel : MonoBehaviour
{
    [HideInInspector]
    public LevelGrid            LevelGridComponent      { get; private set; }
    public uint                 InternalPixelOriginX    { get; private set; }
    public uint                 InternalPixelOriginY    { get; private set; }
    public uint                 InternalPixelWidth      { get; private set; }
    public uint                 InternalPixelHeight     { get; private set; }

    public uint                 PixelsPerUnit;
    public uint                 PixelOriginX, PixelOriginY;
    
    public uint                 PixelWidth, PixelHeight;

	private void Update ()
    {
        LevelGridComponent = GetComponent<LevelGrid>();

        if (!LevelGridComponent)
        {
            LevelGridComponent = gameObject.AddComponent<LevelGrid>();
        }

        if (PixelsPerUnit == 0)
        {
            PixelsPerUnit = 1;
        }
        
        InternalPixelOriginX                    = Mathematics.FindClosestMultipleOf(PixelOriginX, PixelsPerUnit);
        InternalPixelOriginY                    = Mathematics.FindClosestMultipleOf(PixelOriginY, PixelsPerUnit);
        InternalPixelWidth                      = Mathematics.FindClosestMultipleOf(PixelWidth, PixelsPerUnit);
        InternalPixelHeight                     = Mathematics.FindClosestMultipleOf(PixelHeight, PixelsPerUnit);
        LevelGridComponent.Origin               = new Vector2(InternalPixelOriginX / PixelsPerUnit, InternalPixelOriginY / PixelsPerUnit);
        LevelGridComponent.HorizontalCellCount  = (int)(InternalPixelWidth / PixelsPerUnit);
        LevelGridComponent.VerticalCellCount    = (int)(InternalPixelHeight / PixelsPerUnit);
        LevelGridComponent.CellSize             = new Vector2(1.0f, 1.0f);
        LevelGridComponent.Awake();
        LevelGridComponent.Update();
	}
};