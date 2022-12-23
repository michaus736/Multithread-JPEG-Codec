﻿using System;
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
        uint chunksq = (uint)(chunkSize * chunkSize);
        int rowChunkNum = pixels.Height / chunkSize;
        int colChunkNum = pixels.Width / chunkSize;

        YCbCrPixel[] zigzagRes = new YCbCrPixel[pixels.Length];

        for (int i = 0; i < rowChunkNum; i++) 
        {
            for(int j = 0; j < colChunkNum; j++)
            {
                var getChunkTextSlice = pixels.Slice(i, j, chunkSize, chunkSize);
                //getting pixels to 1 dimentional array from individual chunks
                for(int k = 0; k < chunkSize; k++)
                {
                    for(int l = 0; l < chunkSize; l++)
                    {
                        ref YCbCrPixel pixel = ref getChunkTextSlice[k,l];
                        zigzagRes[index + zigzagDirection8[k, l]] = pixel;
                        
                    }
                }

                index += chunksq;
            }
        }

        return zigzagRes;
    }

    public static YCbCrPixel[,] PreformInverseZigzag(Span<YCbCrPixel> zigzagSpan, int height, int width, int chunkSize = 8)
    {
        //validate
        if (zigzagSpan.IsEmpty) throw new Exception("zigzag array is null");
        if (zigzagSpan.Length != height * width) throw new Exception("zigzag length isn't equal lenght of 2d span");
        if (zigzagSpan.Length % chunkSize != 0 && height % chunkSize != 0 && width % chunkSize != 0) throw new Exception($"zigzag length: {zigzagSpan.Length}, height: {height}, width: {width} cannot be chunked into equal chunks of: {chunkSize}");

        int rowChunksNum = height / chunkSize;
        int colChunksNum = width / chunkSize;

        YCbCrPixel[,] reversed2D = new YCbCrPixel[height, width];


        return reversed2D;
    }

}
