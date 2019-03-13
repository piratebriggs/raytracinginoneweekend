using raytracinginoneweekend.Materials;
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
        public static bool intersectRayAABox1(raytracinginoneweekend.Ray ray, SSAABB box, ref float tnear, ref float tfar)
        {
            float tMin = 0.001f;
            float tMax = float.MaxValue;

            if (!intersectPlane(box.Min.X, box.Max.X, ray.Direction.X, ray.Origin.X, ref tMin, ref tMax))
            {
                return false;
            }
            if (!intersectPlane(box.Min.Y, box.Max.Y, ray.Direction.Y, ray.Origin.Y, ref tMin, ref tMax))
            {
                return false;
            }
            if (!intersectPlane(box.Min.Z, box.Max.Z, ray.Direction.Z, ray.Origin.Z, ref tMin, ref tMax))
            {
                return false;
            }
            return true;

        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool intersectPlane(float boxMin, float boxMax, float dir, float origin, ref float tmin, ref float tmax)
        {
            // Implemenation of aabb:hit from Raytracing in one weekend
            var invD = 1.0f / dir;
            float t0 = (boxMin - origin) * invD;
            float t1 = (boxMax - origin) * invD;

            if (invD < 0.0f)
            {
                float temp = t0;
                t0 = t1;
                t1 = temp;
            }
            tmin = t0 > tmin ? t0 : tmin;
            tmax = t1 < tmax ? t1 : tmax;

            if (tmax < tmin)
            {
                return false;
            }
            return true;
        }

        static Vector3 Color(Ray r, ssBVH<IHitable> world, int depth, ImSoRandom rnd, ref uint rayCount) {
            rayCount++;

            var rec = new HitRecord();
            var tempRec = new HitRecord();
            bool hitAnything = false;
            float closestSoFar = float.MaxValue;

            float tnear = 0f, tfar = 0f;
            var hits = world.traverse(box => intersectRayAABox1(r, box, ref tnear, ref tfar));

            foreach (var hit in hits)
            {
                if (hit.gobjects != null)
                {
                    foreach (var hitable in hit.gobjects)
                    {
                        if (hitable.Hit(r, 0.001f, closestSoFar, ref tempRec))
                        {
                            hitAnything = true;
                            closestSoFar = tempRec.T;
                            rec = tempRec;
                        }
                    }
                }
            }

            if (hitAnything)
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
            var red = new Vector3(0.68f, 0.13f, 0.16f);
            var yellow = new Vector3(1f, 0.74f, 0.13f);
            var black = new Vector3(0.14f, 0.07f, 0.07f);
            var white = new Vector3(1f, 1f, 0.9f);

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
                    world.Add(new Sphere(center, 0.5f,
                        new Metal(colour, 0.5f)));
                    counter++;
                }
            }

            var cueCenter = new Vector3(-6, 0.5f, 0);
            world.Add(new MovingSphere(cueCenter, cueCenter + new Vector3(0.75f * rnd.NextFloat(), 0, 0), 0.0f, 1.0f, 0.5f,
                new Metal(white, 0.1f)));

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

            var red = new Lambertian(new ConstantTexture(0.65f, 0.05f, 0.05f));
            var white = new Lambertian(new ConstantTexture(0.73f, 0.73f, 0.73f));
            var green = new Lambertian(new ConstantTexture(0.12f, 0.45f, 0.15f));
            var light = new DiffuseLight(new ConstantTexture(15f, 15f, 15f));

            world.Add(new RectYZ(0, 555, 0, 555, 555, green));
            world.Add(new RectYZ(0, 555, 0, 555, 0, red));
            world.Add(new RectXZ(213, 343, 227, 332, 554, light));
            world.Add(new RectXZ(0, 555, 0, 555, 555, white));
            world.Add(new RectXZ(0, 555, 0, 555, 0, white));
            world.Add(new RectXY(0, 555, 0, 555, 555, white));

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
            int nx = 600;
            int ny = 400;
            int ns = 10;
            var singleThread = false;

            var (world, cam) = CornellScene(new SunsetquestRandom(), nx, ny);
            var wl = world.ToArray();

            var worldBVH = new ssBVH<IHitable>(new IHitableBVHNodeAdaptor(), world);

            uint totalRayCount = 0;
            using (Image<Rgba32> image = new Image<Rgba32>(nx, ny))
            {
                sw.Start();
                if(singleThread)
                {
                    var rnd = new SunsetquestRandom();
                    for (int j = 0; j < ny; j++)
                    {
                        RenderRow(image, worldBVH, cam, j, nx, ny, ns, rnd, ref totalRayCount);
                    }
                }
                else
                {
                    Parallel.For(0, ny, () => new SunsetquestRandom(), (j, loop, rnd) =>
                    {
                        RenderRow(image, worldBVH, cam, j, nx, ny, ns, rnd, ref totalRayCount);
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
            Console.WriteLine($"Duration: {seconds} | Rate: {mRate} MRays / sec.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void RenderRow(Image<Rgba32> image, ssBVH<IHitable> wl, Camera cam, int j, int nx, int ny, int ns, ImSoRandom rnd, ref uint rayCount)
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
