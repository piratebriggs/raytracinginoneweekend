using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace raytracinginoneweekend
{
    public struct HitRecord
    {
        public float T { get; set; }
        public Vector3 P { get; set; }
        public Vector3 Normal { get; set; }
    }

    public interface IHitable
    {
        bool Hit(Ray r, float tMin, float tMax, ref HitRecord rec);
    }

    public static class HitableList
    {
        public static bool Hit(this IList<IHitable> list, Ray r, float tMin, float tMax, ref HitRecord rec)
        {
            var tempRec = new HitRecord();
            bool hitAnything = false;
            float closestSoFar = tMax;
            foreach(var hitable in list)
            {
                if (hitable.Hit(r, tMin, closestSoFar, ref tempRec))
                {
                    hitAnything = true;
                    closestSoFar = tempRec.T;
                    rec = tempRec;
                }
            }
            return hitAnything;
        }
    }

}
