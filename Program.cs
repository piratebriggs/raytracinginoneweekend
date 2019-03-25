using raytracinginoneweekend.Hitables;
using raytracinginoneweekend.Materials;
using raytracinginoneweekend.Textures;
using SimpleScene;
using SimpleScene.Util.ssBVH;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace raytracinginoneweekend
{

    class Program
    {
        public static Stopwatch sw = new Stopwatch();

        static Vector3 Color(Ray r, IHitable[] world, int depth, ImSoRandom rnd, ref uint rayCount)
        {
            rayCount++;

            var rec = new HitRecord();
            if(world.Hit(r,0.001f,float.MaxValue,ref rec))
            {
                Ray scattered;
                Vector3 attenuation;
                var emitted = rec.Material.Emitted(0, 0, ref rec.P);
                if (depth < 50 && rec.Material.Scatter(r, rec, out attenuation, out scattered, rnd))
                {
                    return emitted + attenuation * Color(scattered, world, depth + 1, rnd, ref rayCount);
                }
                else
                {
                    return emitted;
                }
            }
            return Vector3.Zero;
        }

        private static (List<IHitable>, Camera) RandomScene(ImSoRandom rnd, int nx, int ny)
        {
            var world = new List<IHitable>();

            var texture = new CheckerTexture(new ConstantTexture(0.2f, 0.3f, 0.1f), new ConstantTexture(0.9f, 0.9f, 0.9f));
            world.Add(new Sphere(new Vector3(0, -1000f, 0), 1000, new Lambertian(texture)));

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
                            world.Add(new MovingSphere(center, center + new Vector3(0, 0.5f * rnd.NextFloat(), 0), 0.0f, 1.0f, 0.2f, new Lambertian(new ConstantTexture(rnd.NextFloat() * rnd.NextFloat(), rnd.NextFloat() * rnd.NextFloat(), rnd.NextFloat() * rnd.NextFloat()))));
                            //world.Add(new Sphere(center, 0.2f, new Lambertian(new Vector3(rnd.NextFloat() * rnd.NextFloat(), rnd.NextFloat() * rnd.NextFloat(), rnd.NextFloat() * rnd.NextFloat()))));
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
            world.Add(new Sphere(new Vector3(-4, 1, 0), 1.0f, new Lambertian(new ConstantTexture(0.4f, 0.2f, 0.1f))));
            world.Add(new Sphere(new Vector3(4, 1, 0), 1.0f, new Metal(new Vector3(0.7f, 0.6f, 0.5f), 0f)));

            world.Add(new Sphere(new Vector3(0, 0, 0), 20.0f, new DiffuseLight(new ConstantTexture(new Vector3(1,1,1)))));


            var lookFrom = new Vector3(13, 2, 3);
            var lookAt = new Vector3(0, 0, 0);
            var distToFocus = 10;
            var aperture = 0.1f;

            var cam = new Camera(lookFrom, lookAt, new Vector3(0, 1, 0), 20, (float)nx / (float)ny, aperture, distToFocus, 0.0f, 1.0f);

            return (world, cam);
        }

        private static (List<IHitable>, Camera) PoolScene(ImSoRandom rnd, int nx, int ny)
        {
            var world = new List<IHitable>();
            world.Add(new Sphere(new Vector3(0, -1000f, 0), 1000, new Lambertian(new ConstantTexture(0.33f, 0.67f, 0.0f))));
            var red = new ConstantTexture(0.68f, 0.13f, 0.16f);
            var yellow = new ConstantTexture(1f, 0.74f, 0.13f);
            var black = new ConstantTexture(0.14f, 0.07f, 0.07f);
            var white = new ConstantTexture(1f, 1f, 0.9f);

            for (var a = 1f; a <= 5; a += 1f)
            {
                var counter = 0 - a;
                for (var b = 0f; b < a; b += 1f)
                {
                    var center = new Vector3(a * 0.9f - 5f, 0.5f, a / 2f - b - 0.5f);
                    var colour = counter % 2 == 0 ? red : yellow;
                    if (a == 3 && b == 1)
                    {
                        colour = black;
                    } if (a == 3 && b == 2) {
                        colour = red;
                    } if (a == 5 && b == 4)
                    {
                        colour = red;
                    }
                    world.Add(new Sphere(center, 0.45f,
                        new Lambertian(colour)));
                    world.Add(new Sphere(center, 0.5f,
                        new Dialectric(1.5f)));
                    counter++;
                }
            }

            var cueCenter = new Vector3(-6, 0.5f, 0);
            var cueCenter1 = cueCenter + new Vector3(0.75f * rnd.NextFloat(), 0, 0);
            world.Add(new MovingSphere(cueCenter, cueCenter1, 0.0f, 1.0f, 0.5f,
                new Dialectric(1.5f)));
            world.Add(new MovingSphere(cueCenter, cueCenter1, 0.0f, 1.0f, 0.45f,
                new Lambertian(white)));

            world.Add(new RectXZ(-3,3,-2,2,5,new DiffuseLight(new ConstantTexture(new Vector3(15,15,15)))));

            var lookFrom = new Vector3(-9, 3.5f, 12);
            var lookAt = new Vector3(-3, 0, 0);
            var distToFocus = (lookFrom - new Vector3(-6, 0.5f, 0)).Length();
            var aperture = 0.5f;

            var cam = new Camera(lookFrom, lookAt, new Vector3(0, 1, 0), 20, (float)nx / (float)ny, aperture, distToFocus, 0.0f, 1.0f);

            return (world, cam);
        }

        private static (List<IHitable>, Camera) CornellScene(ImSoRandom rnd, int nx, int ny)
        {
            var world = new List<IHitable>();

            var grad = new Lambertian(new ColourTexture());
            var blue = new Lambertian(new ConstantTexture(0.05f, 0.05f, 0.65f));
            var red = new Lambertian(new ConstantTexture(0.65f, 0.05f, 0.05f));
            var white = new Lambertian(new ConstantTexture(0.73f, 0.73f, 0.73f));
            var green = new Lambertian(new ConstantTexture(0.12f, 0.45f, 0.15f));
            var light = new DiffuseLight(new ConstantTexture(15f, 15f, 15f));
            world.Add(new RectXZ(213, 343, 227, 332, 554, light));
            
            world.Add(new FlipNormals(new RectYZ(0, 555, 0, 555, 555, green)));
            world.Add(new RectYZ(0, 555, 0, 555, 0, red));
            world.Add(new FlipNormals(new RectXZ(0, 555, 0, 555, 555, white)));
            world.Add(new RectXZ(0, 555, 0, 555, 0, white));
            world.Add(new FlipNormals(new RectXY(0, 555, 0, 555, 555, white)));

            //world.Add( new Translate( new Box(Vector3.Zero, new Vector3(165, 165, 165), white), new Vector3(130, 0, 65)));
            //world.Add( new Translate( new Box(Vector3.Zero, new Vector3(165, 330, 165), white), new Vector3(265, 0, 295)));

            //world.Add(new Sphere(new Vector3(0, 0, 0), 3000.0f, new DiffuseLight(new ConstantTexture(new Vector3(1, 1, 1)))));
            /*
            for (int i = 0; i < 50; i++)
            {
                var a = rnd.RandomVector();
                var b = rnd.RandomVector();
                var c = rnd.RandomVector();

                //world.Add(new Translate(new Triangle(new Vector3(rnd.NextFloat() * 555f, rnd.NextFloat() * 555f, rnd.NextFloat() * 555f), new Vector3(rnd.NextFloat() * 555f, rnd.NextFloat() * 555f, rnd.NextFloat() * 555f), new Vector3(rnd.NextFloat() * 555f, rnd.NextFloat() * 555f, rnd.NextFloat() * 555f), red),new Vector3(10 + rnd.NextFloat() * 100f, 10 + rnd.NextFloat() * 100f, 10 + rnd.NextFloat() * 100f)));
                world.Add(new Triangle(a, b, c, blue));
                world.Add(new FlipNormals( new Triangle(a, b, c, blue)));
            }
            
    */
            world.Add(new Triangle( new Vector3(340, 90, 62),new Vector3(100, 320, 190), new Vector3(262, 331, 400), grad));

            var lookFrom = new Vector3(278, 278, -800);
            var lookAt = new Vector3(278, 278, 0);
            var distToFocus = 10;
            var aperture = 0.1f;
            var vFov = 40;

            var cam = new Camera(lookFrom, lookAt, new Vector3(0, 1, 0), vFov, (float)nx / (float)ny, aperture, distToFocus, 0.0f, 1.0f);

            return (world, cam);
        }

        static void Main(string[] args)
        {
            var x = new Vector3(0, 0, 0);
            int nx = 300;
            int ny = 200;
            int ns = 100;
            var singleThread = false;

            var (world, cam) = CornellScene(new SunsetquestRandom(), nx, ny);

            var worldBVH = new BVH(world);
            var wl = new IHitable[] { worldBVH };


            uint totalRayCount = 0;
            using (Image<Rgba32> image = new Image<Rgba32>(nx, ny))
            {
                sw.Start();
                if(singleThread)
                {
                    var rnd = new SunsetquestRandom();
                    for (int j = 0; j < ny; j++)
                    {
                        RenderRow(image, wl, cam, j, nx, ny, ns, rnd, ref totalRayCount);
                    }
                }
                else
                {
                    Parallel.For(0, ny, () => new SunsetquestRandom(), (j, loop, rnd) =>
                    {
                        RenderRow(image, wl, cam, j, nx, ny, ns, rnd, ref totalRayCount);
                        return rnd;
                    }, (rnd) => { });
                }
                sw.Stop();
                image.Save("test.png");
            }
            float seconds = sw.ElapsedMilliseconds / 1000f;
            float rate = totalRayCount / seconds;
            float mRate = rate / 1_000_000;

            Console.WriteLine($"totalRayCount: {totalRayCount}");
            Console.WriteLine($"BVH max depth: {worldBVH.MaxTestCount}");
            Console.WriteLine($"Duration: {seconds} | Rate: {mRate} MRays / sec.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void RenderRow(Image<Rgba32> image, IHitable[] wl, Camera cam, int j, int nx, int ny, int ns, ImSoRandom rnd, ref uint rayCount)
        {
            var index = ny - 1 - j;
            var rowSpan = image.GetPixelRowSpan(index);

            for (int i = 0; i < nx; i++)
            {
                var col = new Vector3(0);

                for (var s = 0; s < ns; s++)
                {
                    float u = ((float)i + rnd.NextFloat()) / (float)nx;
                    float v = ((float)j + rnd.NextFloat()) / (float)ny;
                    var r = cam.GetRay(u, v, rnd);
                    col += Color(r, wl, 0, rnd, ref rayCount);

                }

                col /= (float)ns;
                col = new Vector3((float)Math.Sqrt(col.X), (float)Math.Sqrt(col.Y), (float)Math.Sqrt(col.Z));

                rowSpan[i] = new Rgba32(col);
            }

        }
    }
}
