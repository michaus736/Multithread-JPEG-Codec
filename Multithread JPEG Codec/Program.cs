
using BenchmarkDotNet.Running;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MultithreadEncodeOpenCV;
/*
string inputFilePath = "C:\\Users\\micha\\OneDrive\\Obrazy\\jpeg test pictures\\boats24.bmp",
    outputFilePath = "C:\\Users\\micha\\OneDrive\\Obrazy\\jpeg test pictures\\results\\boats24.jpg";

BmpToJpegConverter.Convert(inputFilePath, outputFilePath, 20);
*/

var summary = BenchmarkRunner.Run<Multithread_JPEG_Codec.Benchmarks.BmpToJpegConverterBenchmarks>();
