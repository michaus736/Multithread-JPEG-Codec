using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance;
using Multithread_JPEG_Codec.Models;
using SixLabors.ImageSharp.ColorSpaces;

namespace Multithread_JPEG_Codec.Quantization;

public class QuantizationHandler
{
    //zle podejscie
    public readonly static float MinQuantizationTresholdValue = 0.3f / 255f;
    //https://www.sciencedirect.com/topics/engineering/quantization-table
    public readonly static byte[,] LuminanceQuantizationTable =
    {
        { 16,  11,  10,  16,  24,  40,  51,  61 },
        { 12,  12,  14,  19,  26,  58,  60,  55 },
        { 14,  13,  16,  24,  40,  57,  69,  56 },
        { 14,  17,  22,  29,  51,  87,  80,  62 },
        { 18,  22,  37,  56,  68,  109, 103, 77 },
        { 24,  35,  55,  64,  81,  104, 113, 92 },
        { 49,  64,  78,  87,  103, 121, 120, 101 },
        { 72,  92,  95,  98,  112, 100, 103, 99 }
    };

    public readonly static byte[,] ChrominanceQuantizationTable =
    {
        { 17,  18,  24,  47,  99,  99,  99,  99 },
        { 18,  21,  26,  66,  99,  99,  99,  99 },
        { 24,  26,  56,  99,  99,  99,  99,  99 },
        { 47,  66,  99,  99,  99,  99,  99,  99 },
        { 99,  99,  99,  99,  99,  99,  99,  99 },
        { 99,  99,  99,  99,  99,  99,  99,  99 },
        { 99,  99,  99,  99,  99,  99,  99,  99 },
        { 99,  99,  99,  99,  99,  99,  99,  99 }
    };


    public static void PerformQuantization(Span2D<YCbCrPixel> spanArr)
    {
        for(int i=0;i<spanArr.Height;i++)
        {
            for(int j=0;j<spanArr.Width;j++)
            {
                spanArr[i, j].Y = spanArr[i, j].Y / LuminanceQuantizationTable[i & 8, j & 8];
                spanArr[i, j].Cb = spanArr[i, j].Cb / ChrominanceQuantizationTable[i & 8, j & 8];
                spanArr[i, j].Cr = spanArr[i, j].Cr / ChrominanceQuantizationTable[i & 8, j & 8];

                //check treshold

                if (Math.Abs(spanArr[i, j].Y) < MinQuantizationTresholdValue) spanArr[i, j].Y = 0f;
                if (Math.Abs(spanArr[i, j].Cb) < MinQuantizationTresholdValue) spanArr[i, j].Cb = 0f;
                if (Math.Abs(spanArr[i, j].Cr) < MinQuantizationTresholdValue) spanArr[i, j].Cr = 0f;
            }
        }
        
    }

}
