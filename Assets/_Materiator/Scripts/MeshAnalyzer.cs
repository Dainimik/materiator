using System.Collections.Generic;
using UnityEngine;

namespace Materiator
{
    public class MeshAnalyzer : MonoBehaviour
    {
        public int GridSize = 4;

        private Mesh _mesh;
        public Rect[] UVRects;
        public GenericDictionary<int, Vector2> FilteredUVs;
        public Vector2[] UVs;

        private void Awake()
        {
            _mesh = GetComponent<MeshFilter>().sharedMesh;
            UVs = _mesh.uv;
            UVRects = CalculateRects(GridSize);
            FilteredUVs = FilterUVs(UVRects, _mesh.uv);
            
        }

        private GenericDictionary<int, Vector2> FilterUVs(Rect[] rects, Vector2[] uvs)
        {
            var filteredUVs = new GenericDictionary<int, Vector2>();

            for (int i = 0; i < uvs.Length; i++)
            {
                for (int r = 0; r < rects.Length; r++)
                {
                    if (rects[r].Contains(uvs[i]))
                    {
                        if (!filteredUVs.ContainsKey(r))
                        {
                            filteredUVs.Add(r, uvs[i]);
                        } 
                    }
                }
            }

            return filteredUVs;
        }

        public static Rect[] CalculateRects(int size)
        {
            var rects = new Rect[size * size];
            var rectSize = 1 / (float)size;

            for (int i = 0, y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++, i++)
                {
                    if (i >= size * size) break;
                    rects[i] = new Rect(x / (float)size, y / (float)size, rectSize, rectSize);
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