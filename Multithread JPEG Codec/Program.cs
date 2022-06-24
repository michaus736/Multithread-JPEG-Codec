using Multithread_JPEG_Codec;




//testing

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
