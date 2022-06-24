using Multithread_JPEG_Codec;




//testing
string filePath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "devices_list.txt";
//Stream consoleStream = Console.OpenStandardOutput();
FileStream fileStream = File.Open(filePath, FileMode.Append, FileAccess.Write);


var test = new ExtensionClass(ExtensionClass.builderAcceleratorType.Cpu);



///todo:
///serilog
///loading images in foreach
///apply ilgpu 
///
///jpeg direction:
///extract rgb from image
///convert to YCbCr
///sampling to blocks
///2d dct
///quantization(quaz tables)
///zig zag scan
///entropy coding:
///     extract ac dc components
///     encoding
///      huffmann encoding
///create jpeg file
