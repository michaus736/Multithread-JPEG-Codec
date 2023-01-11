using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using MultithreadEncodeOpenCV;

namespace Multithread_JPEG_Codec.Benchmarks;
[SimpleJob]
[MemoryDiagnoser]
[ThreadingDiagnoser]
[BenchmarkDotNet.Attributes.MedianColumn]
[BenchmarkDotNet.Attributes.MeanColumn]
[BenchmarkCategory("Conversion")]
public class BmpToJpegConverterWithMoreRegionsBenchmarks
{
    public static readonly string ResultsFolder = "C:\\Users\\micha\\OneDrive\\Obrazy\\jpeg test pictures\\results\\";
    public static readonly string InputFolder = "C:\\Users\\micha\\OneDrive\\Obrazy\\jpeg test pictures\\";


    [Params("boats24.bmp",
        "lena24.bmp",
        "MARBLES.BMP",
        "sample_5184×3456.bmp")]
    public string InputBmpFileName { get; set; } = string.Empty;

    [Params(1, 3, 4, 16, 100)]
    public int RegionCount { get; set; }




    [Benchmark]
    public void Convert()
    {
        Thread.CurrentThread.Priority = ThreadPriority.Normal;
        ConvertAndMeasurePerformance();
    }

    private void ConvertAndMeasurePerformance()
    {
        string inputBmpFileName = Path.Combine(InputFolder, InputBmpFileName);
        string outputJpegFileName = Path.Combine(ResultsFolder, Path.GetFileNameWithoutExtension(InputBmpFileName) + Guid.NewGuid().ToString() + ".jpg");

        var stopwatch = Stopwatch.StartNew();

        BmpToJpegConverter.ConvertWithMoreRegions(
            inputBmpFileName,
            outputJpegFileName,
            95,
            RegionCount);

        stopwatch.Stop();
        Console.WriteLine($"Conversion took {stopwatch.ElapsedMilliseconds} milliseconds");
    }
}
