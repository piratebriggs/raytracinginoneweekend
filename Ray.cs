using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace raytracinginoneweekend
{
    public class Ray
    {
        public Ray(Vector3 origin, Vector3 direction, float time)
        {
            A = origin; B = direction; Time = time;
        }
        public Vector3 A { get; set; }
        public Vector3 B { get; set; }
        public Vector3 Origin { get { return A; } }
        public Vector3 Direction { get { return B; } }
        public float Time;
        public Vector3 PointAtParameter(float t) { return A + t * B; }
    }
}
