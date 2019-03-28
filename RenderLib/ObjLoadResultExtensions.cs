using ObjLoader.Loader.Loaders;
using raytracinginoneweekend.Hitables;
using SimpleScene;
using System;
using System.Collections.Generic;
using System.Text;

namespace raytracinginoneweekend
{
    public static class ObjLoadResultExtensions
    {
        public static SSAABB GetBoundingBox(this LoadResult obj )
        {
            var bbox = new SSAABB();
            foreach (var g in obj.Groups)
            {
                foreach (var f in g.Faces)
                {
                    if (f.Count != 3) throw new NotImplementedException("Non triangular face found in obj");

                    var v0 = obj.Vertices[f[0].VertexIndex - 1].ToVector3();
                    var v1 = obj.Vertices[f[1].VertexIndex - 1].ToVector3();
                    var v2 = obj.Vertices[f[2].VertexIndex - 1].ToVector3();

                    var t = new Triangle(v0, v1, v2, null);
                    bbox.ExpandToFit(t.BoundingBox);
                }
            }
            return bbox;
        }
    }
}
