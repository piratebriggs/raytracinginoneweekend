using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using raytracinginoneweekend.Materials;
using SimpleScene;

namespace raytracinginoneweekend.Hitables
{
    public class Box : IHitable
    {
        public Vector3 pmin;
        public Vector3 pmax;
        public Material material;
        public IHitable[] rectangles = new IHitable[6];

        public Box(Vector3 pmin, Vector3 pmax, Material material)
        {
            this.pmin = pmin;
            this.pmax = pmax;
            this.material = material;

            rectangles[0] = new RectXY(pmin.X, pmax.X, pmin.Y, pmax.Y, pmax.Z, material);
            rectangles[1] = new FlipNormals(new RectXY(pmin.X, pmax.X, pmin.Y, pmax.Y, pmin.Z, material));
            rectangles[2] = new RectXZ(pmin.X, pmax.X, pmin.Z, pmax.Z, pmax.Y, material);
            rectangles[3] = new FlipNormals(new RectXZ(pmin.X, pmax.X, pmin.Z, pmax.Z, pmin.Y, material));
            rectangles[4] = new RectYZ(pmin.Y, pmax.Y, pmin.Z, pmax.Z, pmax.X, material);
            rectangles[5] = new FlipNormals(new RectYZ(pmin.Y, pmax.Y, pmin.Z, pmax.Z, pmin.X, material));
        }

        public SSAABB BoundingBox => new SSAABB(pmin, pmax);

        public bool Hit(Ray r, float tMin, float tMax, ref HitRecord rec)
        {
            return rectangles.Hit(r, tMin, tMax, ref rec);
        }
    }
}
