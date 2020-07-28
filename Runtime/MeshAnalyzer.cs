using UnityEngine;

namespace Materiator
{
    public static class MeshAnalyzer
    {
        public static SerializableDictionary<int, Rect> FilterRects(Rect[] rects, Vector2[] uvs)
        {
            var filteredRects = new SerializableDictionary<int, Rect>();

            for (int i = 0; i < uvs.Length; i++)
            {
                for (int r = 0; r < rects.Length; r++)
                {
                    if (rects[r].Contains(uvs[i]))
                    {
                        if (!filteredRects.ContainsKey(r))
                        {
                            filteredRects.Add(r, rects[r]);
                        } 
                    }
                }
            }

            return filteredRects;
        }
        /// <summary>
        /// Returns Rects with percent-based position and size values that are relative to the offset Rect.
        /// </summary>
        /// <param name="size"> Size in pixels</param>
        /// <param name="offset"> Relative rect to calculate rects against</param>
        /// <returns></returns>
        public static Rect[] CalculateRects(Vector2Int size, Rect offset)
        {
            var rects = new Rect[size.x * size.y];
            var rectSize = new Vector2();
            rectSize.x = 1 / (float)size.x * offset.width;
            rectSize.y = 1 / (float)size.y * offset.height;

            for (int i = 0, y = 0; y < size.y; y++)
            {
                for (int x = 0; x < size.x; x++, i++)
                {
                    if (i >= size.x * size.y) break;
                    rects[i] = new Rect(offset.x + (x / (float)size.x * offset.width), offset.y + (y / (float)size.y * offset.height), rectSize.x, rectSize.y);
                    rects[i].xMin = rects[i].x;
                    rects[i].yMin = rects[i].y;
                    rects[i].xMax = rects[i].x + rectSize.x;
                    rects[i].yMax = rects[i].y + rectSize.y;
                }
            }

            return rects;
        }
    }
}