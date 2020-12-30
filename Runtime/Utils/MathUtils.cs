using UnityEngine;

namespace Materiator
{
    public static class MathUtils
    {
        public static Vector2 Transform2DCoord(Vector2 coord, Rect destRect)
        {
            coord.x = destRect.x + (coord.x * destRect.width);
            coord.y = destRect.y + (coord.y * destRect.height);

            return coord;
        }

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
    }
}