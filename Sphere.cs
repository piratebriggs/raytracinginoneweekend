using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using raytracinginoneweekend.Materials;

namespace raytracinginoneweekend
{
    public class Sphere : IHitable
    {

        public Sphere(Vector3 center, float radius, IMaterial material)
        {
            Center = center;
            Radius = radius;
            Material = material;
        }

        public float Radius { get; set; }
        public Vector3 Center { get; set; }
        public IMaterial Material { get; set; }

        public bool Hit(Ray r, float tMin, float tMax, ref HitRecord rec)
        {
            var oc = r.Origin - Center;
            var a = Vector3.Dot(r.Direction, r.Direction);
            var b = Vector3.Dot(oc, r.Direction);
            var c = Vector3.Dot(oc, oc) - Radius * Radius;
            var discriminant = b*b - a*c;
            if(discriminant > 0)
            {
                var temp = (-b - (float)Math.Sqrt(discriminant)) / a;
                if (temp < tMax && temp > tMin)
                {
                    GetHitRec(r, temp, ref rec);
                    return true;
                }
                temp = (-b + (float)Math.Sqrt(discriminant)) / a;
                if (temp < tMax && temp > tMin)
                {
                    GetHitRec(r, temp, ref rec);
                    return true;
                }
            }
            return false;
        }

        private void GetHitRec(Ray r, float temp, ref HitRecord rec)
        {
            var p = r.PointAtParameter(temp);
            rec.T = temp;
            rec.P = p;
            rec.Normal = (p - Center) / Radius;
            rec.Material = Material;
        }
    }
}
