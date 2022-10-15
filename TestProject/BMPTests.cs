using System.Configuration;
using System.Drawing;
using Multithread_JPEG_Codec;
using Multithread_JPEG_Codec.BMP;
using Multithread_JPEG_Codec.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;


namespace TestProject;

public class BMPTests
{
    string[] bmpFilesPaths;
    string bmpPath = @"C:\Users\micha\OneDrive\Obrazy\jpeg test pictures\lena_gray.bmp";
    string bmpDirectory;
    public BMPTests()
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
    [Fact]
    public void Execute2DCT()
    {
        BMPReader reader = new BMPReader(bmpPath);

        //convert to ycbcr

        var colorTable = reader.tablePixel;
        YCbCrPixel[,] scaledTable = new YCbCrPixel[colorTable.GetLength(0), colorTable.GetLength(1)];

        if (colorTable is null) throw new Exception("error accessing pixel table");

        for (int i = 0; i < colorTable.GetLength(0); i++)
        {
            for (int j = 0; j < colorTable.GetLength(1); j++)
            {
                scaledTable[i, j] = ExtensionClass.ConvertToYCbCr(colorTable[i, j]);
            }
        }

        
        //subsampling

        ExtensionClass.SubSampling420(ref scaledTable, ExtensionClass.SubSamplingType.average);

        //2dct

        int temp = 0;
        Multithread_JPEG_Codec.DCT.DCTHandler.PerformDCT(scaledTable);

        //print out Y DCT

        bool flag = true;

        using (Image<Rgb24> Image = new Image<Rgb24>(colorTable.GetLength(0), colorTable.GetLength(1)))
        {
            for (int i = 0; i < scaledTable.GetLength(0); i++)
            {
                for (int j = 0; j < scaledTable.GetLength(1); j++)
                {
                    byte dctYByte = (byte)(scaledTable[i, j].Y * 255);
                    Image[i, j] = new Rgb24 {R = dctYByte, G = dctYByte, B = dctYByte };
                }
            }


            Image.SaveAsBmp(bmpDirectory + @"/colorTesting/" + Path.GetFileNameWithoutExtension(bmpDirectory) + "DCT.bmp");
        }

        // inverse 2dct
        Multithread_JPEG_Codec.DCT.DCTHandler.PerformInverseDCT(scaledTable);

        //get back rgb pallet
        var backedRGB = new RGBPixel[scaledTable.GetLength(0), scaledTable.GetLength(1)];

        for (int i = 0; i < backedRGB.GetLength(0); i++)
        {
            for (int j = 0; j < backedRGB.GetLength(1); j++)
            {
                backedRGB[i, j] = ExtensionClass.ConvertToRGB(scaledTable[i, j]);
            }
        }
        //print out restored image
        using (Image<Rgb24> Image = new Image<Rgb24>(colorTable.GetLength(0), colorTable.GetLength(1)))
        {
            for (int i = 0; i < scaledTable.GetLength(0); i++)
            {
                for (int j = 0; j < scaledTable.GetLength(1); j++)
                {
                    Image[i, j] = new Rgb24 { R = backedRGB[i, j].R, G = backedRGB[i, j].G, B = backedRGB[i, j].B };
                }
            }


            Image.SaveAsBmp(bmpDirectory + @"/colorTesting/" + Path.GetFileNameWithoutExtension(bmpDirectory) + "RESTORED.bmp");
        }





    }
}
