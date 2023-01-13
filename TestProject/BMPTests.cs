using System.Configuration;
using System.Drawing;
using MultithreadEncodeOpenCV;
using OpenCvSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;


namespace TestProject;

public class BMPTests
{
    string bmpDirectory;
    private string[] bmpFilesPaths;

    public BMPTests()
    {
        ConfigurationFileMap fileMap = new ConfigurationFileMap(@"C:\workspace\dotnet\Multithread JPEG Codec\App.config");
        System.Configuration.Configuration configuration = ConfigurationManager.OpenMappedMachineConfiguration(fileMap);

        bmpDirectory = configuration.AppSettings.Settings["testBmpDirectory"].Value;
        bmpFilesPaths = Directory.EnumerateFiles(bmpDirectory, "*.bmp").ToArray();
    }




    [Fact]
    public void Convert_ShouldConvertBmpToJpeg()
    {
        // Arrange
        
        var inputFilePath = bmpDirectory+@"\boats24.bmp";
        var outputFilePath = bmpDirectory+@"\results\boats24.jpg";
        // Act
        BmpToJpegConverter.Convert(inputFilePath, outputFilePath, 20);

        // Assert
        Assert.True(File.Exists(outputFilePath));
        using (var outputMat = Cv2.ImRead(outputFilePath))
        {
            Assert.Equal(MatType.CV_8UC3, outputMat.Type());
        }
    }
    
}
