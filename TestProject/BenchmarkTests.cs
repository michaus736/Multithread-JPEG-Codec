using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;


namespace TestProject;

[Collection("Benchmark")]
public class BenchmarkTests
{
    [Fact]
    public void RunBenchmarks()
    {
        var summary = BenchmarkRunner.Run<Multithread_JPEG_Codec.Benchmarks.BmpToJpegConverterBenchmarks>();
        Assert.True(summary.Reports.Count() > 0);
    }
}
