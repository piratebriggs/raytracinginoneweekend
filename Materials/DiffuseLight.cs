using raytracinginoneweekend.Textures;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace raytracinginoneweekend.Materials
{
    public class DiffuseLight : Material
    {
        Texture emit;

        public DiffuseLight(Texture emit)
        {
            this.emit = emit;
        }

        public override bool Scatter(Ray rayIn, HitRecord hitRecord, out Vector3 attenuation, out Ray scattererd, ImSoRandom rnd)
        {
            attenuation = Vector3.Zero;
            scattererd = new Ray(Vector3.Zero, Vector3.Zero, 0);
            return false;
        }
        public override Vector3 Emitted(float u, float v, ref Vector3 p)
        {
            return emit.value(u, v, ref p);
        }
    }
}
