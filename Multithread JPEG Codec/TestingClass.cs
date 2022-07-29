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



namespace Multithread_JPEG_Codec;

internal class TestingClass
{
    ILGPU.Context context;
    
    public enum builderAcceleratorType
    {
        OpenCL, CUDA, Cpu, All
    }

    public TestingClass(TestingClass.builderAcceleratorType type = builderAcceleratorType.OpenCL)
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


        //Stream consoleStream = Console.OpenStandardOutput();
        string filePath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "devices_list.txt";
        FileStream fileStream = File.OpenWrite(filePath);
        Stream stream = fileStream;

        StreamWriter sw = new StreamWriter(stream);
        foreach (var (device, accelerator, stringWriter) in from device in context.GetPreferredDevices(preferCPU: false, matchingDevicesOnly: false)
                                                            let accelerator = device.CreateAccelerator(context)
                                                            let stringWriter = new StringWriter()
                                                            select (device, accelerator, stringWriter))
        {
            accelerator.PrintInformation(stringWriter);
            sw.WriteLine("hardware: {0}, accelerator: {1}",
                        device.Name,
                        accelerator.Name
                        );
            sw.WriteLine("accelerator info: {0}", stringWriter.ToString());
            sw.WriteLine();
            accelerator.Dispose();
        }
        sw.Dispose();
        if(stream is FileStream)
        {
            Console.WriteLine($"creating file in {filePath}");
        }
        stream.Dispose();
    }

    public static void printDevices(StreamWriter sw, TestingClass.builderAcceleratorType acceleratorType = TestingClass.builderAcceleratorType.Cpu)
    {
        ILGPU.Context context = Context.Create(builder =>
        {
            switch (acceleratorType)
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


        
        sw.WriteLine($"{acceleratorType} accelerators:");
        foreach (var (device, accelerator, stringWriter) in from device in context.GetPreferredDevices(preferCPU: false, matchingDevicesOnly: false)
                                                            let accelerator = device.CreateAccelerator(context)
                                                            let stringWriter = new StringWriter()
                                                            select (device, accelerator, stringWriter))
        {
            accelerator.PrintInformation(stringWriter);
            sw.WriteLine("\thardware: {0}, accelerator: {1}",
                        device.Name,
                        accelerator.Name
                        );
            sw.WriteLine("\taccelerator info: {0}", stringWriter.ToString());
            sw.WriteLine();
            accelerator.Dispose();
        }
    }



}
