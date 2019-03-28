using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using SimpleScene;

namespace raytracinginoneweekend.Hitables
{
    public class Translate : IHitable
    {
        IHitable ptr;
        Vector3 displacement;

        public Translate(IHitable ptr, Vector3 displacement)
        {
            this.ptr = ptr;
            this.displacement = displacement;
        }

        public SSAABB BoundingBox => new SSAABB(ptr.BoundingBox.Min + displacement, ptr.BoundingBox.Max + displacement);

        public bool Hit(Ray r, float tMin, float tMax, ref HitRecord rec)
        {
            var moved = new Ray(r.Origin - displacement, r.Direction, r.Time);
            if(ptr.Hit(moved, tMin,tMax,ref rec))
            {
                rec.P += displacement;
                return true;
            }
            return false;
        }
    }
}
