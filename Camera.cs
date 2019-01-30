using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace raytracinginoneweekend
{
    public class Camera
    {
        public Camera(Vector4 lookfrom, Vector4 lookat, Vector4 vup,  float vfov, float aspect, float aperture, float focusDist)
        {
            LensRadius = aperture / 2f;
            var theta = vfov * Math.PI / 180;
            var halfHeight = (float)Math.Tan(theta / 2);
            var halfWidth = aspect * halfHeight;
            Origin = lookfrom;
            W = Vector4.Normalize(lookfrom - lookat);
            U = Vector4.Normalize(vup.Cross(W));
            V = W.Cross(U);
            LowerLeftCorner = Origin - halfWidth * focusDist * U - halfHeight * focusDist * V - focusDist * W;
            Horizontal = 2 * halfWidth * focusDist * U;
            Vertical = 2 * halfHeight * focusDist * V;
        }

        public Ray GetRay(float u, float v, ImSoRandom rnd)
        {
            var rd = LensRadius * rnd.RandomInUnitDisk();
            var offset = U * rd.X + V * rd.Y;
            return new Ray(Origin+offset, LowerLeftCorner + u * Horizontal + v * Vertical - Origin - offset);
        }

        Vector4 Origin { get; set; }
        Vector4 LowerLeftCorner { get; set; }
        Vector4 Horizontal { get; set; }
        Vector4 Vertical { get; set; }
        Vector4 U { get; set; }
        Vector4 V { get; set; }
        Vector4 W { get; set; }
        float LensRadius { get; set; }
    }
}
