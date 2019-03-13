using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using raytracinginoneweekend.Materials;
using SimpleScene;

namespace raytracinginoneweekend.Hitables
{
    public class RectXY : IHitable
    {
        float x0, x1, y0, y1, k;
        Material mat;
        SSAABB box;

        public RectXY(float x0, float x1, float y0, float y1, float k, Material mat)
        {
            this.x0 = x0;
            this.x1 = x1;
            this.y0 = y0;
            this.y1 = y1;
            this.k = k;
            this.mat = mat;

            box.Min = new Vector3(x0, y0, k - 0.0001f);
            box.Max = new Vector3(x1, y1, k + 0.0001f);
        }

        public SSAABB BoundingBox => box;

        public bool Hit(Ray r, float tMin, float tMax, ref HitRecord rec)
        {
            float t = (k - r.Origin.Z) / r.Direction.Z;
            float x = r.Origin.X + t * r.Direction.X;
            float y = r.Origin.Y + t * r.Direction.Y;
            if (x < x0 || x > x1 || y < y0 || y > y1)
                return false;
            rec.U = (x - x0) / (x1 - x0);
            rec.V = (y - y0) / (y1 - y0);
            rec.T = t;
            rec.Material = mat;
            rec.P = r.PointAtParameter(t);
            rec.Normal = new Vector3(0, 0, 1);
            return true;

        }
    }
}
