public class Solution {
    public int MinEatingSpeed(int[] piles, int h) {
    int left = 1;
    int right = 0;

    foreach (var pile in piles)
    {
        if (pile > right)
            right = pile;
    }

    long totalHours = 0;
    int res = 0;

    while (left <= right)
    {
        int mid = left + (right - left) / 2;

        totalHours = 0;

        for (int i = 0; i < piles.Length; i++)
        {
            totalHours += ((long)piles[i] + mid - 1) / mid;
        }

        if (totalHours <= h)
        {
            res = mid;
            right = mid - 1;
        }
        else
        {
            left = mid + 1;
        }
    }

    return res;
    }
}