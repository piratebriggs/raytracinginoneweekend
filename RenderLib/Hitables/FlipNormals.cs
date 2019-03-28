using System;
using System.Collections.Generic;
using System.Text;
using SimpleScene;

namespace raytracinginoneweekend.Hitables
{
    public class FlipNormals : IHitable
    {
        IHitable ptr;
        public FlipNormals(IHitable ptr)
        {
            this.ptr = ptr;
        }

        public SSAABB BoundingBox => ptr.BoundingBox;

        public bool Hit(Ray r, float tMin, float tMax, ref HitRecord rec)
        {
            if(ptr.Hit(r,tMin,tMax,ref rec))
            {
                rec.Normal = -rec.Normal;
                return true;
            }
            return false;
        }
    }
}
