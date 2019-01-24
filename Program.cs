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

        static Vector3 Color(Ray r, IList<IHitable> world, int depth, ImSoRandom rnd) {
            var rec = new HitRecord();
            if (world.Hit(r, 0.001f, float.MaxValue, ref rec))
            {
                Ray scattered;
                Vector3 attenuation;
                if (depth < 50 && rec.Material.Scatter(r, rec, out attenuation, out scattered, rnd))
                {
                    return attenuation * Color(scattered, world, depth + 1, rnd);
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
            var startTime = DateTime.Now;

            var x = new Vector3(0, 0, 0);
            int nx = 800;
            int ny = 400;
            int ns = 1;
            
            var world = new List<IHitable>();
/*       
            var R = (float)Math.Cos(Math.PI / 4);
            world.Add(new Sphere(new Vector3(-R, 0, -1), R, new Lambertian(new Vector3(0f, 0f, 1f))));
            world.Add(new Sphere(new Vector3(R, 0, -1), R, new Lambertian(new Vector3(1f, 0f, 0f))));
*/
            world.Add(new Sphere(new Vector3(0, 0, -1), 0.5f, new Lambertian(new Vector3(0.8f, 0.3f, 0.3f))));
            world.Add(new Sphere(new Vector3(0, -100.5f, -1), 100, new Lambertian(new Vector3(0.8f, 0.8f, 0.0f))));
            world.Add(new Sphere(new Vector3(1, 0, -1), 0.5f, new Metal(new Vector3(0.8f, 0.6f, 0.2f), 0f)));
            world.Add(new Sphere(new Vector3(-1, 0, -1), 0.5f, new Dialectric(1.5f)));
            world.Add(new Sphere(new Vector3(-1, 0, -1), -0.45f, new Dialectric(1.5f)));

            var lookFrom = new Vector3(3, 3, 2);
            var lookAt = new Vector3(0, 0, -1);
            var distToFocus = (lookFrom - lookAt ).Length();
            var aperture = 2f;

            var cam = new Camera(lookFrom, lookAt, new Vector3(0, 1, 0), 20, (float)nx / (float)ny, aperture, distToFocus);

            ImSoRandom[] rnd = new ImSoRandom[ns];
            for (var index = 0; index < ns; index++) {
                rnd[index] = new SunsetquestRandom();
            }
            var lockobj = new object();

            using (Image<Rgba32> image = new Image<Rgba32>(nx, ny))
            {
                for (int j = 0; j < ny; j++)
                {
                    var rowSpan = image.GetPixelRowSpan(j);

                    for (int i = 0; i < nx; i++)
                    {
                        var col = new Vector3(0);

                        Parallel.For(0, ns, (index) =>
                        {
                            float u = ((float)i + rnd[index].NextFloat()) / (float)nx;
                            float v = ((float)j + rnd[index].NextFloat()) / (float)ny;
                            var r = cam.GetRay(u, v, rnd[index]);
                            var indCol = Color(r, world, 0, rnd[index]);
                           
                        });

                        col /= (float)ns;
                        col = new Vector3((float)Math.Sqrt(col.X), (float)Math.Sqrt(col.Y), (float)Math.Sqrt(col.Z));

                        rowSpan[i] = new Rgba32(col);
                    }
                }
                image.Save("test.png");
                var duration = DateTime.Now - startTime;
                Console.WriteLine($"Duration: {duration}");
                Console.ReadLine();
            }
        }


        public static Vector3 Reflect(Vector3 v, Vector3 n)
        {
            return v - 2 * Vector3.Dot(v, n) * n;
        }

    }
}
