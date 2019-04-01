using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using raytracinginoneweekend;
using raytracinginoneweekend.Hitables;
using RenderLib;
using SixLabors.ImageSharp;


namespace ServerlessTracing
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static void Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, 
            TraceWriter log)
        {
            string name = req.Query["name"];
            int nx = 300;
            int ny = 300;
            int ns = 50;

            string path = (new System.Uri(Assembly.GetExecutingAssembly().CodeBase)).AbsolutePath;
            path = Path.GetFullPath(path);
            path = Path.GetDirectoryName(path);
            path += @"\..\teapot.obj";

            log.Info($"Obj path: {path}");

            var (world, cam) = Scenes.CornellScene(path, new SunsetquestRandom(), nx, ny);

            var worldBVH = new BVH(world);
            var wl = new IHitable[] { worldBVH };

            var pathTracer = new PathTracer(nx, ny, ns, false);
            uint totalRayCount = 0;
            var sw = Stopwatch.StartNew();
            var image = pathTracer.RenderScene(wl, cam, ref totalRayCount, (pcComplete => log.Info($"{pcComplete}%")));
            sw.Stop();
            //image.Save("test.png");
            float seconds = sw.ElapsedMilliseconds / 1000f;
            float rate = totalRayCount / seconds;
            float mRate = rate / 1_000_000;

            log.Info($"totalRayCount: {totalRayCount}");
            log.Info($"BVH max depth: {worldBVH.MaxTestCount}");
            log.Info($"Duration: {seconds} | Rate: {mRate} MRays / sec.");

            log.Info($"C# Queue trigger function processed: ");
        }
    }
}
