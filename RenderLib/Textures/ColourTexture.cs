using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace raytracinginoneweekend.Textures
{
    public class ColourTexture : Texture
    {

        public override Vector3 value(float u, float v, ref Vector3 p)
        {
            return new Vector3(u, v, 1 - u - v);
        }
    }
}
