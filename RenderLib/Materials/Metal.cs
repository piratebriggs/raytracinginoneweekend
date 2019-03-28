using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace raytracinginoneweekend.Materials
{
    public class Metal : Material
    {
        private Vector3 _albedo;
        private float _fuzz;

        public Metal(Vector3 a, float f)
        {
            _albedo = a;
            _fuzz = (f < 1) ? f : 1;
        }

        public override bool Scatter(Ray rayIn, HitRecord rec, out Vector3 attenuation, out Ray scattererd, ImSoRandom rnd)
        {
            Vector3 reflected = Vector3.Reflect(Vector3.Normalize(rayIn.Direction), rec.Normal);
            scattererd = new Ray(rec.P, reflected + _fuzz * rnd.RandomInUnitSphere(), rayIn.Time);
            attenuation = _albedo;
            return (Vector3.Dot(scattererd.Direction,rec.Normal) > 0);

        }
    }
}
