using UnityEngine;

public static class Mathematics
{
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
};