using System;
using System.Collections.Generic;
using System.Text;

namespace RenderLib
{
    public class RenderParameters
    {
        public int nx;
        public int ny;
        public int ns;
        public int tileSize;
        public bool doLog;

        public int HorizTileCount => (int)Math.Ceiling((double)nx / tileSize);
        public int VertTileCount => (int)Math.Ceiling((double)ny / tileSize);

        public int TileCount => HorizTileCount * VertTileCount;

        public (int minx, int miny, int maxx, int maxy) GetTileDetails(int tile)
        {
            var miny = (int)Math.Floor((double)tile / HorizTileCount) * tileSize;
            var minx = (tile % HorizTileCount)  * tileSize;
            var maxy = miny + tileSize > ny ? ny-1 : miny + tileSize - 1;
            var maxx = minx + tileSize > nx ? nx-1 : minx + tileSize - 1;
            return (minx, miny, maxx, maxy);
        }
    }
}
