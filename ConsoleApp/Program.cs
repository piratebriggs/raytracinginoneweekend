using raytracinginoneweekend.Hitables;
using SixLabors.ImageSharp;
using System;
using System.Diagnostics;
using RenderLib;
using System.IO;

namespace raytracinginoneweekend
{

    public class Program
    {
        public static Stopwatch sw = new Stopwatch();

        static void Main(string[] args)
        {
            int nx = 300;
            int ny = 300;
            int ns = 10;

            var (world, cam) = Scenes.CornellScene("../../../../SampleObj/teapot.obj", new SunsetquestRandom(), nx, ny);

            var worldBVH = new BVH(world);
            var wl = new IHitable[] { worldBVH };
            uint totalRayCount;
            var pathTracer = new PathTracer(nx, ny, ns, false);
            using(var fileStream = new FileStream("test.png", FileMode.Create, FileAccess.Write, FileShare.None)){
                sw.Start();
                totalRayCount = pathTracer.RenderScene(wl, cam, fileStream, (pcComplete => Console.WriteLine($"{pcComplete}%")));
                sw.Stop();
            }
            float seconds = sw.ElapsedMilliseconds / 1000f;
            float rate = totalRayCount / seconds;
            float mRate = rate / 1_000_000;

            Console.WriteLine($"totalRayCount: {totalRayCount}");
            Console.WriteLine($"BVH max depth: {worldBVH.MaxTestCount}");
            Console.WriteLine($"Duration: {seconds} | Rate: {mRate} MRays / sec.");
        }
    }
}
