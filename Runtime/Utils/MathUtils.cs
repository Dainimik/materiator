using UnityEngine;

namespace Materiator
{
    public static class MathUtils
    {

        public static Vector2 Scale2D(Vector2 vector, Vector2 scale, Vector2 pivot)
        {
            return new Vector2(pivot.x + scale.x * (vector.x - pivot.x), pivot.y + scale.y * (vector.y - pivot.y));
        }
    }
}