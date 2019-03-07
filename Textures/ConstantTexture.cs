using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace raytracinginoneweekend.Textures
{
    public class ConstantTexture : Texture
    {
        Vector3 colour;

        public ConstantTexture(float r, float g, float b) : this(new Vector3(r, g, b)) { }

        public ConstantTexture(Vector3 colour)
        {
            this.colour = colour;
        }

        public override Vector3 value(float u, float v, ref Vector3 p)
        {
            return colour;
        }
    }
}
