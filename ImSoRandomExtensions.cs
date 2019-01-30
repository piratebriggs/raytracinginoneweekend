using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace raytracinginoneweekend
{
    public static class ImSoRandomExtensions
    {
        public static Vector4 RandomInUnitSphere(this ImSoRandom random)
        {
            var p = VectorExtensions.Vec(0,0,0);
            do
            {
                p = 2.0f * VectorExtensions.Vec(random.NextFloat(), random.NextFloat(), random.NextFloat()) - VectorExtensions.Vec(1, 1, 1);
            } while (p.LengthSquared() >= 1.0f);
            return p;
        }

        public static Vector4 RandomInUnitDisk(this ImSoRandom random)
        {
            var p = VectorExtensions.Vec(0,0,0);
            do
            {
                p = 2.0f * VectorExtensions.Vec(random.NextFloat(), random.NextFloat(), 0) - VectorExtensions.Vec(1, 1, 0);
            } while (Vector4.Dot(p, p) >= 1.0f);
            return p;
        }

    }
}
