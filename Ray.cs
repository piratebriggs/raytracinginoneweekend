using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace raytracinginoneweekend
{
    public class Ray
    {
        public Ray(Vector4 origin, Vector4 direction)
        {
            A = origin; B = direction;
        }
        public Vector4 A { get; set; }
        public Vector4 B { get; set; }
        public Vector4 Origin { get { return A; } }
        public Vector4 Direction { get { return B; } }
        public Vector4 PointAtParameter(float t) { return A + t * B; }
    }
}
