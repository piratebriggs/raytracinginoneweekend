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

        private static IList<IHitable> RandomScene(ImSoRandom rnd)
        {
            var world = new List<IHitable>();
            world.Add(new Sphere(new Vector3(0, -1000f, 0), 1000, new Lambertian(new Vector3(0.5f, 0.5f, 0.5f))));

            int i = 1;
            for (int a = -11; a < 11; a++)
            {
                for (int b = -11; b < 11; b++)
                {
                    float choose_mat = rnd.NextFloat();
                    var center = new Vector3(a + 0.9f * rnd.NextFloat(), 0.2f, b + 0.9f * rnd.NextFloat());
                    if ((center - new Vector3(4, 0.2f, 0)).Length() > 0.9)
                    {
                        if (choose_mat < 0.8)
                        {  // diffuse
                            world.Add(new Sphere(center, 0.2f, new Lambertian(new Vector3(rnd.NextFloat() * rnd.NextFloat(), rnd.NextFloat() * rnd.NextFloat(), rnd.NextFloat() * rnd.NextFloat()))));
                        }
                        else if (choose_mat < 0.95)
                        { // metal
                            world.Add(new Sphere(center, 0.2f,
                            new Metal(new Vector3(0.5f * (1 + rnd.NextFloat()), 0.5f * (1 + rnd.NextFloat()), 0.5f * (1 + rnd.NextFloat())), 0.5f * rnd.NextFloat())));
                        }
                        else
                        {  // glass
                            world.Add(new Sphere(center, 0.2f, new Dialectric(1.5f)));
                        }
                    }
                }
            }

            world.Add(new Sphere(new Vector3(0, 1, 0), 1.0f, new Dialectric(1.5f)));
            world.Add(new Sphere(new Vector3(-4, 1, 0), 1.0f, new Lambertian(new Vector3(0.4f, 0.2f, 0.1f))));
            world.Add(new Sphere(new Vector3(4, 1, 0), 1.0f, new Metal(new Vector3(0.7f, 0.6f, 0.5f), 0f)));

            return world;
        }

        static void Main(string[] args)
        {
            var startTime = DateTime.Now;

            var x = new Vector3(0, 0, 0);
            int nx = 600;
            int ny = 400;
            int ns = 10;

            var world = RandomScene(new SunsetquestRandom());

            var lookFrom = new Vector3(13, 2, 3);
            var lookAt = new Vector3(0, 0, 0);
            var distToFocus = 10;
            var aperture = 0.1f;

            var cam = new Camera(lookFrom, lookAt, new Vector3(0, 1, 0), 20, (float)nx / (float)ny, aperture, distToFocus);
            var rnd = new SunsetquestRandom();
            using (Image<Rgba32> image = new Image<Rgba32>(nx, ny))
            {
                for(var index = 0; index < ny; index++)
                {
                    var j = ny - 1 - index;
                    var rowSpan = image.GetPixelRowSpan((int)index);

                    for (int i = 0; i < nx; i++)
                    {
                        var col = new Vector3(0);

                        for (var s = 0; s < ns; s++)
                        {
                            float u = ((float)i + rnd.NextFloat()) / (float)nx;
                            float v = ((float)j + rnd.NextFloat()) / (float)ny;
                            var r = cam.GetRay(u, v, rnd);
                            col += Color(r, world, 0, rnd);

                        }

                        col /= (float)ns;
                        col = new Vector3((float)Math.Sqrt(col.X), (float)Math.Sqrt(col.Y), (float)Math.Sqrt(col.Z));

                        rowSpan[i] = new Rgba32(col);
                    }
                   
                }
                image.Save("test.png");
                var duration = DateTime.Now - startTime;
                Console.WriteLine($"Duration: {duration}");
            }
        }
    }
}
