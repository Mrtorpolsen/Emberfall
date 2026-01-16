using System;
using UnityEngine;

public class CinderRewardCalculator
{
    private static readonly int[] cindersPerMinute = new int[]
    {
        20, 33, 49, 66, 84, 103, 123, 144, 165, 187,
        210, 233, 256, 280, 305, 330, 355, 380, 406, 432
    };

    public static int GetCinders(int minutes)
    {
        if (minutes < 1) return 0;

        if (minutes > 20)
        {
            return cindersPerMinute[19] + ((minutes - 20) * 20);
        }

        return cindersPerMinute[minutes - 1];
    }
}
