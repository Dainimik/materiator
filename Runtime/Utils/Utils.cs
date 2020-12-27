using UnityEngine;

namespace Materiator
{
    public static class Utils
    {
        /// <summary>
        /// Returns absolute rect values (in pixels)
        /// </summary>
        /// <param name="gridSize"></param>
        /// <param name="rect">Rect with relative values (in percent).</param>
        /// <returns></returns>
        public static RectInt GetRectIntFromRect(Vector2Int gridSize, Rect rect)
        {
            return new RectInt((int)(gridSize.x * rect.x), (int)(gridSize.y * rect.y), (int)(gridSize.x * rect.width), (int)(gridSize.y * rect.height));
        }

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