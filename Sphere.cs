using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using raytracinginoneweekend.Materials;
using SimpleScene;

namespace raytracinginoneweekend
{
    public class Sphere : IHitable
    {

        public Sphere(Vector3 center, float radius, Material material)
        {
            Center = center;
            Radius = radius;
            Material = material;

            Box.Min.X = Center.X - Radius;
            Box.Max.X = Center.X + Radius;
            Box.Min.Y = Center.Y - Radius;
            Box.Max.Y = Center.Y + Radius;
            Box.Min.Z = Center.Z - Radius;
            Box.Max.Z = Center.Z + Radius;
        }

        public float Radius;
        public Vector3 Center;
        public Material Material;
        SSAABB Box;

        public  SSAABB BoundingBox
        {
            get {
                return Box;
            }
        }

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
