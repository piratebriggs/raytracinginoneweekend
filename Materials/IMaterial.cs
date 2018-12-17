using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace raytracinginoneweekend.Materials
{
    public interface IMaterial
    {
        bool Scatter(Ray rayIn, HitRecord hitRecord, out Vector3 attenuation, out Ray scattererd);
    }
}
