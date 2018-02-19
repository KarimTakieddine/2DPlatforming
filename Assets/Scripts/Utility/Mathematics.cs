using UnityEngine;

public static class Mathematics
{
    public static int FindClosestMultipleOf(
        int number,
        int multiple
    )
    {
        for (int i = number; i > 0; --i)
        {
            if ((i % multiple) == 0)
            {
                return i;
            }
        }

        return 0;
    }
};