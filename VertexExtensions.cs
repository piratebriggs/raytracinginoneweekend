using ObjLoader.Loader.Data.VertexData;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace raytracinginoneweekend
{
    public static class VertexExtensions
    {
        public static Vector3 ToVector3(this Vertex v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }

        public static Vertex ConvertRightHandedToLeftHandedVertex(this Vertex v)
        {
            return new Vertex(v.X, v.Y, -v.Z);
        }
    }
}
