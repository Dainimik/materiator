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

        public static Rect[] CalculateRects(int size, Rect offset)
        {
            var rects = new Rect[size * size];
            var rectSize = 1 / (float)size;

            for (int i = 0, y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++, i++)
                {
                    if (i >= size * size) break;
                    rects[i] = new Rect(offset.x + (x / (float)size), offset.y + (y / (float)size), rectSize, rectSize);
                    rects[i].xMin = rects[i].x;
                    rects[i].yMin = rects[i].y;
                    rects[i].xMax = rects[i].x + rectSize;
                    rects[i].yMax = rects[i].y + rectSize;
                }
            }

            return rects;
        }
    }
}