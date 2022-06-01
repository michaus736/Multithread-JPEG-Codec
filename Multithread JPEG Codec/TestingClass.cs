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
}
