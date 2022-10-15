using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Enumerables;
using Multithread_JPEG_Codec.Models;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.Metadata.Profiles.Icc;

namespace Multithread_JPEG_Codec.DCT;

public class DCTHandler
{
    static int _chunkSize;

    DCTHandler(int chunkSize)
    {
        _chunkSize = chunkSize;
    }
    
    /// <summary>
    /// Performing 2d dct transform on 2d span with width and height 8*n x 8*m by performing 1d dct for rows and columns separatelly
    /// </summary>
    /// <param name="spanArr"></param>
    /// <param name="chunkSize"></param>
    public static void PerformDCT(Span2D<YCbCrPixel> spanArr, int chunkSize = 8)
    {
        

        int numOfChunksHor = spanArr.Width / chunkSize;
        int numOfChunksVert = spanArr.Height / chunkSize;

        //dct horizontally
        for (int i = 0; i < spanArr.Height; i++)
        {
            var rowEnumerable = spanArr.GetRow(i);
            //var rowEnumerator = rowEnumerable.GetEnumerator();

            int chunkOffset = 0;
            for (int j = 0; j < numOfChunksHor; j++) 
            {
                
                PerformDCT1DPixel(rowEnumerable, chunkOffset, chunkSize);
                chunkOffset += chunkSize;
            }
        }


        //dct vertically
        for(int i = 0; i < spanArr.Width; i++)
        {
            var columnEnumerable = spanArr.GetColumn(i);
            //var columnEnumerator = columnEnumerator.GetEnumerator();

            int chunkOffset = 0;
            for (int j = 0; j < numOfChunksVert; j++) 
            {
                PerformDCT1DPixel(columnEnumerable, chunkOffset, chunkSize);
                chunkOffset += chunkSize;
            }
        }

    }
    static void PerformDCT1DPixel(RefEnumerable<YCbCrPixel> chunk, int chunkOffset, int chunkSize)
    {
        float inverseRoot = 1f / MathF.Sqrt(2f * chunkSize);
        YCbCrPixel[] tempDCT = new YCbCrPixel[chunkSize];
        for(int k = 0;k< chunkSize;k++)
        {
            float Ck = (k == 0) ? 1f / MathF.Sqrt(2f) : 1f;

            float sumY = 0;
            float sumCb = 0;
            float sumCr = 0;

            float cosinusKernel = k * MathF.PI / (2 * chunkSize);

            for(int n = 0;n<chunkSize;n++)
            {
                sumY += chunk[chunkOffset + n].Y * 2f * MathF.Cos((2f * n + 1f) * cosinusKernel);
                sumCb += chunk[chunkOffset + n].Cb * 2f * MathF.Cos((2f * n + 1f) * cosinusKernel);
                sumCr += chunk[chunkOffset + n].Cr * 2f * MathF.Cos((2f * n + 1f) * cosinusKernel);
            }
            tempDCT[k] = new YCbCrPixel
            {
                Y = inverseRoot * Ck * sumY,
                Cb = inverseRoot * Ck * sumCb,
                Cr = inverseRoot * Ck * sumCr
            };
        }

        for(int i = 0;i<chunkSize;i++)
        {
            chunk[chunkOffset + i] = tempDCT[i];
        }


    }


    public static void PerformInverseDCT(Span2D<YCbCrPixel> spanArr, int chunkSize = 8)
    {
        int numOfChunksHor = spanArr.Width / chunkSize;
        int numOfChunksVert = spanArr.Height / chunkSize;

        //dct horizontally
        for (int i = 0; i < spanArr.Height; i++)
        {
            var rowEnumerable = spanArr.GetRow(i);
            //var rowEnumerator = rowEnumerable.GetEnumerator();

            int chunkOffset = 0;
            for (int j = 0; j < numOfChunksHor; j++)
            {

                PerformIDCT1DPixel(rowEnumerable, chunkOffset, chunkSize);
                chunkOffset += chunkSize;
            }
        }


        //dct vertically
        for (int i = 0; i < spanArr.Width; i++)
        {
            var columnEnumerable = spanArr.GetColumn(i);
            //var columnEnumerator = columnEnumerator.GetEnumerator();

            int chunkOffset = 0;
            for (int j = 0; j < numOfChunksVert; j++)
            {
                PerformIDCT1DPixel(columnEnumerable, chunkOffset, chunkSize);
                chunkOffset += chunkSize;
            }
        }
    }

    private static void PerformIDCT1DPixel(RefEnumerable<YCbCrPixel> chunk, int chunkOffset, int chunkSize)
    {
        float inverseRoot = 1f / MathF.Sqrt(2f * chunkSize);
        YCbCrPixel[] tempIDCT = new YCbCrPixel[chunkSize];
        for(int n=0;n<chunkSize; n++)
        {
            float sumY = 0;
            float sumCb = 0;
            float sumCr = 0;

            float cosinusKernel = MathF.PI * (2f * n + 1f) / (2 * chunkSize);
            for(int k=0;k<chunkSize;k++) {
                float Ck = (k == 0) ? 1/MathF.Sqrt(2): 1f;

                sumY += Ck * chunk[chunkOffset + k].Y * 2f * MathF.Cos(k * cosinusKernel);
                sumCb += Ck * chunk[chunkOffset + k].Cb * 2f * MathF.Cos(k * cosinusKernel);
                sumCr += Ck * chunk[chunkOffset + k].Cr * 2f * MathF.Cos(k * cosinusKernel);


            }
            tempIDCT[n] = new YCbCrPixel
            {
                Y = inverseRoot * sumY,
                Cb = inverseRoot * sumCb,
                Cr = inverseRoot * sumCr
            };
        }

        for(int i=0;i<chunkSize;i++)
        {
            chunk[chunkOffset + i] = tempIDCT[i];
        }


    }
}
