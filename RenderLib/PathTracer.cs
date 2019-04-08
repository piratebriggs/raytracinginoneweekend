using raytracinginoneweekend;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Concurrent;
using System.IO;
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
        private static void RenderRow(Span<Rgba32> rowSpan, IHitable[] wl, Camera cam, int j, int iMin, int iMax,int nx, int ny, int ns, ImSoRandom rnd, ConcurrentDictionary<int, int> processedRows, ref uint rayCount)
        {
            for (int i = iMin; i <= iMax; i++)
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

                rowSpan[i - iMin] = new Rgba32(col);
            }
            processedRows.TryAdd(j, Thread.CurrentThread.ManagedThreadId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="world"></param>
        /// <param name="cam"></param>
        /// <param name="totalRayCount"></param>
        /// <param name="progressCallback"></param>
        /// <param name="startRow">0-indexed row number</param>
        /// <param name="endRow">0-indexed row number</param>
        /// <returns></returns>
        public uint RenderScene(IHitable[] world, Camera cam, Stream outstream, Action<float> progressCallback, int startRow = 0, int? endRow = null, int startCol = 0, int? endCol = null)
        {
            var processedRows = new ConcurrentDictionary<int, int>();

            uint tmpTotalRayCount = 0;

            if (!endRow.HasValue)
            {
                endRow = ny - 1;
            }
            if (!endCol.HasValue)
            {
                endCol = nx - 1;
            }

            var numRows = endRow.Value - startRow + 1;
            var numCols = endCol.Value - startCol + 1;

            Image<Rgba32> image = new Image<Rgba32>(numCols, numRows);

            if (singleThread)
            {
                var rnd = new SunsetquestRandom();
                for (int j = startRow; j <= endRow; j++)
                {
                    var index = numRows - 1 - (j - startRow);
                    var rowSpan = image.GetPixelRowSpan(index);

                    RenderRow(rowSpan, world, cam, j, startCol, endCol.Value, nx, ny, ns, rnd, processedRows, ref tmpTotalRayCount);
                    progressCallback(processedRows.Count / (float)numRows * 100f);
                }
            }
            else
            {
                Parallel.For(startRow, endRow.Value, () => new SunsetquestRandom(), (j, loop, rnd) =>
                {
                    var index = numRows - 1 - (j - startRow);
                    var rowSpan = image.GetPixelRowSpan(index);

                    RenderRow(rowSpan, world, cam, j, startCol, endCol.Value, nx, ny, ns, rnd, processedRows, ref tmpTotalRayCount);
                    progressCallback(processedRows.Count / (float)numRows * 100f);

                    return rnd;
                }, (rnd) => { });
            }
            image.SaveAsPng(outstream);
            return tmpTotalRayCount;
        }
    }
}

