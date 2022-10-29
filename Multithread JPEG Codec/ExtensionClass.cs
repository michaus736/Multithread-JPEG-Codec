using ILGPU;
using ILGPU.Runtime;
using ILGPU.Runtime.CPU;
using ILGPU.Runtime.Cuda;
using ILGPU.Runtime.OpenCL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Intrinsics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Runtime.InteropServices;
using Multithread_JPEG_Codec.Models;

namespace Multithread_JPEG_Codec;

public static class ExtensionClass
{
    public enum SubSamplingType
    {
        average,
        leftTopPixel
    }


    public static void SubSampling420(ref YCbCrPixel[,] pixels, SubSamplingType type = SubSamplingType.average)
    {
        for (int i = 0; i < pixels.GetLength(0) - 1; i += 2)
        {
            for (var j = 0; j < pixels.GetLength(1) - 1; j += 2)
            {
                ref YCbCrPixel pixel1 = ref pixels[i, j];
                ref YCbCrPixel pixel2 = ref pixels[i, j + 1];
                ref YCbCrPixel pixel3 = ref pixels[i + 1, j];
                ref YCbCrPixel pixel4 = ref pixels[i + 1, j + 1];

                float subSampledCb = type switch
                {
                    SubSamplingType.average => (pixel1.Cb + pixel2.Cb + pixel3.Cb + pixel4.Cb) / 4,
                    SubSamplingType.leftTopPixel => pixel1.Cb,
                    _ => (pixel1.Cb + pixel2.Cb + pixel3.Cb + pixel4.Cb) / 4
                };
                float subSampledCr = type switch
                {
                    SubSamplingType.average => (pixel1.Cr + pixel2.Cr + pixel3.Cr + pixel4.Cr) / 4,
                    SubSamplingType.leftTopPixel => pixel1.Cr,
                    _ => (pixel1.Cr + pixel2.Cr + pixel3.Cr + pixel4.Cr) / 4
                };

                pixel1.Cr = subSampledCr;
                pixel1.Cb = subSampledCb;
                pixel2.Cr = subSampledCr;
                pixel2.Cb = subSampledCb;
                pixel3.Cr = subSampledCr;
                pixel3.Cb = subSampledCb;
                pixel4.Cr = subSampledCr;
                pixel4.Cb = subSampledCb;
                


            }
        }
    }
    private static void SubSampling420(PixelAccessor<Rgba32> acc , char type = 'a')
    {
        for(int y = 0; y < acc.Height - 1; y+=2)
        {
            var row1 = acc.GetRowSpan(y);
            var row2 = acc.GetRowSpan(y+1);
            for(int x = 0; x < row1.Length - 1; x+=2)
            {
                ref Rgba32 pixel1 = ref row1[x];
                ref Rgba32 pixel2 = ref row1[x + 1];
                ref Rgba32 pixel3 = ref row2[x];
                ref Rgba32 pixel4 = ref row2[x + 1];
                byte leftSignificantCb = pixel1.G;
                byte leftSignificantCr = pixel1.B;
                byte averageCb = (byte)(((int)pixel1.G + (int)pixel2.G + (int)pixel3.G + (int)pixel4.G) / 4);
                byte averageCr = (byte)(((int)pixel1.B + (int)pixel2.B + (int)pixel3.B + (int)pixel4.B) / 4);
                Rgba32 subsampledPixel = new Rgba32(
                    pixel1.R,
                    type switch
                {
                    'l'=>leftSignificantCb,
                    'a'=>averageCb,
                    _ =>averageCb
                },
                type switch
                {
                    'l' => leftSignificantCr,
                    'a' => averageCr,
                    _ => averageCr
                },
                pixel1.A);


                pixel1 = subsampledPixel;
                pixel2 = subsampledPixel;
                pixel3 = subsampledPixel;
                pixel4 = subsampledPixel;
            }
        }
        int a = 0;
        


    }
    
    public static byte ConvertRGBFloatToByte(float value)
    {
        var temp = (int)Math.Floor(value >= 1.0 ? (double)255 : (double)(value * 256));
        return (byte)temp;
    }

    public static YCbCrPixel ConvertToYCbCr(RGBPixel rgb)
    {
        float fr = (float)rgb.R / 255;
        float fg = (float)rgb.G / 255;
        float fb = (float)rgb.B / 255;
        return new YCbCrPixel {
            Y = (float)(0.2989 * fr + 0.5866 * fg + 0.1145 * fb),
            Cb = (float)(-0.1687 * fr - 0.3313 * fg + 0.5000 * fb),
            Cr = (float)(0.5000 * fr - 0.4184 * fg - 0.0816 * fb),
       };
    }

    public static RGBPixel ConvertToRGB(YCbCrPixel ycbcr)
    {
        float r = Math.Max(0.0f, Math.Min(1.0f, (float)(ycbcr.Y + 0.0000 * ycbcr.Cb + 1.4022 * ycbcr.Cr)));
        float g = Math.Max(0.0f, Math.Min(1.0f, (float)(ycbcr.Y - 0.3456 * ycbcr.Cb - 0.7145 * ycbcr.Cr)));
        float b = Math.Max(0.0f, Math.Min(1.0f, (float)(ycbcr.Y + 1.7710 * ycbcr.Cb + 0.0000 * ycbcr.Cr)));

        return new RGBPixel { 
            R = (byte)(r * 255),
            G = (byte)(g * 255),
            B = (byte)(b * 255)
        };
    }
    


}
