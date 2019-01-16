using raytracinginoneweekend.Materials;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace raytracinginoneweekend
{

    class Program
    {
        public static ImSoRandom Rnd;


        static Vector3 Color(Ray r, IList<IHitable> world, int depth) {
            var rec = new HitRecord();
            if (world.Hit(r, 0.001f, float.MaxValue, ref rec))
            {
                Ray scattered;
                Vector3 attenuation;
                if (depth < 50 && rec.Material.Scatter(r, rec, out attenuation, out scattered))
                {
                    return attenuation * Color(scattered, world, depth + 1);
                }
                else
                {
                    return new Vector3(0);
                }
            }
            var unit_direction = Vector3.Normalize(r.Direction);
            var t = 0.5f * (unit_direction.Y + 1.0f);
            return (1.0f - t) * new Vector3(1.0f, 1.0f, 1.0f) + t * new Vector3(0.5f, 0.7f, 1.0f);
        }

        static void Main(string[] args)
        {
            Rnd = new SunsetquestRandom();

            var x = new Vector3(0, 0, 0);
            int nx = 800;
            int ny = 400;
            int ns = 10;
            
            var world = new List<IHitable>();
            world.Add(new Sphere(new Vector3(0, 0, -1), 0.5f, new Lambertian(new Vector3(0.8f, 0.3f, 0.3f))));
            world.Add(new Sphere(new Vector3(0, -100.5f, -1), 100, new Lambertian(new Vector3(0.8f, 0.8f, 0.0f))));
            world.Add(new Sphere(new Vector3(1, 0, -1), 0.5f, new Metal(new Vector3(0.8f, 0.6f, 0.2f), 0f)));
            world.Add(new Sphere(new Vector3(-1, 0, -1), 0.5f, new Dialectric(1.5f)));
            world.Add(new Sphere(new Vector3(-1, 0, -1), -0.45f, new Dialectric(1.5f)));

            var cam = new Camera();
            var rnd = new Random(123);

            using (Image<Rgba32> image = new Image<Rgba32>(nx, ny))
            {
                Parallel.For(0, ny, index => {
                    var j = ny - 1 - index;
                    var rowSpan = image.GetPixelRowSpan(index);

                    for (int i = 0; i < nx; i++)
                    {
                        var col = new Vector3(0);

                        for(var s = 0;  s < ns; s++)
                        {
                            float u = ((float)i + Rnd.NextFloat()) / (float)nx;
                            float v = ((float)j + Rnd.NextFloat()) / (float)ny;
                            var r = cam.GetRay(u, v);
                            col += Color(r, world, 0);
                        }
                        col /= (float)ns;
                        col = new Vector3((float)Math.Sqrt(col.X), (float)Math.Sqrt(col.Y), (float)Math.Sqrt(col.Z));

                        rowSpan[i] = new Rgba32(col);
                    }
                });
                image.Save("test.png");
            }
        }

        public static Vector3 RandomInUnitSphere()
        {
            var p = new Vector3();
            do
            {
                p = 2.0f * new Vector3(Program.Rnd.NextFloat(), Program.Rnd.NextFloat(), Program.Rnd.NextFloat()) - new Vector3(1, 1, 1);
            } while (p.LengthSquared() >= 1.0f);
            return p;
        }

        public static Vector3 Reflect(Vector3 v, Vector3 n)
        {
            return v - 2 * Vector3.Dot(v, n) * n;
        }

    }
}
