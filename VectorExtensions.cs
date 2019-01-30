using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace raytracinginoneweekend
{
    public static class VectorExtensions
    {
        public static Vector4 Vec(float x, float y, float z)
        {
            return new Vector4(x, y, z, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Cross(this Vector4 vector1, Vector4 vector2)
        {
            return new Vector4(
                vector1.Y * vector2.Z - vector1.Z * vector2.Y,
                vector1.Z * vector2.X - vector1.X * vector2.Z,
                vector1.X * vector2.Y - vector1.Y * vector2.X,
                0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Reflect(this Vector4 vector, Vector4 normal)
        {
            float dot = vector.X * normal.X + vector.Y * normal.Y + vector.Z * normal.Z;
            float tempX = normal.X * dot * 2f;
            float tempY = normal.Y * dot * 2f;
            float tempZ = normal.Z * dot * 2f;
            return new Vector4(vector.X - tempX, vector.Y - tempY, vector.Z - tempZ, 0);
        }

    }
}
