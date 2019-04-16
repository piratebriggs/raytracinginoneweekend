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
using System.Numerics;

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

            var tasks = new Task<(uint totalRayCount, uint sampleCount, TimeSpan render, TimeSpan init, uint maxBvhDepth)>[p.TileCount];
            for (int i = 0; i < p.TileCount; i++)
            {
                tasks[i] = context.CallActivityAsync<(uint, uint, TimeSpan, TimeSpan, uint)>(
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
            TimeSpan minTimespan = new TimeSpan(long.MaxValue);
            TimeSpan maxTimespan = new TimeSpan(0);
            uint maxBvhDepth = 0;

            foreach (var task in tasks)
            {
                totalRayCount += task.Result.totalRayCount;
                renderTimespan += task.Result.render;
                sceneGenTimespan += task.Result.init;
                var functionTimespan = task.Result.init + task.Result.render;
                minTimespan = functionTimespan < minTimespan ? functionTimespan : minTimespan;
                maxTimespan = functionTimespan > maxTimespan ? functionTimespan : maxTimespan;
                maxBvhDepth = task.Result.maxBvhDepth > maxBvhDepth ? task.Result.maxBvhDepth : maxBvhDepth;
            }
            float seconds = (float)(renderTimespan.TotalMilliseconds / 1000.0);
            float rate = totalRayCount / seconds;
            float mRate = rate / 1_000_000;

            log.LogInformation($"totalRayCount: {totalRayCount}");
            log.LogInformation($"BVH max depth: {maxBvhDepth}");
            log.LogInformation($"Min/Max function duration: {minTimespan}/{maxTimespan}");
            log.LogInformation($"Duration: {seconds} | Rate: {mRate} MRays / sec.");
            log.LogInformation($"Scene Generation Duration: {sceneGenTimespan}.");

            return 0;
        }

        [FunctionName("DurableRender_RenderRow")]
        public static (uint, uint, TimeSpan, TimeSpan, uint) RenderRow(
            [ActivityTrigger] DurableActivityContext context, 
            [Blob("rows/{instanceId}/{data.currentTile}.png", FileAccess.Write)] Stream outStream,
            ILogger log)
        {
            var sw = Stopwatch.StartNew();

            var input = context.GetInput<RenderParametersInstance>();

            var timeout = new TimeSpan(0, 0, input.ns);

            // Super temporary way to find local teapot obj
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
            var pathTracer = new PathTracer(input.nx, input.ny, input.ns, true);
            var elapsedInit = sw.Elapsed;

            log.LogInformation($"Tile {input.currentTile} Init Duration: {elapsedInit}");

            var buffer = new Vector4[input.nx * input.ny];
            uint sampleCount = 0;

            uint rayCount = pathTracer.RenderScene(wl, cam,buffer,input.nx,ref sampleCount, newSampleCount => { if(input.doLog) log.LogInformation("Tile: {0} - {1} samples", input.currentTile, newSampleCount); return sw.Elapsed < timeout; }, tileDetails.miny, tileDetails.maxy, tileDetails.minx, tileDetails.maxx);
            sw.Stop();

            PngHelper.SaveImage(outStream, tileDetails.maxx - tileDetails.minx + 1, tileDetails.maxy - tileDetails.miny + 1, buffer, input.nx, sampleCount);

            return (rayCount, sampleCount, sw.Elapsed - elapsedInit, elapsedInit, worldBVH.MaxTestCount);
        }


        [FunctionName("DurableRender_AssembleImage")]
        public static async Task AssembleImage(
            [ActivityTrigger] DurableActivityContext context,
            [Blob("rows/{instanceId}")] CloudBlobDirectory directory,
            [Blob("output/{instanceId}.png", FileAccess.Write)] Stream outputStream,
            ILogger log)
        {
            var input = context.GetInput<RenderParameters>();

            var list = directory.ListBlobsAsync();
            log.LogInformation("Number of blobs: {0}", list.Result.Count);

            var image = new Image<Rgba32>(input.nx, input.ny);

            var tasks = new List<Task>();

            foreach (var item in list.Result)
            {
                if (item.GetType() == typeof(CloudBlockBlob))
                {
                    var blob = (CloudBlockBlob)item;

                    if (!int.TryParse(blob.Name.Split('/')[1].Split('.')[0], out var currentTile))
                    {
                        throw new InvalidDataException("Unable to parse row number: " + blob.Name);
                    }
                    var tileDetails = input.GetTileDetails(currentTile);

                    var ms = new MemoryStream();
                    var downloadTask = blob.DownloadToStreamAsync(ms)
                        .ContinueWith(laa => CopyTile(ms, image, tileDetails.miny, tileDetails.minx, input.ny));
                    tasks.Add(downloadTask);
                }
            }
            Task.WaitAll(tasks.ToArray());

            image.SaveAsPng(outputStream);
        }

        public static void CopyTile(Stream source, Image<Rgba32> destination, int destinationRow, int destinationCol, int ny)
        {
            source.Seek(0, SeekOrigin.Begin);

            var sourceImage = Image.Load(source);

            // remove sampleCount data from A channel
            var imageSpan = sourceImage.GetPixelSpan();
            for (var i = 0; i < 4; i++)
            {
                imageSpan[i].A = 255;
            }


            for (int i = 0; i < sourceImage.Height; i++)
            {
                var sourceRow = sourceImage.GetPixelRowSpan<Rgba32>(sourceImage.Height - 1 - i);

                var rowIndex = ny - 1 - (destinationRow + i);
                var destRow = destination.GetPixelRowSpan(rowIndex);

                for(int j = 0; j<sourceImage.Width; j++)
                {
                    destRow[destinationCol + j] = sourceRow[j];
                }
            }

        }

        /*
         * Sample invokations
         * /api/render/100/100/10/50/true
         * /api/render/300/300/5000/10/true
         * /api/render/1920/1080/5000/10/true
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