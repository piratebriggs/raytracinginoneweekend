using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using SimpleScene;
using SimpleScene.Util.ssBVH;

namespace raytracinginoneweekend.Hitables
{
    public class BVH : IHitable
    {
        ssBVH<IHitable> theBVH;
        uint maxTestCount;

        public uint MaxTestCount => maxTestCount;

        public BVH(List<IHitable> world)
        {
            theBVH = new ssBVH<IHitable>(new IHitableBVHNodeAdaptor(), world);
        }

        public SSAABB BoundingBox => throw new NotImplementedException();

        public bool Hit(Ray r, float tMin, float tMax, ref HitRecord rec)
        {
            var tempRec = new HitRecord();
            bool hitAnything = false;
            float closestSoFar = tMax;

            float tnear = 0.001f, tfar = float.MaxValue;
            var hits = theBVH.traverse(box => intersectRayAABox1(r, box, ref tnear, ref tfar));

            uint testCount = 0;
            foreach (var hit in hits)
            {
                if (hit.gobjects != null)
                {
                    foreach (var hitable in hit.gobjects)
                    {
                        testCount++;
                        if (hitable.Hit(r, tMin, closestSoFar, ref tempRec))
                        {
                            hitAnything = true;
                            closestSoFar = tempRec.T;
                            rec = tempRec;
                        }
                    }
                }
            }
            if (testCount > maxTestCount)
            {
                maxTestCount = testCount;
            }
            if(hitAnything)
            {
                rec = tempRec;
                return true;
            }
            return false;
        }

        public static bool intersectRayAABox1(raytracinginoneweekend.Ray ray, SSAABB box, ref float tnear, ref float tfar)
        {
            float tMin = tnear;
            float tMax = tfar;

            if (!intersectPlane(box.Min.X, box.Max.X, ray.Direction.X, ray.Origin.X, ref tMin, ref tMax))
            {
                return false;
            }
            if (!intersectPlane(box.Min.Y, box.Max.Y, ray.Direction.Y, ray.Origin.Y, ref tMin, ref tMax))
            {
                return false;
            }
            if (!intersectPlane(box.Min.Z, box.Max.Z, ray.Direction.Z, ray.Origin.Z, ref tMin, ref tMax))
            {
                return false;
            }
            if(tMax > tfar)
            {
                throw new Exception("tMax > tfar");
            }

            return true;

        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool intersectPlane(float boxMin, float boxMax, float dir, float origin, ref float tmin, ref float tmax)
        {
            // Implemenation of aabb:hit from Raytracing in one weekend
            var invD = 1.0f / dir;
            float t0 = (boxMin - origin) * invD;
            float t1 = (boxMax - origin) * invD;

            if (invD < 0.0f)
            {
                float temp = t0;
                t0 = t1;
                t1 = temp;
            }
            tmin = t0 > tmin ? t0 : tmin;
            tmax = t1 < tmax ? t1 : tmax;

            if (tmax < tmin)
            {
                return false;
            }
            return true;
        }

    }
}
