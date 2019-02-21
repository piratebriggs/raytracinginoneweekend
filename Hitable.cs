using raytracinginoneweekend.Materials;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace raytracinginoneweekend
{
    public struct HitRecord
    {
        public float T;
        public Vector3 P;
        public Vector3 Normal;
        public IMaterial Material;
    }

    public interface IHitable
    {
        bool Hit(Ray r, float tMin, float tMax, ref HitRecord rec);
    }

    public static class HitableListExtensions
    {
        public static bool Hit(this IHitable[] list, Ray r, float tMin, float tMax, ref HitRecord rec)
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
