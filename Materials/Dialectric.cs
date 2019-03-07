using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace raytracinginoneweekend.Materials
{
    public class Dialectric : Material
    {
        private float _refIndex;

        public Dialectric(float  ri)
        {
            _refIndex = ri;
        }

        public override bool Scatter(Ray rayIn, HitRecord rec, out Vector3 attenuation, out Ray scattererd, ImSoRandom random)
        {
            Vector3 reflected = Vector3.Reflect(rayIn.Direction, rec.Normal);
            attenuation = new Vector3(1.0f);

            Vector3 outward_normal;
            float niOverNt;
            float cosine;
            if(Vector3.Dot(rayIn.Direction,rec.Normal)>0)
            {
                outward_normal = -rec.Normal;
                niOverNt = _refIndex;
                cosine = _refIndex * Vector3.Dot(rayIn.Direction, rec.Normal) / rayIn.Direction.Length();
            }else
            {
                outward_normal = rec.Normal;
                niOverNt = 1.0f / _refIndex;
                cosine = -Vector3.Dot(rayIn.Direction, rec.Normal) / rayIn.Direction.Length();
            }

            Vector3 refracted;
            float reflectProb;
            if (Refract(rayIn.Direction,outward_normal,niOverNt,out refracted))
            {
                reflectProb = Schlick(cosine, _refIndex);
            } else
            {
                reflectProb = 1.0f;
            }

            if(random.NextFloat() < reflectProb)
            {
                scattererd = new Ray(rec.P, reflected, rayIn.Time);
            } else
            {
                scattererd = new Ray(rec.P, refracted, rayIn.Time);
            }
            return true;
        }

        float Schlick(float cosine, float refIndex)
        {
            float r0 = (1 - refIndex) / (1 + refIndex);
            r0 = r0 * r0;
            return r0 + (1f - r0) * (float)Math.Pow((1f - cosine), 5f);
        }

        bool Refract(Vector3 v, Vector3 n, float niOverNt , out Vector3 refracted)
        {
            var uv = Vector3.Normalize(v);
            var dot = Vector3.Dot(uv, n);
            float discriminant = 1.0f - niOverNt * niOverNt * (1f - dot * dot);
            if (discriminant > 0)
            {
                refracted = niOverNt * (uv - n * dot) - n * (float)Math.Sqrt(discriminant);
                return true;
            }
            else
            {
                refracted = new Vector3(0);
                return false;
            }
        }
    }
}
