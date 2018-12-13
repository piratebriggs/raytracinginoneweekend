using System;
using System.Numerics;

namespace raytracinginoneweekend
{
    class Ray
    {
        public Ray(Vector3 origin, Vector3 direction)
        {
            A = origin; B = direction;
        }
        public Vector3 A { get; set; }
        public Vector3 B { get; set; }
        public Vector3 Origin { get { return A; } }
        public Vector3 Direction { get { return B; } }
        public Vector3 PointAtParameter(float t) { return A + t * B; }
    }

    class Program
    {

        static float HitSphere(Vector3 center, float radius, Ray r)
        {
            var oc = r.Origin - center;
            var a = Vector3.Dot(r.Direction, r.Direction);
            var b = 2.0f * Vector3.Dot(oc, r.Direction);
            var c = Vector3.Dot(oc, oc) - radius * radius;
            var discriminant = b * b - 4 * a * c;
            return (discriminant < 0) ? -1.0f : (-b - (float)Math.Sqrt(discriminant)) / (2.0f * a);
        }

        static Vector3 Color(Ray r) {
            var spherePos = new Vector3(0, 0, -1);
            var t = HitSphere(spherePos, 0.5f, r);
            if(t > 0)
            {
                var N = Vector3.Normalize(r.PointAtParameter(t) - spherePos);
                return 0.5f * new Vector3(N.X + 1, N.Y + 1, N.Z + 1);
            }
            var unit_direction = Vector3.Normalize(r.Direction);
            t = 0.5f * (unit_direction.Y + 1.0f);
            return (1.0f - t) * new Vector3(1.0f, 1.0f, 1.0f) + t * new Vector3(0.5f, 0.7f, 1.0f);
        }

        static void Main(string[] args)
        {
            var x = new Vector3(0, 0, 0);
            int nx = 2000;
            int ny = 1000;

            var origin = new Vector3(0.0f);
            var lower_left_corner = new Vector3(-2.0f, -1.0f, -1.0f);
            var horz = new Vector3(4,0,0);
            var vert = new Vector3(0,2,0);

            Console.Write($"P3\n{nx} {ny}\n255\n");

            for (int j = ny - 1; j >= 0; j--)
            {
                for (int i = 0; i < nx; i++)
                {
                    float u = (float)i / (float)nx;
                    float v = (float)j / (float)ny;

                    var r = new Ray(origin, lower_left_corner +u * horz + v * vert);
                    var col = Color(r);

                    var ir = (int)(255.99 * col.X);
                    var ig = (int)(255.99 * col.Y);
                    var ib = (int)(255.99 * col.Z);
                    Console.Write( $"{ir} {ig} {ib}\n");
                }
            }
        }
    }
}
