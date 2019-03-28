using raytracinginoneweekend;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Concurrent;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace RenderLib
{
    public class PathTracer
    {
        readonly int nx;
        readonly int ny;
        readonly int ns;
        readonly bool singleThread;

        public PathTracer(int nx, int ny, int ns, bool singleThread)
        {
            this.nx = nx;
            this.ny = ny;
            this.ns = ns;
            this.singleThread = singleThread;
        }

        static Vector3 Color(Ray r, IHitable[] world, int depth, ImSoRandom rnd, ref uint rayCount)
        {
            rayCount++;

            var rec = new HitRecord();
            if (world.Hit(r, 0.001f, float.MaxValue, ref rec))
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


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void RenderRow(Image<Rgba32> image, IHitable[] wl, Camera cam, int j, int nx, int ny, int ns, ImSoRandom rnd, ConcurrentDictionary<int, int> processedRows, ref uint rayCount)
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
            processedRows.TryAdd(j, Thread.CurrentThread.ManagedThreadId);
        }

        public Image<Rgba32> RenderScene(IHitable[] world, Camera cam, ref uint totalRayCount, Action<float> progressCallback)
        {
            var processedRows = new ConcurrentDictionary<int, int>();

            uint tmpTotalRayCount = 0;

            Image<Rgba32> image = new Image<Rgba32>(nx, ny);

            if (singleThread)
            {
                var rnd = new SunsetquestRandom();
                for (int j = 0; j < ny; j++)
                {
                    RenderRow(image, world, cam, j, nx, ny, ns, rnd, processedRows, ref tmpTotalRayCount);
                    progressCallback(processedRows.Count / (float)ny * 100f);
                }
            }
            else
            {
                Parallel.For(0, ny, () => new SunsetquestRandom(), (j, loop, rnd) =>
                {
                    RenderRow(image, world, cam, j, nx, ny, ns, rnd, processedRows, ref tmpTotalRayCount);
                    progressCallback(processedRows.Count / (float)ny * 100f);

                    return rnd;
                }, (rnd) => { });
            }
            totalRayCount = tmpTotalRayCount;
            return image;
        }
    }
}

