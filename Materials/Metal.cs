using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace raytracinginoneweekend.Materials
{
    public class Metal : IMaterial
    {
        private Vector4 _albedo;
        private float _fuzz;

        public Metal(Vector4 a, float f)
        {
            _albedo = a;
            _fuzz = (f < 1) ? f : 1;
        }

        public bool Scatter(Ray rayIn, HitRecord rec, out Vector4 attenuation, out Ray scattererd, ImSoRandom rnd)
        {
            Vector4 reflected = Vector4.Normalize(rayIn.Direction).Reflect( rec.Normal);
            scattererd = new Ray(rec.P, reflected + _fuzz * rnd.RandomInUnitSphere());
            attenuation = _albedo;
            return (Vector4.Dot(scattererd.Direction,rec.Normal) > 0);

        }
    }
}
