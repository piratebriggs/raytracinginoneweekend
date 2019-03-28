using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using raytracinginoneweekend.Materials;
using SimpleScene;

namespace raytracinginoneweekend.Hitables
{
    public class Triangle : IHitable
    {
        const float kEpsilon = 1e-8f;

        private Vector3 v0, v1, v2;
        private Material Material;
        private SSAABB Box;
        public Vector3 Normal;
        private float Denom;


        public Triangle(Vector3 a, Vector3 b, Vector3 c, Material material)
        {
            this.v0 = a;
            this.v1 = b;
            this.v2 = c;
            this.Material = material;

            Box.Min.X = Math.Min(a.X, Math.Min(b.X, c.X));
            Box.Min.Y = Math.Min(a.Y, Math.Min(b.Y, c.Y));
            Box.Min.Z = Math.Min(a.Z, Math.Min(b.Z, c.Z));
            Box.Max.X = Math.Max(a.X, Math.Max(b.X, c.X));
            Box.Max.Y = Math.Max(a.Y, Math.Max(b.Y, c.Y));
            Box.Max.Z = Math.Max(a.Z, Math.Max(b.Z, c.Z));

            // compute plane's normal
            var v0v1 = v1 - v0;
            var v0v2 = v2 - v0;
            // no need to normalize
            Normal = Vector3.Cross(v0v1, v0v2); // N 
            Denom = Vector3.Dot(Normal, Normal);

        }

        public SSAABB BoundingBox => Box;
        public bool Hit(Ray r, float tMin, float tMax, ref HitRecord rec)
        {

            var mtRec = new HitRecord();
            var mtResult = Möller_Trumbore(r, tMin, tMax, ref mtRec);

            /*
            var origRec = new HitRecord();
            var origResult = HitOrig(r, tMin, tMax, ref origRec);

            if(mtResult != origResult)
            {
                Debugger.Break();
            }
            */
            if (mtResult)
            {
                rec = mtRec;
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Möller_Trumbore(Ray r, float tMin, float tMax, ref HitRecord rec)
        {
            var v0v1 = v1 - v0;
            var v0v2 = v2 - v0;

            var pvec = Vector3.Cross(r.Direction, v0v2);
            var det = Vector3.Dot(v0v1, pvec);

            if (det < kEpsilon) // almost 0 
                return false;  

            float invDet = 1 / det;

            var tvec = r.Origin - v0;
            var u = Vector3.Dot(tvec, pvec) * invDet;
            if (u < 0 || u > 1) return false;

            var qvec = Vector3.Cross(tvec, v0v1);
            var v = Vector3.Dot(r.Direction, qvec) * invDet;
            if (v < 0 || u + v > 1) return false;

            var temp = Vector3.Dot(v0v2, qvec) * invDet;
            if (temp >= tMax || temp <= tMin)
            {
                return false;
            }

            rec.T = temp;
            rec.Normal = Vector3.Normalize(Normal);
            rec.P = r.PointAtParameter(rec.T);
            rec.U = u;
            rec.V = v;
            rec.Material = Material;

            return true;
        }

        public bool HitOrig(Ray r, float tMin, float tMax, ref HitRecord rec)
        {

            // check if ray and plane are parallel ?
            float NdotRayDirection = Vector3.Dot(Normal, r.Direction);
            if (Math.Abs(NdotRayDirection) < kEpsilon) // almost 0 
                return false; // they are parallel so they don't intersect ! 

            // compute t (equation 3)
            var temp = (-Vector3.Dot(Normal, r.Origin - v0)) / NdotRayDirection;
            // check if the triangle is in behind the ray
            if (temp < 0) return false; // the triangle is behind 

            if (temp >= tMax || temp <= tMin)
            {
                return false;
            }

            // compute the intersection point using equation 1
            var p = r.PointAtParameter(temp);
            
            // Step 2: inside-outside test
            Vector3 C; // vector perpendicular to triangle's plane 

            // edge 0
            var edge0 = v1 - v0;
            var vp0 = p - v0;
            C = Vector3.Cross(edge0, vp0);
            if (Vector3.Dot(Normal, C) < 0) return false; // P is on the right side 

            // edge 1
            var edge1 = v2 - v1;
            var vp1 = p - v1;
            C = Vector3.Cross(edge1, vp1);
            var u = Vector3.Dot(Normal, C);
            if (u < 0) return false; // P is on the right side 

            // edge 2
            var edge2 = v0 - v2;
            var vp2 = p - v2;
            C = Vector3.Cross(edge2, vp2);
            var v = Vector3.Dot(Normal, C);
            if (v < 0) return false; // P is on the right side; 

            u /= Denom;
            v /= Denom;

            rec.U = u;
            rec.V = v;
            
            rec.T = temp;
            rec.P = p;
            rec.Normal = Vector3.Normalize(Normal);
            rec.Material = Material;

            return true; // this ray hits   the triangle 
        }
    }
}
