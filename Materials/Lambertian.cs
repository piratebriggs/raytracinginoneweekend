using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace raytracinginoneweekend.Materials
{
    public class Lambertian : IMaterial
    {
        private Vector4 _albedo;

        public Lambertian(Vector4 a)
        {
            _albedo = a;
        }

        public bool Scatter(Ray rayIn, HitRecord rec, out Vector4 attenuation, out Ray scattererd, ImSoRandom rnd)
        {
            Vector4 target = rec.P + rec.Normal + rnd.RandomInUnitSphere();
            scattererd = new Ray(rec.P, target - rec.P);
            attenuation = _albedo;
            return true;

        }
    }
}
