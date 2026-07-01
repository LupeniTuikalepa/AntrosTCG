using UnityEngine;

namespace ATCG.Utilities
{
    public static class GameMaths
    {
        public static int Round(float value) => Mathf.CeilToInt(value);
    }
}