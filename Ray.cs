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
            Origin = origin; Direction = direction; Time = time;
        }
        public Vector3 Origin;
        public Vector3 Direction;
        public float Time;
        public Vector3 PointAtParameter(float t) { return Origin + t * Direction; }
    }
}
