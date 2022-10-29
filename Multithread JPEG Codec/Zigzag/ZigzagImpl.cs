using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance;
using Multithread_JPEG_Codec.Models;

namespace Multithread_JPEG_Codec.Zigzag;


public class ZigzagImpl
{
    public static readonly int[] supportedChunkSize = new int[] { 8 };
    private static readonly uint[,] zigzagDirection8 = new uint[,] { 
        {  0,  1,  5,  6, 14, 15, 27, 28 },
        {  2,  4,  7, 13, 16, 26, 29, 42 },
        {  3,  8, 12, 17, 25, 30, 41, 43 },
        {  9, 11, 18, 24, 31, 40, 44, 53 },
        { 10, 19, 23, 32, 39, 45, 52, 54 },
        { 20, 22, 33, 38, 46, 51, 55, 60 },
        { 21, 34, 37, 47, 50, 56, 59, 61 },
        { 35, 36, 48, 49, 57, 58, 62, 63 }
    };

    public static YCbCrPixel[] PerformZigZag(Span2D<YCbCrPixel> pixels, int chunkSize = 8)
    {
        if(!supportedChunkSize.Contains(chunkSize))
        {
            throw new NotSupportedException($"chunk size for zigzag is not supported, chunksize: {chunkSize}, supported: {String.Join(", ", supportedChunkSize)}");
        }
        uint index = 0;
        int chunksq = chunkSize * chunkSize;
        int rowChunkNum = pixels.Height / chunkSize;
        int colChunkNum = pixels.Width / chunkSize;

        YCbCrPixel[] zigzagRes = new YCbCrPixel[pixels.Length];

        for (int i = 0; i < rowChunkNum; i++) 
        {
            for(int j = 0; j < colChunkNum; j++)
            {
                //getting pixels to 1 dimentional array from individual chunks
                for(int k = 0; k < chunkSize; k++)
                {
                    for(int l = 0; l < chunkSize; l++)
                    {
                        zigzagRes[index++]= pixels[k + i * chunkSize, l + j * chunkSize];
                    }
                }
            }
        }
        



        return zigzagRes;
    }

}
