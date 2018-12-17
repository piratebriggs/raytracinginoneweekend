using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace raytracinginoneweekend.Materials
{
    public class Metal : IMaterial
    {
        private Vector3 _albedo;
        private float _fuzz;

        public Metal(Vector3 a, float f)
        {
            _albedo = a;
            _fuzz = (f < 1) ? f : 1;
        }

        public bool Scatter(Ray rayIn, HitRecord rec, out Vector3 attenuation, out Ray scattererd)
        {
            Vector3 reflected = Reflect(Vector3.Normalize(rayIn.Direction), rec.Normal);
            scattererd = new Ray(rec.P, reflected + _fuzz * Program.RandomInUnitSphere());
            attenuation = _albedo;
            return (Vector3.Dot(scattererd.Direction,rec.Normal) > 0);

        }

        Vector3 Reflect(Vector3 v, Vector3 n)
        {
            return v - 2 * Vector3.Dot(v, n) * n;
        }
    }
}
