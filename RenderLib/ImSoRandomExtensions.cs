using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace raytracinginoneweekend
{
    public static class ImSoRandomExtensions
    {
        public static Vector3 RandomInUnitSphere(this ImSoRandom random)
        {
            var p = new Vector3();
            do
            {
                p = 2.0f * new Vector3(random.NextFloat(), random.NextFloat(), random.NextFloat()) - new Vector3(1, 1, 1);
            } while (p.LengthSquared() >= 1.0f);
            return p;
        }

        public static Vector3 RandomInUnitDisk(this ImSoRandom random)
        {
            var p = new Vector3();
            do
            {
                p = 2.0f * new Vector3(random.NextFloat(), random.NextFloat(), 0) - new Vector3(1, 1, 0);
            } while (Vector3.Dot(p, p) >= 1.0f);
            return p;
        }

        public static Vector3 RandomVector(this ImSoRandom random)
        {
            return new Vector3(10 + random.NextFloat() * 400f, 10 + random.NextFloat() * 400f, 10 + random.NextFloat() * 400f);
        }
    }
}
