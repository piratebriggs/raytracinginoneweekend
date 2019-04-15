using raytracinginoneweekend.Hitables;
using System;
using System.Diagnostics;
using RenderLib;
using System.IO;
using System.Threading.Tasks;
using System.Numerics;
using System.Threading;

namespace raytracinginoneweekend
{

    public class Program
    {
        public static Stopwatch sw = new Stopwatch();

        static void Main(string[] args)
        {
            var p = new RenderParameters();
            p.nx = 300;
            p.ny = 300;
            p.ns = 10;
            p.tileSize = 30;

            var (world, cam) = Scenes.CornellScene("../../../../SampleObj/teapot.obj", new SunsetquestRandom(), p.nx, p.ny);

            var worldBVH = new BVH(world);
            var wl = new IHitable[] { worldBVH };
            int totalRayCount = 0;
            var pathTracer = new PathTracer(p.nx, p.ny, p.ns, false);

            var buffer = new Vector4[p.nx * p.ny];
            uint inputSampleCount = 0;

            var ms = new MemoryStream();
            if (File.Exists("test.png"))
            {
                using (var fileStream = new FileStream("test.png", FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    PngHelper.LoadImage(fileStream, buffer, p.nx, ref inputSampleCount);
                }
            }

            using (var fileStream = new FileStream("test.png", FileMode.Create, FileAccess.Write, FileShare.None)){
                sw.Start();

                Parallel.For(0, p.TileCount, currentTile =>
                {
                    var tmpSampleCount = inputSampleCount;
                    var tileDetails = p.GetTileDetails(currentTile);

                    var bufferStart = buffer.AsMemory().Slice(((p.ny - 1) - tileDetails.maxy) * p.nx + tileDetails.minx);

                    var rayCount = pathTracer.RenderScene(wl, cam, bufferStart, p.nx, ref tmpSampleCount, newSampleCount => { Console.WriteLine($"Tile: {currentTile}, {newSampleCount} Samples"); return newSampleCount < inputSampleCount + p.ns; }, tileDetails.miny, tileDetails.maxy, tileDetails.minx, tileDetails.maxx);

                    Interlocked.Add(ref totalRayCount, (int)rayCount);
                });

                sw.Stop();

                PngHelper.SaveImage(fileStream, p.nx, p.ny, buffer, p.nx, (uint)(inputSampleCount + p.ns));
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
