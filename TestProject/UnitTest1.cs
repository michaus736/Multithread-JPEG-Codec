using System.Configuration;
using Multithread_JPEG_Codec.BMP;

namespace TestProject;

public class UnitTest1
{
    [Fact]
    public void GettingPixelsFromBmp()
    {
        ConfigurationFileMap fileMap = new ConfigurationFileMap(@"C:\workspace\dotnet\Multithread JPEG Codec\App.config");
        Configuration configuration = ConfigurationManager.OpenMappedMachineConfiguration(fileMap);

        var bmpDirectory = configuration.AppSettings.Settings["testBmpDirectory"].Value;
        var bmpsFiles = Directory.EnumerateFiles(bmpDirectory, "*.bmp").ToList();
        

        var bmpPath = @"C:\Users\micha\OneDrive\Obrazy\jpeg test pictures\sample_1920×1280.bmp";

        BMPReader reader = new BMPReader(bmpPath);

    }
}
