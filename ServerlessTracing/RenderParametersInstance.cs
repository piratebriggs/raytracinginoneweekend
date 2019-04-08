using RenderLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServerlessTracing
{
    public class RenderParametersInstance : RenderParameters
    {
        public int currentTile;
    }

    public static class RenderParametersExtensions
    {
        public static RenderParametersInstance GetInstance(this RenderParameters parameters, int tile)
        {
            return new RenderParametersInstance()
            {
                nx = parameters.nx,
                ny = parameters.ny,
                ns = parameters.ns,
                tileSize = parameters.tileSize,
                doLog = parameters.doLog,
                currentTile = tile
            };
        }
    }

}
