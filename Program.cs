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

        static Vector3 Color(Ray r, IHitable[] world, int depth, ImSoRandom rnd) {
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

        private static List<IHitable> RandomScene(ImSoRandom rnd)
        {
            var world = new List<IHitable>();
            world.Add(new Sphere(new Vector3(0, -1000f, 0), 1000, new Lambertian(new Vector3(0.5f, 0.5f, 0.5f))));

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

        private static List<IHitable> PoolScene(ImSoRandom rnd)
        {
            var world = new List<IHitable>();
            world.Add(new Sphere(new Vector3(0, -1000f, 0), 1000, new Lambertian(new Vector3(0.33f, 0.67f, 0.0f))));
            var red = new Vector3(0.68f, 0.13f, 0.16f);
            var yellow = new Vector3(1f, 0.74f, 0.13f);
            var black = new Vector3(0.14f, 0.07f, 0.07f);
            var white = new Vector3(1f, 1f, 0.9f);

            for (var a = 1f; a <= 5; a+=1f)
            {
                var counter = 0-a;
                for (var b = 0f; b < a; b += 1f)
                {
                    var center = new Vector3(a*0.9f - 5f , 0.5f,  a/2f - b - 0.5f );
                    var colour = a == 3 && b == 1 ? black : counter % 2 == 0 ? red : yellow;
                    world.Add(new Sphere(center, 0.5f,
                        new Metal(colour, 0.1f)));
                    counter++;
                }
            }

            world.Add(new Sphere(new Vector3(-6,0.5f,0), 0.5f,
                new Metal(white, 0.1f)));

            return world;
        }


        static void Main(string[] args)
        {
            var startTime = DateTime.Now;

            var x = new Vector3(0, 0, 0);
            int nx = 600;
            int ny = 400;
            int ns = 30;

            var world = PoolScene(new SunsetquestRandom());
            var wl = world.ToArray();

            var lookFrom = new Vector3(-8, 3.5f, 10);
            var lookAt = new Vector3(-3, 0, 0);
            var distToFocus = (lookFrom - new Vector3(-6, 0.5f, 0)).Length();
            var aperture = 0.5f;

            var cam = new Camera(lookFrom, lookAt, new Vector3(0, 1, 0), 20, (float)nx / (float)ny, aperture, distToFocus);

            using (Image<Rgba32> image = new Image<Rgba32>(nx, ny))
            {
                Parallel.For(0, ny, () => new SunsetquestRandom(), (index, loop, rnd) =>
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
                            col += Color(r, wl, 0, rnd);

                        }

                        col /= (float)ns;
                        col = new Vector3((float)Math.Sqrt(col.X), (float)Math.Sqrt(col.Y), (float)Math.Sqrt(col.Z));

                        rowSpan[i] = new Rgba32(col);
                    }
                    return rnd;
                }, (rnd) => { });
                var duration = DateTime.Now - startTime;
                image.Save("test.png");
                Console.WriteLine($"Duration: {duration}");
            }
        }
    }
}
