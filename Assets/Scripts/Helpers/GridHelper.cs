using Unity.Mathematics;

public class GridHelper
{
    public static int4 CalculateGridOccupationFromBounds(AABB bounds)
    {
        int2 x = new int2((int)math.round(bounds.Center.x - bounds.Extents.x), (int)math.round(bounds.Center.x + bounds.Extents.x));
        int2 y = new int2((int)math.round(bounds.Center.z - bounds.Extents.z), (int)math.round(bounds.Center.z + bounds.Extents.z));
        return new int4(x.x, y.x, x.y, y.y);
    }

}
