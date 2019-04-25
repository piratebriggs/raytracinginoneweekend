using raytracinginoneweekend;
using System;
using System.Collections.Concurrent;
using System.Numerics;
using System.Runtime.CompilerServices;

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
        private static void RenderRow(Span<Vector4> rowSpan, IHitable[] wl, Camera cam, int j, int iMin, int iMax,int nx, int ny, int ns, ImSoRandom rnd, ConcurrentDictionary<int, int> processedRows, ref uint rayCount)
        {
        }

        /*
                            var pcComplete = (int)((float)sw.ElapsedMilliseconds / (float)timeout.TotalMilliseconds * 100f);
                if (pcComplete != pcCompleteLast)
                {
                    pcCompleteLast = pcComplete;
                    progressCallback(pcComplete);
                }



         */

        /// <summary>
        /// Overload for use in framework code that doesn't have access to Memory<>
        /// </summary>
        /// <param name="world"></param>
        /// <param name="cam"></param>
        /// <param name="buffer"></param>
        /// <param name="bufferStepSize"></param>
        /// <param name="sampleCount"></param>
        /// <param name="progressCallback"></param>
        /// <param name="startRow"></param>
        /// <param name="endRow"></param>
        /// <param name="startCol"></param>
        /// <param name="endCol"></param>
        /// <returns></returns>
        public uint RenderScene(IHitable[] world, Camera cam, Vector4[] buffer, int bufferStepSize, ref uint sampleCount, Func<uint, bool> progressCallback, int startRow = 0, int? endRow = null, int startCol = 0, int? endCol = null)
        {
            if (!endRow.HasValue)
            {
                endRow = ny - 1;
            }
            if (!endCol.HasValue)
            {
                endCol = nx - 1;
            }

            var bufferStart = buffer.AsMemory().Slice(((ny - 1) - endRow.Value) * nx + startCol);

            return RenderScene(world, cam, bufferStart, bufferStepSize, ref sampleCount, progressCallback, startRow, endRow, startCol, endCol);
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
        public uint RenderScene(IHitable[] world, Camera cam, Memory<Vector4> buffer, int bufferStepSize, ref uint sampleCount, Func<uint, bool> progressCallback, int startRow = 0, int? endRow = null, int startCol = 0, int? endCol = null)
        {
            var timeout = TimeSpan.FromSeconds(ns);

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

            var rnd = new SunsetquestRandom();
            do
            {
                sampleCount += 1;
                for (int j = startRow; j <= endRow; j++)
                {
                    var index = numRows - 1 - (j - startRow);
                    var rowSpan = buffer.Slice(index * bufferStepSize, numCols).Span;

                    for (int i = startCol; i <= endCol.Value; i++)
                    {
                        float u = ((float)i + rnd.NextFloat()) / (float)nx;
                        float v = ((float)j + rnd.NextFloat()) / (float)ny;
                        var r = cam.GetRay(u, v, rnd);
                        var col = new Vector4(Color(r, world, 0, rnd, ref tmpTotalRayCount), 1);

                        rowSpan[i - startCol] += (col - rowSpan[i - startCol]) / sampleCount;
                    }
                }
            } while (progressCallback(sampleCount));

            return tmpTotalRayCount;
        }
    }
}

