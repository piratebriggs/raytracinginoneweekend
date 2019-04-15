using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace RenderLib
{
    public static class PngHelper

    {
        public static void LoadImage(Stream stream, Memory<Vector4> buffer, int bufferStepSize, ref uint sampleCount)
        {
            Span<byte> byteSpan = BitConverter.GetBytes(sampleCount);
            Span<uint> intSpan = MemoryMarshal.Cast<byte, uint>(byteSpan);

            // TODO: Verify the image has a sampleCount in alpha channel?
            var sourceImage = Image.Load(stream);
            var sourceBuffer = sourceImage.GetPixelSpan();
            for (var i = 0; i < 4; i++)
            {
                byteSpan[i] = sourceBuffer[i].A;
            }
            sampleCount = intSpan[0];

            for (var j = 0; j < sourceImage.Height; j++)
            {
                var bufferSpan = buffer.Slice(j * bufferStepSize, sourceImage.Width).Span;
                for (var i = 0; i < sourceImage.Width; i++)
                {
                    bufferSpan[i] = sourceBuffer[j * bufferStepSize + i].ToVector4();
                }
            }
        }

        public static void SaveImage(Stream stream, int numCols, int numRows, Memory<Vector4> buffer, int bufferStepSize, uint sampleCount)
        {
            Span<byte> byteSpan = BitConverter.GetBytes(sampleCount);
            Span<uint> intSpan = MemoryMarshal.Cast<byte, uint>(byteSpan);


            var image = new Image<Rgba32>(numCols, numRows);
            for (int i = 0; i < numRows; i++)
            {
                var sourceRowSpan = buffer.Slice(i * bufferStepSize).Span;
                var rowSpan = image.GetPixelRowSpan<Rgba32>(i);

                for (var j = 0; j < numCols; j++)
                {
                    var col = sourceRowSpan[j];

                    var col2 = new Vector3((float)Math.Sqrt(col.X), (float)Math.Sqrt(col.Y), (float)Math.Sqrt(col.Z));

                    rowSpan[j] = new Rgba32(col2);
                }
            }
            var imageSpan = image.GetPixelSpan();
            intSpan[0] = sampleCount;
            for (var i = 0; i < 4; i++)
            {
                imageSpan[i].A = byteSpan[i];
            }

            image.SaveAsPng(stream);
        }
    }
}
