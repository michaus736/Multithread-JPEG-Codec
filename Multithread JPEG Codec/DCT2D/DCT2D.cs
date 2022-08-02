using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Multithread_JPEG_Codec.Models;

namespace Multithread_JPEG_Codec.DCT2D;

public static class DCT2D
{
    public static bool isSquaredTable<T>(T[,] arr)
    {
        return arr.GetLength(0) == arr.GetLength(1);
    }

    public static YCbCrPixel[,] InitCoofficientTable(int chunkSize)
    {
        YCbCrPixel[,] initTable = new YCbCrPixel[chunkSize, chunkSize];
        var sqrtOf2 = Math.Sqrt(2.0);
        for(int i = 0; i < chunkSize; i++)
        {
            initTable[i, 0].Y = (float)(sqrtOf2 / chunkSize);
            initTable[i, 0].Cb = (float)(sqrtOf2 / chunkSize);
            initTable[i, 0].Cr = (float)(sqrtOf2 / chunkSize);

            initTable[0, i].Y = (float)(sqrtOf2 / chunkSize);
            initTable[0, i].Cb = (float)(sqrtOf2 / chunkSize);
            initTable[0, i].Cr = (float)(sqrtOf2 / chunkSize);
        }

        initTable[0, 0].Y = (float)(1.0 / chunkSize);
        initTable[0, 0].Cb = (float)(1.0 / chunkSize);
        initTable[0, 0].Cr = (float)(1.0 / chunkSize);

        for (int i = 1; i < chunkSize; i++)
        {
            for (int j = 1; j < chunkSize; j++)
            {
                initTable[i, j].Y = (float)(2.0 / chunkSize);
                initTable[i, j].Cb = (float)(2.0 / chunkSize);
                initTable[i, j].Cr = (float)(2.0 / chunkSize);
            }
        }


        return initTable;
    }

    public static YCbCrPixel[,] DCT2DPerform(YCbCrPixel[,] pixels)
    {


        return null;
    }




}
