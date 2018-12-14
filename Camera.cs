using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace raytracinginoneweekend
{
    public class Camera
    {
        public Camera()
        {
            Origin = new Vector3(0.0f);
            LowerLeftCorner = new Vector3(-2.0f, -1.0f, -1.0f);
            Horizontal = new Vector3(4, 0, 0);
            Vertical = new Vector3(0, 2, 0);
        }

        public Ray GetRay(float u, float v)
        {
            return new Ray(Origin, LowerLeftCorner + u * Horizontal + v * Vertical - Origin);
        }

        Vector3 Origin { get; set; }
        Vector3 LowerLeftCorner { get; set; }
        Vector3 Horizontal { get; set; }
        Vector3 Vertical { get; set; }
    }
}
