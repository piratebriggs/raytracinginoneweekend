﻿using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace raytracinginoneweekend
{

    class Program
    {
        static CryptoRandom rnd = new CryptoRandom();

        static Vector3 RandomInUnitSphere()
        {
            var p = new Vector3();
            do
            {
                p = 2.0f * new Vector3(rnd.NextFloat(), rnd.NextFloat(), rnd.NextFloat()) - new Vector3(1,1,1);
            } while (p.LengthSquared() >= 1.0f);
            return p;
        }

        static Vector3 Color(Ray r, IList<IHitable> world) {
            var rec = new HitRecord();
            if (world.Hit(r, 0.001f, float.MaxValue, ref rec))
            {
                Vector3 target = rec.P + rec.Normal + RandomInUnitSphere();
                return 0.5f * Color(new Ray(rec.P, target - rec.P), world);
            }
            var unit_direction = Vector3.Normalize(r.Direction);
            var t = 0.5f * (unit_direction.Y + 1.0f);
            return (1.0f - t) * new Vector3(1.0f, 1.0f, 1.0f) + t * new Vector3(0.5f, 0.7f, 1.0f);
        }

        static void Main(string[] args)
        {
            var x = new Vector3(0, 0, 0);
            int nx = 200;
            int ny = 100;
            
            var world = new List<IHitable>();
            world.Add(new Sphere(new Vector3(0, 0, -1), 0.5f));
            world.Add(new Sphere(new Vector3(0, -100.5f, -1), 100));

            var cam = new Camera();
            var rnd = new Random(123);

            var antiAlias = new List<Tuple<float, float>>() {
                { new Tuple<float, float>(0.25f, 0.25f) },
                { new Tuple<float, float>(0.25f, 0.75f) },
                { new Tuple<float, float>(0.75f, 0.25f) },
                { new Tuple<float, float>(0.75f, 0.75f) },
                { new Tuple<float, float>(0.5f, 0.5f) }
            };

            using (Image<Rgba32> image = new Image<Rgba32>(nx, ny))
            {
                Parallel.For(0, ny, index => {
                    var j = ny - 1 - index;
                    var rowSpan = image.GetPixelRowSpan(index);

                    for (int i = 0; i < nx; i++)
                    {
                        var col = new Vector3(0);

                        foreach(var point in antiAlias)
                        {
                            float u = ((float)i + point.Item1) / (float)nx;
                            float v = ((float)j + point.Item2) / (float)ny;
                            var r = cam.GetRay(u, v);
                            col += Color(r, world);
                        }
                        col /= (float)antiAlias.Count;
                        col = new Vector3((float)Math.Sqrt(col.X), (float)Math.Sqrt(col.Y), (float)Math.Sqrt(col.Z));

                        rowSpan[i] = new Rgba32(col);
                    }
                });
                image.Save("test.png");
            }
        }
    }
}
