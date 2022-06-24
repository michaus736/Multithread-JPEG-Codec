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

namespace Multithread_JPEG_Codec;

public class ExtensionClass
{
    ILGPU.Context context;
    public enum builderAcceleratorType
    {
        OpenCL, CUDA, Cpu, All
    }

    public ExtensionClass(ExtensionClass.builderAcceleratorType type = builderAcceleratorType.OpenCL)
    {
        context = Context.Create(builder =>
        {
            switch (type)
            {
                case builderAcceleratorType.OpenCL:
                    builder.OpenCL();
                    break;
                case builderAcceleratorType.CUDA:
                    builder.Cuda();
                    break;
                case builderAcceleratorType.Cpu:
                    builder.CPU();
                    break;
                case builderAcceleratorType.All:
                    builder.AllAccelerators();
                    break;
                default:
                    builder.OpenCL();
                    break;
            }
        });
        
        foreach (var device in context.GetPreferredDevices(preferCPU: false, matchingDevicesOnly: false))
        {
            using Accelerator accelerator = device.CreateAccelerator(context);
            using StringWriter stringWriter = new StringWriter();
            accelerator.PrintInformation(stringWriter);
            Console.WriteLine("hardware: {0}, accelerator: {1}",
                device.Name,
                accelerator.Name
                );
            
            Console.WriteLine("accelerator info: {0}", stringWriter.ToString());
            Console.WriteLine();
            accelerator.Dispose();

        }
    }

    public static void ConvertToYCbCr()
    {
        var bmpsFiles = Directory.EnumerateFiles(Resource.bmpDirectory, "*.bmp");

        foreach (var bmpPath in bmpsFiles)
        {
            
            using Image<Rgba32> image = Image.Load<Rgba32>(bmpPath);
            //if (image.Height % 8 != 0 || image.Width % 8 != 0) throw new Exception("wrong image resolution");
            //converting to ycbcr
            image.ProcessPixelRows(acc =>
            {
                for(int y = 0; y < acc.Height; y++)
                {
                    Span<Rgba32> row = acc.GetRowSpan(y);
                    for (int x = 0; x < row.Length; x++)
                    {
                        ref Rgba32 pixel = ref row[x];
                        pixel = ConvertToYCbCr(pixel);
                        
                    }

                }
                //subsampling 4:2:0
                Subsampling420(acc);


                //back to rgb color scale
                for (int y = 0; y < acc.Height; y++)
                {
                    Span<Rgba32> row = acc.GetRowSpan(y);
                    for (int x = 0; x < row.Length; x++)
                    {
                        ref Rgba32 pixel = ref row[x];
                        pixel = ConvertRgba(pixel);

                    }

                }




            });

            //saving 
            string fileName = Path.GetFileName(bmpPath);
            string newFilePath = Resource.jpegDirectory + fileName;

            image.SaveAsBmp(newFilePath);



        }

    }


    private static void Subsampling420(PixelAccessor<Rgba32> acc , char type = 'a')
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



    }

    public static Rgba32 ConvertToYCbCr(Rgba32 rgba)
    {
        
        return new Rgba32(
            (float)((float)(16 + 65.738 * rgba.R / 256 + 129.057 * rgba.G / 256 + 25.064 * rgba.B / 256) / 256.0),
            (float)((float)(128 - 37.945 * rgba.R / 256 - 74.494 * rgba.G / 256 + 112.439 * rgba.B / 256) / 256.0),
            (float)((float)(128 + 112.439 * rgba.R / 256 - 94.154 * rgba.G / 256 - 18.285 * rgba.B / 256) / 256.0),
            (float)rgba.A

       );
        
    }

    public static Rgba32 ConvertRgba(Rgba32 pixel)
    {

        return new Rgba32(
            (float)((float)(298.082 * pixel.R / 256 + 408.583 * pixel.B / 256 - 222.921) / 256.0),
            (float)((float)(298.082 * pixel.R / 256 - 100.291 * pixel.G / 256 - 208.120 * pixel.B / 256 + 135.576) / 256.0),
            (float)((float)(298.082 * pixel.R / 256 + 516.412 * pixel.G / 256 - 276.836) / 256.0),
            (float)pixel.A
       );


    }


}
