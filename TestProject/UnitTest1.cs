using System.Configuration;
using System.Drawing;
using System.Runtime.InteropServices;
using Multithread_JPEG_Codec;
using Multithread_JPEG_Codec.BMP;
using Multithread_JPEG_Codec.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace TestProject;

public class UnitTest1
{
    string[] bmpFilesPaths;
    string bmpPath = @"C:\Users\micha\OneDrive\Obrazy\jpeg test pictures\lena-soderberg-topless.bmp";
    string bmpDirectory;
    public UnitTest1()
    {
        ConfigurationFileMap fileMap = new ConfigurationFileMap(@"C:\workspace\dotnet\Multithread JPEG Codec\App.config");
        System.Configuration.Configuration configuration = ConfigurationManager.OpenMappedMachineConfiguration(fileMap);

        bmpDirectory = configuration.AppSettings.Settings["testBmpDirectory"].Value;
        bmpFilesPaths = Directory.EnumerateFiles(bmpDirectory, "*.bmp").ToArray();
    }


    [Fact]
    public void GettingPixelsFromBmp()
    {
        
        BMPReader reader = new BMPReader(bmpPath);
    }

    [Fact]
    public void CompareYCbCrPixels()
    {

        BMPReader reader = new BMPReader(bmpPath);

        //convert to ycbcr

        var colorTable = reader.tablePixel;
        YCbCrPixel[,] scaledTable = new YCbCrPixel[colorTable.GetLength(0), colorTable.GetLength(1)]; 

        if (colorTable is null) throw new Exception("error accessing pixel table");

        for(int i = 0; i < colorTable.GetLength(0); i++)
        {
            for(int j = 0; j<colorTable.GetLength(1); j++)
            {
                scaledTable[i, j] = ExtensionClass.ConvertToYCbCr(colorTable[i, j]);
            }
        }


        //y scale
        using (Image<Rgb24> Image = new Image<Rgb24>(colorTable.GetLength(0), colorTable.GetLength(1)))
        {
            for(int i = 0; i < scaledTable.GetLength(0); i++)
            {
                for (int j = 0; j < scaledTable.GetLength(1); j++)
                {
                    Image[i, j] = new Rgb24 { R = ExtensionClass.ConvertRGBFloatToByte(scaledTable[i, j].Y), B = 0, G = 0 };
                }
            }


            Image.SaveAsBmp(bmpDirectory + @"/colorTesting/" + Path.GetFileNameWithoutExtension(bmpDirectory) + "Yscale.bmp");
        }

        //cb scale
        using (Image<Rgb24> Image = new Image<Rgb24>(colorTable.GetLength(0), colorTable.GetLength(1)))
        {
            for (int i = 0; i < scaledTable.GetLength(0); i++)
            {
                for (int j = 0; j < scaledTable.GetLength(1); j++)
                {
                    Image[i, j] = new Rgb24 { R = 0, B = ExtensionClass.ConvertRGBFloatToByte(scaledTable[i, j].Cb + 0.5f), G = 0 };
                }
            }


            Image.SaveAsBmp(bmpDirectory + @"/colorTesting/" + Path.GetFileNameWithoutExtension(bmpDirectory) + "Cbscale.bmp");
        }

        //cr scale
        using (Image<Rgb24> Image = new Image<Rgb24>(colorTable.GetLength(0), colorTable.GetLength(1)))
        {
            for (int i = 0; i < scaledTable.GetLength(0); i++)
            {
                for (int j = 0; j < scaledTable.GetLength(1); j++)
                {
                    Image[i, j] = new Rgb24 { R = 0, B = 0, G = ExtensionClass.ConvertRGBFloatToByte(scaledTable[i, j].Cr + 0.5f) };
                }
            }


            Image.SaveAsBmp(bmpDirectory + @"/colorTesting/" + Path.GetFileNameWithoutExtension(bmpDirectory) + "Crscale.bmp");
        }
        
        ExtensionClass.SubSampling420(ref scaledTable, ExtensionClass.SubSamplingType.average);

        var backedRGB = new RGBPixel[scaledTable.GetLength(0), scaledTable.GetLength(1)];

        for(int i = 0; i < backedRGB.GetLength(0); i++)
        {
            for(int j = 0; j < backedRGB.GetLength(1); j++)
            {
                backedRGB[i, j] = ExtensionClass.ConvertToRGB(scaledTable[i, j]);
            }
        }

        //backed rgb scale
        using (Image<Rgb24> Image = new Image<Rgb24>(colorTable.GetLength(0), colorTable.GetLength(1)))
        {
            for (int i = 0; i < scaledTable.GetLength(0); i++)
            {
                for (int j = 0; j < scaledTable.GetLength(1); j++)
                {
                    Image[i, j] = new Rgb24 { R = backedRGB[i,j].R, G = backedRGB[i,j].G, B = backedRGB[i,j].B };
                }
            }


            Image.SaveAsBmp(bmpDirectory + @"/colorTesting/" + Path.GetFileNameWithoutExtension(bmpDirectory) + "RGBscale.bmp");
        }




    }
}
