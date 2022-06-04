using Multithread_JPEG_Codec;




//testing
string filePath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "devices_list.txt";
//Stream consoleStream = Console.OpenStandardOutput();
FileStream fileStream = File.Open(filePath, FileMode.Append, FileAccess.Write);

Stream stream = fileStream;
StreamWriter sw = new StreamWriter(stream);

TestingClass.printDevices(sw, TestingClass.builderAcceleratorType.Cpu);
TestingClass.printDevices(sw, TestingClass.builderAcceleratorType.OpenCL);

if(stream is FileStream)
{
    Console.WriteLine($"appeding file in: {filePath}");
}


sw.Dispose();
Console.WriteLine("press key to exit");
Console.ReadKey();