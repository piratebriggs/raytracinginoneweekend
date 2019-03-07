using raytracinginoneweekend.Textures;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace raytracinginoneweekend.Materials
{
    public class Lambertian : Material
    {
        private Texture _albedo;

        public Lambertian(Texture a)
        {
            _albedo = a;
        }

        public override bool Scatter(Ray rayIn, HitRecord rec, out Vector3 attenuation, out Ray scattererd, ImSoRandom rnd)
        {
            Vector3 target = rec.P + rec.Normal + rnd.RandomInUnitSphere();
            scattererd = new Ray(rec.P, target - rec.P, rayIn.Time);
            attenuation = _albedo.value(rec.U, rec.V, ref rec.P);
            return true;

        }
    }
}
