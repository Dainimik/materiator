using UnityEngine;

namespace Materiator
{
    public static class Utils
    {
        public static Color32[] ColorToColor32Array(Color[] colors)
        {
            Color32[] color32 = new Color32[colors.Length];

            for (int i = 0; i < colors.Length; i++)
            {
                color32[i] = new Color32((byte)(colors[i].r * 255), (byte)(colors[i].g * 255), (byte)(colors[i].b * 255), (byte)(colors[i].a * 255));
            }

            return color32;
        }
    }
}