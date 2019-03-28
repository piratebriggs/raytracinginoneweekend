using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using raytracinginoneweekend.Materials;
using SimpleScene;

namespace raytracinginoneweekend.Hitables
{
    public class RectXZ : IHitable
    {
        float x0, x1, z0, z1, k;
        Material mat;
        SSAABB box;

        public RectXZ(float x0, float x1, float z0, float z1, float k, Material mat)
        {
            this.x0 = x0;
            this.x1 = x1;
            this.z0 = z0;
            this.z1 = z1;
            this.k = k;
            this.mat = mat;

            box.Min = new Vector3(x0, k - 0.0001f, z0);
            box.Max = new Vector3(x1, k + 0.0001f, z1);
        }

        public SSAABB BoundingBox => box;

        public bool Hit(Ray r, float tMin, float tMax, ref HitRecord rec)
        {
            float t = (k - r.Origin.Y) / r.Direction.Y;
            if (t < tMin || t > tMax)
                return false;
            float x = r.Origin.X + t * r.Direction.X;
            float z = r.Origin.Z + t * r.Direction.Z;
            if (x < x0 || x > x1 || z < z0 || z > z1)
                return false;
            rec.U = (x - x0) / (x1 - x0);
            rec.V = (z - z0) / (z1 - z0);
            rec.T = t;
            rec.Material = mat;
            rec.P = r.PointAtParameter(t);
            rec.Normal = new Vector3(0, 1, 0);
            return true;

        }
    }
}
