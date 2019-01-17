using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace raytracinginoneweekend
{
    public class Camera
    {
        public Camera(Vector3 lookfrom, Vector3 lookat, Vector3 vup,  float vfov, float aspect)
        {
            var theta = vfov * Math.PI / 180;
            var halfHeight = (float)Math.Tan(theta / 2);
            var halfWidth = aspect * halfHeight;
            Origin = lookfrom;
            var w = Vector3.Normalize(lookfrom - lookat);
            var u = Vector3.Normalize(Vector3.Cross(vup, w));
            var v = Vector3.Cross(w, u);
            LowerLeftCorner = Origin - halfWidth * u - halfHeight * v - w;
            Horizontal = 2 * halfWidth * u;
            Vertical = 2 * halfHeight * v;
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
