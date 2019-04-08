using raytracinginoneweekend.Hitables;
using SixLabors.ImageSharp;
using System;
using System.Diagnostics;
using RenderLib;

namespace raytracinginoneweekend
{

    public class Program
    {
        public static Stopwatch sw = new Stopwatch();

        static void Main(string[] args)
        {
            int nx = 300;
            int ny = 300;
            int ns = 50;

            var (world, cam) = Scenes.CornellScene("../../../../SampleObj/teapot.obj", new SunsetquestRandom(), nx, ny);

            var worldBVH = new BVH(world);
            var wl = new IHitable[] { worldBVH };

            var pathTracer = new PathTracer(nx, ny, ns, false);
            uint totalRayCount=0;
            sw.Start();
            // TODO:  create FileStream
            //var image = pathTracer.RenderScene(wl, cam,  ref  totalRayCount, (pcComplete => Console.WriteLine($"{pcComplete}%")));
            sw.Stop();
            //image.Save("test.png");
            float seconds = sw.ElapsedMilliseconds / 1000f;
            float rate = totalRayCount / seconds;
            float mRate = rate / 1_000_000;

            Console.WriteLine($"totalRayCount: {totalRayCount}");
            Console.WriteLine($"BVH max depth: {worldBVH.MaxTestCount}");
            Console.WriteLine($"Duration: {seconds} | Rate: {mRate} MRays / sec.");
        }

    }
}
