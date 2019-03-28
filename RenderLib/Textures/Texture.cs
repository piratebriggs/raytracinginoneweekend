using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace raytracinginoneweekend.Textures
{
    public abstract class Texture
    {
        public abstract Vector3 value(float u, float v, ref Vector3 p);
    }
}
