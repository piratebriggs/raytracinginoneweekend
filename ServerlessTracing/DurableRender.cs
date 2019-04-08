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
using System.Linq;

namespace ServerlessTracing
{
    public static class DurableRender
    {
        [FunctionName("DurableRender")]
        public static async Task<int> RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context,
            ILogger log)
        {

            var p = context.GetInput<RenderParameters>();

            var tasks = new Task<(uint, TimeSpan, TimeSpan)>[p.TileCount];
            for (int i = 0; i < p.TileCount; i++)
            {
                tasks[i] = context.CallActivityAsync<(uint, TimeSpan, TimeSpan)>(
                    "DurableRender_RenderRow",
                    p.GetInstance(i) );
            }

            await Task.WhenAll(tasks);

            await context.CallActivityAsync(
                "DurableRender_AssembleImage",
                p
                );

            uint totalRayCount= 0;
            TimeSpan renderTimespan = new TimeSpan();
            TimeSpan sceneGenTimespan = new TimeSpan();
            foreach(var task in tasks)
            {
                totalRayCount += task.Result.Item1;
                renderTimespan += task.Result.Item2;
                sceneGenTimespan += task.Result.Item3;
            }
            float seconds = renderTimespan.Milliseconds / 1000f;
            float rate = totalRayCount / seconds;
            float mRate = rate / 1_000_000;

            log.LogInformation($"totalRayCount: {totalRayCount}");
            //log.LogInformation($"BVH max depth: {worldBVH.MaxTestCount}");
            log.LogInformation($"Duration: {seconds} | Rate: {mRate} MRays / sec.");
            log.LogInformation($"Scene Generation Duration: {sceneGenTimespan.Milliseconds}ms.");

            return 0;
        }

        [FunctionName("DurableRender_RenderRow")]
        public static (uint,TimeSpan, TimeSpan) RenderRow(
            [ActivityTrigger] DurableActivityContext context, 
            [Blob("rows/{instanceId}/{data.currentTile}.png", FileAccess.Write)] Stream outStream,
            ILogger log)
        {
            var input = context.GetInput<RenderParametersInstance>();

            // Super temporary way to find local teapot obj
            var swInit = Stopwatch.StartNew();
            string path = (new System.Uri(Assembly.GetExecutingAssembly().CodeBase)).AbsolutePath;
            path = Path.GetFullPath(path);
            path = Path.GetDirectoryName(path);
            path = path.Replace("%20", " ");
            path += @"\..\teapot.obj";
            //log.LogInformation($"Obj path: {path}");

            var (world, cam) = Scenes.CornellScene(path, new SunsetquestRandom(), input.nx, input.ny);

            var tileDetails = input.GetTileDetails(input.currentTile);

            var worldBVH = new BVH(world);
            var wl = new IHitable[] { worldBVH };
            swInit.Stop();
            var pathTracer = new PathTracer(input.nx, input.ny, input.ns, true);
            var sw = Stopwatch.StartNew();
            var totalRayCount = pathTracer.RenderScene(wl, cam, outStream, (pcComplete => log.LogInformation($"{pcComplete}%")), tileDetails.miny, tileDetails.maxy, tileDetails.minx, tileDetails.maxx);
            sw.Stop();


            /*
            float seconds = sw.ElapsedMilliseconds / 1000f;
            float rate = totalRayCount / seconds;
            float mRate = rate / 1_000_000;

            log.LogInformation($"totalRayCount: {totalRayCount}");
            log.LogInformation($"BVH max depth: {worldBVH.MaxTestCount}");
            log.LogInformation($"Duration: {seconds} | Rate: {mRate} MRays / sec.");

            log.LogInformation($"C# Queue trigger function processed: ");
            */

            return (totalRayCount, sw.Elapsed, swInit.Elapsed);
        }

        // TODO: update to tiles
        [FunctionName("DurableRender_AssembleImage")]
        public static async Task AssembleImage(
    [ActivityTrigger] DurableActivityContext context,
    [Blob("rows/{instanceId}")] CloudBlobDirectory directory,
    [Blob("output/{instanceId}.png", FileAccess.Write)] Stream outputStream,
    ILogger log)
        {
            var input = context.GetInput<(RenderParameters p, int currentTile)>();

            var list = directory.ListBlobsAsync();
            log.LogInformation("Number of blobs: {0}", list.Result.Count);

            var image = new Image<Rgba32>(input.p.nx, input.p.ny);

            var tasks = new List<Task>();

            foreach (var item in list.Result)
            {
                if (item.GetType() == typeof(CloudBlockBlob))
                {
                    var blob = (CloudBlockBlob)item;

                    if (!int.TryParse(blob.Name.Split('/')[1].Split('.')[0], out var rowNumber))
                    {
                        throw new InvalidDataException("Unable to parse row number: " + blob.Name);
                    }

                    var ms = new MemoryStream();
                    var downloadTask = blob.DownloadToStreamAsync(ms)
                        .ContinueWith(laa => CopyRow(ms, image, rowNumber, input.p.ny));
                    tasks.Add(downloadTask);
                }
            }
            Task.WaitAll(tasks.ToArray());

            image.SaveAsPng(outputStream);
        }

        public static void CopyRow(Stream source, Image<Rgba32> destination, int destinationRow, int ny)
        {
            source.Seek(0, SeekOrigin.Begin);

            var rowImage = Image.Load(source);
            var sourceRow = rowImage.GetPixelRowSpan<Rgba32>(0);

            var rowIndex = ny - 1 - destinationRow;
            var destRow = destination.GetPixelRowSpan(rowIndex);

            sourceRow.CopyTo(destRow);
        }

        /*
         * /render/100/100/50/10/true
         */

        [FunctionName("DurableRender_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "render/{nx:int}/{ny:int}/{ns:}/{tileSize:int}/{doLog:bool}")]HttpRequestMessage req,
            [OrchestrationClient]DurableOrchestrationClient starter,
            ILogger log,
            int nx, int ny, int ns, int tileSize, bool dolog)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("DurableRender", new RenderParameters()
            {
                nx = nx,
                ny = ny,
                ns = ns,
                tileSize = tileSize,
                doLog = dolog
            });

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }

        [FunctionName("DurableRender_TerminateAll")]
        public static async Task TerminateAll(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]HttpRequestMessage req,
            [OrchestrationClient] DurableOrchestrationClient client,
            ILogger log)
        {
            IList<DurableOrchestrationStatus> instances = await client.GetStatusAsync(); // You can pass CancellationToken as a parameter.
            foreach (var instance in instances)
            {
                await client.TerminateAsync(instance.InstanceId, "manual");
            };
        }

    }
}