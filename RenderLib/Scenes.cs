using ObjLoader.Loader.Loaders;
using raytracinginoneweekend;
using raytracinginoneweekend.Hitables;
using raytracinginoneweekend.Materials;
using raytracinginoneweekend.Textures;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace RenderLib
{
    public class Scenes

    {

        public static (List<IHitable>, Camera) RandomScene(ImSoRandom rnd, int nx, int ny)
        {
            var world = new List<IHitable>();

            var texture = new CheckerTexture(new ConstantTexture(0.2f, 0.3f, 0.1f), new ConstantTexture(0.9f, 0.9f, 0.9f));
            world.Add(new Sphere(new Vector3(0, -1000f, 0), 1000, new Lambertian(texture)));

            for (int a = -11; a < 11; a++)
            {
                for (int b = -11; b < 11; b++)
                {
                    float choose_mat = rnd.NextFloat();
                    var center = new Vector3(a + 0.9f * rnd.NextFloat(), 0.2f, b + 0.9f * rnd.NextFloat());
                    if ((center - new Vector3(4, 0.2f, 0)).Length() > 0.9)
                    {
                        if (choose_mat < 0.8)
                        {  // diffuse
                            world.Add(new MovingSphere(center, center + new Vector3(0, 0.5f * rnd.NextFloat(), 0), 0.0f, 1.0f, 0.2f, new Lambertian(new ConstantTexture(rnd.NextFloat() * rnd.NextFloat(), rnd.NextFloat() * rnd.NextFloat(), rnd.NextFloat() * rnd.NextFloat()))));
                            //world.Add(new Sphere(center, 0.2f, new Lambertian(new Vector3(rnd.NextFloat() * rnd.NextFloat(), rnd.NextFloat() * rnd.NextFloat(), rnd.NextFloat() * rnd.NextFloat()))));
                        }
                        else if (choose_mat < 0.95)
                        { // metal
                            world.Add(new Sphere(center, 0.2f,
                            new Metal(new Vector3(0.5f * (1 + rnd.NextFloat()), 0.5f * (1 + rnd.NextFloat()), 0.5f * (1 + rnd.NextFloat())), 0.5f * rnd.NextFloat())));
                        }
                        else
                        {  // glass
                            world.Add(new Sphere(center, 0.2f, new Dialectric(1.5f)));
                        }
                    }
                }
            }

            world.Add(new Sphere(new Vector3(0, 1, 0), 1.0f, new Dialectric(1.5f)));
            world.Add(new Sphere(new Vector3(-4, 1, 0), 1.0f, new Lambertian(new ConstantTexture(0.4f, 0.2f, 0.1f))));
            world.Add(new Sphere(new Vector3(4, 1, 0), 1.0f, new Metal(new Vector3(0.7f, 0.6f, 0.5f), 0f)));

            world.Add(new Sphere(new Vector3(0, 0, 0), 20.0f, new DiffuseLight(new ConstantTexture(new Vector3(1, 1, 1)))));


            var lookFrom = new Vector3(13, 2, 3);
            var lookAt = new Vector3(0, 0, 0);
            var distToFocus = 10;
            var aperture = 0.1f;

            var cam = new Camera(lookFrom, lookAt, new Vector3(0, 1, 0), 20, (float)nx / (float)ny, aperture, distToFocus, 0.0f, 1.0f);

            return (world, cam);
        }

        public static (List<IHitable>, Camera) PoolScene(ImSoRandom rnd, int nx, int ny)
        {
            var world = new List<IHitable>();
            world.Add(new Sphere(new Vector3(0, -1000f, 0), 1000, new Lambertian(new ConstantTexture(0.33f, 0.67f, 0.0f))));
            var red = new ConstantTexture(0.68f, 0.13f, 0.16f);
            var yellow = new ConstantTexture(1f, 0.74f, 0.13f);
            var black = new ConstantTexture(0.14f, 0.07f, 0.07f);
            var white = new ConstantTexture(1f, 1f, 0.9f);

            for (var a = 1f; a <= 5; a += 1f)
            {
                var counter = 0 - a;
                for (var b = 0f; b < a; b += 1f)
                {
                    var center = new Vector3(a * 0.9f - 5f, 0.5f, a / 2f - b - 0.5f);
                    var colour = counter % 2 == 0 ? red : yellow;
                    if (a == 3 && b == 1)
                    {
                        colour = black;
                    }
                    if (a == 3 && b == 2)
                    {
                        colour = red;
                    }
                    if (a == 5 && b == 4)
                    {
                        colour = red;
                    }
                    world.Add(new Sphere(center, 0.45f,
                        new Lambertian(colour)));
                    world.Add(new Sphere(center, 0.5f,
                        new Dialectric(1.5f)));
                    counter++;
                }
            }

            var cueCenter = new Vector3(-6, 0.5f, 0);
            var cueCenter1 = cueCenter + new Vector3(0.75f * rnd.NextFloat(), 0, 0);
            world.Add(new MovingSphere(cueCenter, cueCenter1, 0.0f, 1.0f, 0.5f,
                new Dialectric(1.5f)));
            world.Add(new MovingSphere(cueCenter, cueCenter1, 0.0f, 1.0f, 0.45f,
                new Lambertian(white)));

            world.Add(new RectXZ(-3, 3, -2, 2, 5, new DiffuseLight(new ConstantTexture(new Vector3(15, 15, 15)))));

            var lookFrom = new Vector3(-9, 3.5f, 12);
            var lookAt = new Vector3(-3, 0, 0);
            var distToFocus = (lookFrom - new Vector3(-6, 0.5f, 0)).Length();
            var aperture = 0.5f;

            var cam = new Camera(lookFrom, lookAt, new Vector3(0, 1, 0), 20, (float)nx / (float)ny, aperture, distToFocus, 0.0f, 1.0f);

            return (world, cam);
        }

        public static (List<IHitable>, Camera) CornellScene(string objPath, ImSoRandom rnd, int nx, int ny)
        {
            var world = new List<IHitable>();

            var grad = new Lambertian(new ColourTexture());
            var blue = new Lambertian(new ConstantTexture(0.05f, 0.05f, 0.65f));
            var red = new Lambertian(new ConstantTexture(0.65f, 0.05f, 0.05f));
            var white = new Lambertian(new ConstantTexture(0.73f, 0.73f, 0.73f));
            var green = new Lambertian(new ConstantTexture(0.12f, 0.45f, 0.15f));
            var light = new DiffuseLight(new ConstantTexture(15f, 15f, 15f));
            var glass = new Dialectric(1.5f);

            world.Add(new RectXZ(213, 343, 227, 332, 554, light));

            world.Add(new FlipNormals(new RectYZ(0, 555, 0, 555, 555, green)));
            world.Add(new RectYZ(0, 555, 0, 555, 0, red));
            world.Add(new FlipNormals(new RectXZ(0, 555, 0, 555, 555, white)));
            world.Add(new RectXZ(0, 555, 0, 555, 0, white));
            world.Add(new FlipNormals(new RectXY(0, 555, 0, 555, 555, white)));

            //world.Add( new Translate( new Box(Vector3.Zero, new Vector3(165, 165, 165), white), new Vector3(130, 0, 65)));
            //world.Add( new Translate( new Box(Vector3.Zero, new Vector3(165, 330, 165), white), new Vector3(265, 0, 295)));

            //world.Add(new Sphere(new Vector3(0, 0, 0), 3000.0f, new DiffuseLight(new ConstantTexture(new Vector3(1, 1, 1)))));
            /*
            for (int i = 0; i < 50; i++)
            {
                var a = rnd.RandomVector();
                var b = rnd.RandomVector();
                var c = rnd.RandomVector();

                //world.Add(new Translate(new Triangle(new Vector3(rnd.NextFloat() * 555f, rnd.NextFloat() * 555f, rnd.NextFloat() * 555f), new Vector3(rnd.NextFloat() * 555f, rnd.NextFloat() * 555f, rnd.NextFloat() * 555f), new Vector3(rnd.NextFloat() * 555f, rnd.NextFloat() * 555f, rnd.NextFloat() * 555f), red),new Vector3(10 + rnd.NextFloat() * 100f, 10 + rnd.NextFloat() * 100f, 10 + rnd.NextFloat() * 100f)));
                world.Add(new Triangle(a, b, c, blue));
            }
            
    */
            //world.Add(new Triangle( new Vector3(340, 90, 62),new Vector3(100, 320, 190), new Vector3(262, 331, 400), grad));

            var objLoaderFactory = new ObjLoaderFactory();
            var objLoader = objLoaderFactory.Create();
            var fileStream = new FileStream(objPath, FileMode.Open, FileAccess.Read, FileShare.Read );
            var obj = objLoader.Load(fileStream);
            var objBox = obj.GetBoundingBox();
            var centerOffset = objBox.Center();
            var height = objBox.Max.Y - objBox.Min.Y;

            var scaleFactor = new Vector3(50, 50, 50); // scale obj to wold space
            var displacement = new Vector3(555f / 2, height / 2f * scaleFactor.Y, 555f / 3 * 2);
            var glossScaleFactor = new Vector3(0.5f, 0.5f, 0.5f);   // scale the unit vector normal
            foreach (var g in obj.Groups)
            {
                foreach (var f in g.Faces)
                {
                    // Verticies have their origin set to the center of the bounding box
                    var v0 = obj.Vertices[f[0].VertexIndex - 1].ToVector3() - centerOffset;
                    var v1 = obj.Vertices[f[1].VertexIndex - 1].ToVector3() - centerOffset;
                    var v2 = obj.Vertices[f[2].VertexIndex - 1].ToVector3() - centerOffset;

                    var t = new Triangle(v0 * scaleFactor, v1 * scaleFactor, v2 * scaleFactor, glass);

                    // add Dialectric
                    world.Add(new Translate(t, displacement));

                    // add Lambertian. translated toward obj center by glossScaleFactor of a unit vector
                    world.Add(new Translate(new Triangle(v0 * scaleFactor, v1 * scaleFactor, v2 * scaleFactor, white), displacement - Vector3.Normalize(t.Normal)));
                }
            }

            var lookFrom = new Vector3(278, 278, -800);
            var lookAt = new Vector3(278, 278, 0);
            var distToFocus = (lookFrom - lookAt).Length() - scaleFactor.Length() / 2;
            var aperture = 0.1f;
            var vFov = 40;

            var cam = new Camera(lookFrom, lookAt, new Vector3(0, 1, 0), vFov, (float)nx / (float)ny, aperture, distToFocus, 0.0f, 1.0f);

            return (world, cam);
        }


    }
}
