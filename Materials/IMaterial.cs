using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace raytracinginoneweekend.Materials
{
    public abstract class Material
    {
        public abstract bool Scatter(Ray rayIn, HitRecord hitRecord, out Vector3 attenuation, out Ray scattererd, ImSoRandom rnd);
        public virtual Vector3 Emitted(float u, float v, ref Vector3 p)
        {
            return new Vector3(0, 0, 0);
        }
    }

}
