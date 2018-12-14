using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace raytracinginoneweekend
{
    public class Ray
    {
        public Ray(Vector3 origin, Vector3 direction)
        {
            A = origin; B = direction;
        }
        public Vector3 A { get; set; }
        public Vector3 B { get; set; }
        public Vector3 Origin { get { return A; } }
        public Vector3 Direction { get { return B; } }
        public Vector3 PointAtParameter(float t) { return A + t * B; }
    }

    class Program
    {

        static Vector3 Color(Ray r, IList<IHitable> world) {
            var rec = new HitRecord();
            if (world.Hit(r, 0.0f, float.MaxValue, ref rec))
            {
                return 0.5f * new Vector3(rec.Normal.X + 1, rec.Normal.Y + 1, rec.Normal.Z + 1);
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
                Parallel.For(1, ny, index => {
                    var j = ny - index;
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

                        image[i, ny-j] = new Rgba32(col);
                    }
                });
                image.Save("test.png");
            }
        }
    }
}
