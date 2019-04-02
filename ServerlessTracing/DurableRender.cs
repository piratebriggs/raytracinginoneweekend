using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

using System.Diagnostics;
using System.IO;
using System.Reflection;
using raytracinginoneweekend;
using raytracinginoneweekend.Hitables;
using RenderLib;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Advanced;
using System;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ServerlessTracing
{
    public static class DurableRender
    {
        static int nx = 50;
        static int ny = 50;
        static int ns = 50;

        [FunctionName("DurableRender")]
        public static async Task<int> RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context)
        {
 
            var tasks = new Task<(uint, TimeSpan)>[ny];
            for (int i = 0; i < ny; i++)
            {
                tasks[i] = context.CallActivityAsync<(uint, TimeSpan)>(
                    "DurableRender_RenderRow",
                    i);
            }

            await Task.WhenAll(tasks);

            await context.CallActivityAsync(
                "DurableRender_AssembleImage",
                null
                );


            //long totalBytes = tasks.Sum(t => t.Result);
            return 0;
        }


        [FunctionName("DurableRender_AssembleImage")]
        public static async Task AssembleImage(
            [ActivityTrigger] object ignore,
            [Blob("rows/{instanceId}")] CloudBlobDirectory directory,
            [Blob("output/{instanceId}.png", FileAccess.Write)] Stream outputStream,
            ILogger log)
        {
            var list = directory.ListBlobsAsync();
            log.LogInformation("Number of blobs: %d", list.Result.Count);

            var x = directory.GetBlobReference("1.png");
            log.LogInformation("Blob 1 length: %d", x.Properties.Length);

            var image = new Image<Rgba32>(nx, ny);
            image.SaveAsPng(outputStream);
        }

        [FunctionName("DurableRender_RenderRow")]
        public static (uint,TimeSpan, TimeSpan) RenderRow([ActivityTrigger] int row, 
            [Blob("rows/{instanceId}/{row}.png", FileAccess.Write)] Stream outStream,
            ILogger log)
        {
            // Super temporary way to find local teapot obj
            var swInit = Stopwatch.StartNew();
            string path = (new System.Uri(Assembly.GetExecutingAssembly().CodeBase)).AbsolutePath;
            path = Path.GetFullPath(path);
            path = Path.GetDirectoryName(path);
            path = path.Replace("%20", " ");
            path += @"\..\teapot.obj";
            //log.LogInformation($"Obj path: {path}");

            var (world, cam) = Scenes.CornellScene(path, new SunsetquestRandom(), nx, ny);

            var worldBVH = new BVH(world);
            var wl = new IHitable[] { worldBVH };
            swInit.Stop();
            var pathTracer = new PathTracer(nx, ny, ns, true);
            uint totalRayCount = 0;
            var sw = Stopwatch.StartNew();
            var image = pathTracer.RenderScene(wl, cam, ref totalRayCount, (pcComplete => log.LogInformation($"{pcComplete}%")), row, row);
            sw.Stop();
            image.SaveAsPng(outStream);

            float seconds = sw.ElapsedMilliseconds / 1000f;
            float rate = totalRayCount / seconds;
            float mRate = rate / 1_000_000;

            /*
            log.LogInformation($"totalRayCount: {totalRayCount}");
            log.LogInformation($"BVH max depth: {worldBVH.MaxTestCount}");
            log.LogInformation($"Duration: {seconds} | Rate: {mRate} MRays / sec.");

            log.LogInformation($"C# Queue trigger function processed: ");
            */

            return (totalRayCount, sw.Elapsed, swInit.Elapsed);
        }


        [FunctionName("DurableRender_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]HttpRequestMessage req,
            [OrchestrationClient]DurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("DurableRender", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}