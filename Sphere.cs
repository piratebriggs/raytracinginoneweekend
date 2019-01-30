using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using raytracinginoneweekend.Materials;

namespace raytracinginoneweekend
{
    public class Sphere : IHitable
    {

        public Sphere(Vector4 center, float radius, IMaterial material)
        {
            Center = center;
            Radius = radius;
            Material = material;
        }

        public float Radius { get; set; }
        public Vector4 Center { get; set; }
        public IMaterial Material { get; set; }

        public bool Hit(Ray r, float tMin, float tMax, ref HitRecord rec)
        {
            var oc = r.Origin - Center;
            var a = Vector4.Dot(r.Direction, r.Direction);
            var b = Vector4.Dot(oc, r.Direction);
            var c = Vector4.Dot(oc, oc) - Radius * Radius;
            var discriminant = b*b - a*c;
            if (discriminant > 0f)
            {
                var temp = (-b - (float)Math.Sqrt(discriminant)) / a;
                if (temp < tMax && temp > tMin)
                {
                    GetHitRec(ref rec, r, temp);
                    return true;
                }
                temp = (-b + (float)Math.Sqrt(discriminant)) / a;
                if (temp < tMax && temp > tMin)
                {
                    GetHitRec(ref rec, r, temp);
                    return true;
                }
            }
            return false;
        }

        private void GetHitRec(ref HitRecord rec, Ray r,float temp)
        {
            rec.T = temp;
            rec.P = r.PointAtParameter(rec.T);
            rec.Normal = (rec.P - Center) / Radius;
            rec.Material = Material;
        }
    }
}
