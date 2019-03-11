using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using raytracinginoneweekend.Materials;
using SimpleScene;

namespace raytracinginoneweekend
{
    public class RectYZ : IHitable
    {
        float y0, y1, z0, z1, k;
        Material mat;
        SSAABB box;

        public RectYZ(float y0, float y1, float z0, float z1, float k, Material mat)
        {
            this.y0 = y0;
            this.y1 = y1;
            this.z0 = z0;
            this.z1 = z1;
            this.k = k;
            this.mat = mat;

            box.Min = new Vector3(k - 0.0001f, y0, z0);
            box.Max = new Vector3(k + 0.0001f, y1, z1);
        }

        public SSAABB BoundingBox => box;

        public bool Hit(Ray r, float tMin, float tMax, ref HitRecord rec)
        {
            float t = (k - r.Origin.X) / r.Direction.X;
            float y = r.Origin.Y + t * r.Direction.Y;
            float z = r.Origin.Z + t * r.Direction.Z;
            if (y < y0 || y > y1 || z < z0 || z > z1)
                return false;
            rec.U = (y - y0) / (y1 - y0);
            rec.V = (z - z0) / (z1 - z0);
            rec.T = t;
            rec.Material = mat;
            rec.P = r.PointAtParameter(t);
            rec.Normal = new Vector3(1, 0, 0);
            return true;

        }
    }
}
