﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using raytracinginoneweekend.Materials;

namespace raytracinginoneweekend
{
    public class MovingSphere : IHitable
    {

        public MovingSphere(Vector3 center0, Vector3 center1,float t0, float t1 , float radius, IMaterial material)
        {
            Center0 = center0;
            Center1 = center1;
            T0 = t0;
            T1 = t1;
            Radius = radius;
            Material = material;
        }

        public float Radius;
        public Vector3 Center0;
        public Vector3 Center1;
        public float T0;
        public float T1;
        public IMaterial Material;

        public Vector3 Pos => Center0 + (Center1 - Center0);

        float IHitable.Radius => Radius + (Center1 - Center0).Length();

        public bool Hit(Ray r, float tMin, float tMax, ref HitRecord rec)
        {
            var oc = r.Origin - Center(r.Time);
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

        private Vector3 Center(float time)
        {
            return Center0 + ((time - T0) / (T1 - T0)) * (Center1 - Center0);
        }

        private void GetHitRec(ref HitRecord rec, Ray r,float temp)
        {
            rec.T = temp;
            rec.P = r.PointAtParameter(rec.T);
            rec.Normal = (rec.P - Center(r.Time)) / Radius;
            rec.Material = Material;
        }
    }
}
