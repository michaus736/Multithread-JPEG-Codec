
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using BenchmarkDotNet.Running;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Configuration;
using Multithread_JPEG_Codec;
using MultithreadEncodeOpenCV;

//string inputFilePath = "C:\\Users\\micha\\OneDrive\\Obrazy\\jpeg test pictures\\boats24.bmp",
//    outputFilePath = "C:\\Users\\micha\\OneDrive\\Obrazy\\jpeg test pictures\\results\\boats24.jpg";

//BmpToJpegConverter.ConvertWithMoreRegions(inputFilePath, outputFilePath, 20, 5);


//var summary = BenchmarkRunner.Run<Multithread_JPEG_Codec.Benchmarks.BmpToJpegConverterBenchmarks>();
//var summary = BenchmarkRunner.Run<Multithread_JPEG_Codec.Benchmarks.BmpToJpegConverterWithMoreRegionsBenchmarks>();

Dictionary<string, string> startArgs = new Dictionary<string, string>().ConfigureArgs(args);

if(startArgs.Count == 0)
{
    global::System.Console.WriteLine(
        """
        Multithread Codec JPEG:
        A given program is part of an engineering thesis,
        its task is to encode a BMP image to JPEG with a given quality using multithreading techniques.
        To use program use args:
        --bmpFile "filepath" - path to bmp file
        --resultFile "filepath" - path to output JPEG file
        [--quality 95] - quality of output file, from 10 to 100. default is 95
        [--resetInterval 0] - set reset interval, from 0 to 65535
        """
        );
    return;
}

try
{
    if (!startArgs.ContainsKey("bmpFile") || !startArgs.ContainsKey("resultFile")) throw new ArgumentException("'bmpFile' and 'resultFile' arguments are required");
    if (!Path.Exists(startArgs["bmpFile"])) throw new ArgumentException($"BMP File: {startArgs["BmpFile"]} does not exist");
    if (!Path.Exists(Path.GetDirectoryName(startArgs["resultFile"]))) throw new ArgumentException($"Directory of result JPEG file: {startArgs["resultFile"]} does not exist\nDirectory: {Path.GetDirectoryName(startArgs["resultFile"])}");
    int quality, resetInterval;
    if(!startArgs.ContainsKey("quality")) quality = 95;
    else
    {
        if (!int.TryParse(startArgs["quality"], out quality))
        {
            throw new ArgumentException("cannot convert quality to integer");
        }
        if (quality < 10 || quality > 100) throw new ArgumentException("quality value can be only between 10 and 100");
    }
    if(!startArgs.ContainsKey("resetInterval")) resetInterval = 0;
    else
    {
        if (!int.TryParse(startArgs["resetInterval"], out resetInterval))
        {
            throw new ArgumentException("cannot convert quality to integer");
        }
        if (resetInterval < 0 || resetInterval > 100) throw new ArgumentException("quality value can be only between 10 and 100");
    }
    
    
    Stopwatch sw = Stopwatch.StartNew();
    BmpToJpegConverter.Convert(startArgs["bmpFile"], startArgs["resultFile"], quality, resetInterval);
    sw.Stop();
    Console.WriteLine($"Conversion took {sw.ElapsedMilliseconds} milliseconds");


}
catch (Exception ex)
{
    global::System.Console.WriteLine(ex.Message);
    global::System.Console.WriteLine(ex?.InnerException?.Message ?? "no inner exception occured");
    global::System.Console.WriteLine(ex?.StackTrace ?? "no stack trace");
}

