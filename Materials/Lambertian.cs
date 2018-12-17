using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace raytracinginoneweekend.Materials
{
    public class Lambertian : IMaterial
    {
        private Vector3 _albedo;

        public Lambertian(Vector3 a)
        {
            _albedo = a;
        }

        public bool Scatter(Ray rayIn, HitRecord rec, out Vector3 attenuation, out Ray scattererd)
        {
            Vector3 target = rec.P + rec.Normal + RandomInUnitSphere();
            scattererd = new Ray(rec.P, target - rec.P);
            attenuation = _albedo;
            return true;

        }

        static Vector3 RandomInUnitSphere()
        {
            var p = new Vector3();
            do
            {
                p = 2.0f * new Vector3(Program.Rnd.NextFloat(), Program.Rnd.NextFloat(), Program.Rnd.NextFloat()) - new Vector3(1, 1, 1);
            } while (p.LengthSquared() >= 1.0f);
            return p;
        }

    }
}
