using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace raytracinginoneweekend.Textures
{
    public class CheckerTexture : Texture
    {
        Texture odd;
        Texture even;

        public CheckerTexture(Texture t0, Texture t1)
        {
            this.odd = t0;
            this.even = t1;
        }

        public override Vector3 value(float u, float v, ref Vector3 p)
        {
            float sines = (float)(Math.Sin(10 * p.X) * Math.Sin(10 * p.Y) * Math.Sin(10 * p.Z));
            if(sines<0)
            {
                return odd.value(u, v, ref p);
            } else
            {
                return even.value(u, v, ref p);
            }
        }
    }
}
