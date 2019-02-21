using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace raytracinginoneweekend
{
    public class Camera
    {
        public Camera(Vector3 lookfrom, Vector3 lookat, Vector3 vup,  float vfov, float aspect, float aperture, float focusDist, float t0, float t1)
        {
            Time0 = t0;
            Time1 = t1;

            LensRadius = aperture / 2f;
            var theta = vfov * Math.PI / 180;
            var halfHeight = (float)Math.Tan(theta / 2);
            var halfWidth = aspect * halfHeight;
            Origin = lookfrom;
            W = Vector3.Normalize(lookfrom - lookat);
            U = Vector3.Normalize(Vector3.Cross(vup, W));
            V = Vector3.Cross(W, U);
            LowerLeftCorner = Origin - halfWidth * focusDist * U - halfHeight * focusDist * V - focusDist * W;
            Horizontal = 2 * halfWidth * focusDist * U;
            Vertical = 2 * halfHeight * focusDist * V;
        }

        public Ray GetRay(float u, float v, ImSoRandom rnd)
        {
            var rd = LensRadius * rnd.RandomInUnitDisk();
            var offset = U * rd.X + V * rd.Y;
            float time = Time0 + rnd.NextFloat() * (Time1 - Time0);
            return new Ray(Origin+offset, LowerLeftCorner + u * Horizontal + v * Vertical - Origin - offset, time);
        }

        Vector3 Origin;
        Vector3 LowerLeftCorner;
        Vector3 Horizontal;
        Vector3 Vertical;
        Vector3 U;
        Vector3 V;
        Vector3 W;
        float Time0, Time1;
        float LensRadius;
    }
}
