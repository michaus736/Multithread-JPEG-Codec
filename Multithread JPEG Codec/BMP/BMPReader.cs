using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Multithread_JPEG_Codec.Models;

namespace Multithread_JPEG_Codec.BMP;

public class BMPReader
{
    #region Variables
    string fileName;
    public RGBPixel[,] tablePixel = new RGBPixel[0,0];
    private byte[] fileData;
    private Encoding fileEncoding;
    readonly BMPHeader header;
    readonly BMPDIBHeader DIBheader;
    //testing

    #endregion

    #region supportedTypes
    internal readonly short[] supportedBitsPerPixel = new short[] { 8, 24 };
    internal readonly uint[] supportedCompressionFormats = new uint[] { 0 };
    #endregion


    #region auxiliary classes
    public class BMPHeader
    {
        internal Encoding currentEncoding = Encoding.ASCII;
        internal SignatureTypes currentBMPType;
        internal uint currentFileSize;
        internal int reservedFirst;
        internal int reservedSecond;
        internal int tablePixelOffset;

        //1.x version
        internal ushort? os1xbyteWidth;
        internal byte? os1xplanes;
        internal byte? os1xbitsPerPixel;


    }

    public class BMPDIBHeader
    {
        internal uint DIBSize;
        internal BitMapCoreHeaderType bitMapCoreHeaderType;
        internal ushort DIBWitdh;
        internal ushort DIBHeight;
        internal short PlanesCount;
        internal short BitsPerPixel;
        //40 bit extension header
        internal uint? Compression = null;
        internal uint? SizeOfBitmap = null;
        internal int? HorzResolution = null;
        internal int? VertResolution = null;
        internal uint? ColorsUsed = null;
        internal uint? ColorsImportant = null;

    }

    public enum BitMapCoreHeaderType
    {
        OS21XBITMAPHEADER10,
        OS21XBITMAPHEADER12,
        OS22XBITMAPHEADER64,
        OS22XBITMAPHEADER16,
        BITMAPINFOHEADER40,
        BITMAPV2INFOHEADER52,
        BITMAPV3INFOHEADER56,
        BITMAPV4HEADER108,
        BITMAPV5HEADER124,
        INVALID
    }

    public enum SignatureTypes
    {
        BM, // Windows family
        BA, // OS/2 struct bitmap array
        CI, // OS/2 struct color icon
        CP, // OS/2 const color pointer
        IC, // OS/2 struct icon
        PT, // OS/2 pointer
        Invalid, // if two bytes from loaded file don't equal to any values of all BMP possible formats
    };

    #endregion

    

    

    public BMPReader(string filePath)
    {
        
        fileName = filePath;


        

        header = new BMPHeader();
        DIBheader = new BMPDIBHeader();

        ReadFile(filePath);

        BMPParseHeader();
        BMPDIBParseHeader();

        Validate();

        BMPPopulatePixels();


    }


    private void BMPPopulatePixels(short chunkSize = 8)
    {
        
        byte[] colorData = fileData.Skip(header.tablePixelOffset).ToArray();

        this.tablePixel = new RGBPixel[DIBheader.DIBWitdh / chunkSize * chunkSize, DIBheader.DIBHeight / chunkSize * chunkSize];
        //var colorPixelTable = new Color[DIBheader.DIBWitdh, DIBheader.DIBHeight];
        int index = 0;
        
        //i - height, j - width
        for (int i = tablePixel.GetLength(1) - 1; i >= 0; i--)
        {
            for (int j = 0; j < tablePixel.GetLength(0); j++) 
            {
                if(this.DIBheader.BitsPerPixel == 24)
                {
                    byte R = colorData[index++];
                    byte G = colorData[index++];
                    byte B = colorData[index++];

                    tablePixel[j, i] = new RGBPixel{R = B, G = G, B = R};
                    //colorPixelTable[j, i] = Color.FromArgb(B,G,R);

                }
                else if(this.DIBheader.BitsPerPixel == 8)
                {
                    byte grey = colorData[index++];
                    tablePixel[j, i] = new RGBPixel { R = grey, G = grey, B = grey };
                }
                
               


            }
        }
    }


    #region Reading file and validation
    public byte[] ReadFile(string fileName)
    {
        if (!File.Exists(fileName)) throw new ArgumentException("file does not exist");
        if (Path.GetExtension(fileName).ToLower() != ".bmp") throw new ArgumentException("this file's format isn't file");

        FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
        fileData = new byte[stream.Length];
        stream.Read(fileData, 0, fileData.Length);
        stream.Close();
        using (var streamReader = new StreamReader(fileName, true))
        {
            fileEncoding = streamReader.CurrentEncoding;
            header.currentEncoding = fileEncoding;
        }
        return fileData;

    }


    private void Validate()
    {
        if (header.currentBMPType == SignatureTypes.Invalid) throw new FormatException("current file type is not valid bmp file type");
        if (!supportedBitsPerPixel.Contains(DIBheader.BitsPerPixel)) throw new PlatformNotSupportedException($"current format of color: {DIBheader.BitsPerPixel}; is not supported");
        if (DIBheader.Compression is not null && !supportedCompressionFormats.Contains((uint)DIBheader.Compression)) throw new PlatformNotSupportedException($"this compression type: {DIBheader.Compression}; is not supported");

    }
    #endregion

    #region parsing
    internal void BMPParseHeader()
    {
        int actualIndex = 0;
        byte[] BMPHeaderData = new byte[14];

        for (int i = 0; i < BMPHeaderData.Length; i++)
        {
            BMPHeaderData[i] = fileData[i];
        }
        actualIndex += 2;
        byte[] BMPSignature = new byte[2] { BMPHeaderData[0], BMPHeaderData[1] };
        //if bmpsignature=0, then bmp header 1.x
        if (BMPSignature.All(x => x == 0))
        {
            Parse1xHeader(ref actualIndex);
            return;
        }

        header.currentBMPType = BMPCheckType(ref BMPSignature);




        byte[] fileSizeHeaderInfo = new byte[4];
        for (int j = 0; j < fileSizeHeaderInfo.Length; actualIndex++, j++)
        {
            fileSizeHeaderInfo[j] = fileData[actualIndex];
        }

        header.currentFileSize = BMPGetFileSize(ref fileSizeHeaderInfo);



        


        byte[] fileReservedInfo = new byte[4];
        for (int j = 0; j < fileReservedInfo.Length; actualIndex++, j++)
        {
            fileReservedInfo[j] = fileData[actualIndex];
        }
        int[] reservedResult = BMPGetReservedFromHeader(ref fileReservedInfo);
        header.reservedFirst = reservedResult[0];
        header.reservedSecond = reservedResult[1];



        byte[] filePixelOffset = new byte[4];
        for (int j = 0; j < filePixelOffset.Length; actualIndex++, j++)
        {
            filePixelOffset[j] = fileData[actualIndex];
        }
        header.tablePixelOffset = BMPGetColorOffset(ref filePixelOffset);

    }

    private void Parse1xHeader(ref int actualIndex)
    {
        //parse 1.x header
        DIBheader.bitMapCoreHeaderType = BitMapCoreHeaderType.OS21XBITMAPHEADER10;

        byte[] widthData = new byte[2];
        for (int i = 0; i < widthData.Length; i++, actualIndex++)
        {
            widthData[i] = fileData[actualIndex];
        }
        DIBheader.DIBWitdh = DIBGetWidth(ref widthData);


        byte[] heightData = new byte[2];
        for (int i = 0; i < heightData.Length; i++, actualIndex++)
        {
            heightData[i] = fileData[actualIndex];
        }
        DIBheader.DIBHeight = DIBGetHeight(ref heightData);

        byte[] byteWidth = new byte[2];
        for (int i = 0; i < byteWidth.Length; i++, actualIndex++)
        {
            byteWidth[i] = fileData[actualIndex];
        }
        header.os1xbyteWidth = DIBGetHeight(ref byteWidth);

        byte planes = fileData[++actualIndex];
        header.os1xplanes = planes;

        byte bitsPerPixel = fileData[++actualIndex];
        header.os1xbitsPerPixel = bitsPerPixel;




        
    }

    internal void BMPDIBParseHeader()
    {
        int actualIndex = 14;
        byte[] dibHeaderSize = new byte[4];
        for (int j = 0; j < dibHeaderSize.Length; actualIndex++, j++)
        {
            dibHeaderSize[j] = fileData[actualIndex];
        }
        DIBheader.DIBSize = DIBGetSize(ref dibHeaderSize);
        DIBheader.bitMapCoreHeaderType = DIBHeaderType(DIBheader.DIBSize);



        byte[] dibWidth = new byte[4];
        for(int j = 0; j < dibWidth.Length; actualIndex++, j++)
        {
            dibWidth[j] = fileData[actualIndex];
        }
        DIBheader.DIBWitdh = DIBGetWidth(ref dibWidth);



        byte[] dibHeight = new byte[4];
        for(int j = 0;j< dibHeight.Length; actualIndex++, j++)
        {
            dibHeight[j] = fileData[actualIndex];
        }
        DIBheader.DIBHeight = DIBGetHeight(ref dibHeight);


        byte[] dibPlane = new byte[2];
        for(int j = 0;j< dibPlane.Length; actualIndex++, j++)
        {
            dibPlane[j] = fileData[actualIndex];
        }
        DIBheader.PlanesCount = DIBGetPlane(ref dibPlane);


        byte[] dibBytesPerPixel = new byte[2];
        for (int j = 0; j < dibBytesPerPixel.Length; actualIndex++, j++)
        {
            dibBytesPerPixel[j] = fileData[actualIndex];
        }
        DIBheader.BitsPerPixel = DIBGetPixelFormat(ref dibBytesPerPixel);

        //extending dib header

        if (DIBheader.DIBSize < 40) return;

        byte[] compressionData = new byte[4];
        for(int i = 0; i < compressionData.Length; i++, actualIndex++)
        {
            compressionData[i] = fileData[actualIndex];
        }
        DIBheader.Compression = DIBGetCompression(ref compressionData);


        
        byte[] sizeOfBitmapData = new byte[4];
        for(int i = 0;i< sizeOfBitmapData.Length; i++, actualIndex++)
        {
            sizeOfBitmapData[i] = fileData[actualIndex];
        }
        DIBheader.SizeOfBitmap = DIBGetSizeOfBitmap(ref sizeOfBitmapData);

        byte[] herzResolutionData = new byte[4];
        for (int i = 0; i < herzResolutionData.Length; i++, actualIndex++)
        {
            herzResolutionData[i] = fileData[actualIndex];
        }
        DIBheader.HorzResolution = DIBGetHorzVertResolution(ref herzResolutionData);

        byte[] vertResolutionData = new byte[4];
        for (int i = 0; i < vertResolutionData.Length; i++, actualIndex++)
        {
            vertResolutionData[i] = fileData[actualIndex];
        }
        DIBheader.VertResolution = DIBGetHorzVertResolution(ref vertResolutionData);
        
        byte[] colorsUsedData = new byte[4];
        for (int i = 0; i < colorsUsedData.Length; i++, actualIndex++)
        {
            colorsUsedData[i] = fileData[actualIndex];
        }
        DIBheader.ColorsUsed = DIBGetSizeOfBitmap(ref colorsUsedData);

        byte[] colorsImportantData = new byte[4];
        for (int i = 0; i < colorsUsedData.Length; i++, actualIndex++)
        {
            colorsImportantData[i] = fileData[actualIndex];
        }
        DIBheader.ColorsImportant = DIBGetSizeOfBitmap(ref colorsImportantData);


    }






    #endregion



    #region extracting values from bits
    private int? DIBGetHorzVertResolution(ref byte[] ResolutionData)
    {
        int Resolution = 0;


        for (int i = 0; i < ResolutionData.Length; i++)
        {
            Resolution += ResolutionData[i];
        }

        return Resolution;
    }
    private uint? DIBGetSizeOfBitmap(ref byte[] sizeOfBitmapData)
    {
        uint SizeOfBitMap = (uint)sizeOfBitmapData[3] << 24 | (uint)sizeOfBitmapData[2] << 16 | (uint)sizeOfBitmapData[1] << 8 | (uint)sizeOfBitmapData[0];
        return SizeOfBitMap;
    }

    private uint? DIBGetCompression(ref byte[] compressionData)
    {
        uint Compression = (uint)compressionData[3] << 24 | (uint)compressionData[2] << 16 | (uint)compressionData[1] << 8 | (uint)compressionData[0];
        return Compression;
    }
    private static BitMapCoreHeaderType DIBHeaderType(uint size)
    {
        return size switch
        {
            12 => BitMapCoreHeaderType.OS21XBITMAPHEADER12,
            64 => BitMapCoreHeaderType.OS22XBITMAPHEADER64,
            16 => BitMapCoreHeaderType.OS22XBITMAPHEADER16,
            40 => BitMapCoreHeaderType.BITMAPINFOHEADER40,
            52 => BitMapCoreHeaderType.BITMAPV2INFOHEADER52,
            56 => BitMapCoreHeaderType.BITMAPV3INFOHEADER56,
            108 => BitMapCoreHeaderType.BITMAPV4HEADER108,
            124 => BitMapCoreHeaderType.BITMAPV5HEADER124,
            _ => BitMapCoreHeaderType.INVALID
        };
    }

    private static short DIBGetPixelFormat(ref byte[] dibBytesPerPixel)
    {
        short pixelFormat = (short)((int)dibBytesPerPixel[1] << 8 | (int)dibBytesPerPixel[0]);

        return pixelFormat;
    }

    private static short DIBGetPlane(ref byte[] planesCount)
    {
        short planes = (short)((int)planesCount[1] << 8 | (int)planesCount[0]);

        return planes;
    }

    private static ushort DIBGetHeight(ref byte[] dibHeight)
    {
        ushort height = (ushort)((ushort)dibHeight[3] << 24 | (ushort)dibHeight[2] << 16 | (ushort)dibHeight[1] << 8 | (ushort)dibHeight[0]);

        return height;
    }

    private static ushort DIBGetWidth(ref byte[] dibWidth)
    {
        ushort width = (ushort)((ushort)dibWidth[3] << 24 | (ushort)dibWidth[2] << 16 | (ushort)dibWidth[1] << 8 | (ushort)dibWidth[0]);

        return width;
    }

    private static uint DIBGetSize(ref byte[] dibHeaderSize)
    {
        uint DIBSize = (uint)dibHeaderSize[3] << 24 | (uint)dibHeaderSize[2] << 16 | (uint)dibHeaderSize[1] << 8 | (uint)dibHeaderSize[0];
        return DIBSize;
    }

    private static int BMPGetColorOffset(ref byte[] filePixelOffset)
    {
        int resultStartAddress = 0;

        
        for (int i = 0; i < filePixelOffset.Length; i++)
        {
            resultStartAddress += filePixelOffset[i];
        }

        return resultStartAddress;
    }

    private static int[] BMPGetReservedFromHeader(ref byte[] fileReservedInfo)
    {
        int reservedFirst = (int)fileReservedInfo[1] << 8 | (int)fileReservedInfo[0];
        int reservedSecond = (int)fileReservedInfo[3] << 8 | (int)fileReservedInfo[2];

        int[] resultReserved = new int[2];
        resultReserved[0] = reservedFirst;
        resultReserved[1] = reservedSecond;

        return resultReserved;
    }

    internal static uint BMPGetFileSize(ref byte[] fileSizeHeaderInfo)
    {
        uint fileSize = (uint)fileSizeHeaderInfo[3] << 24
                        | (uint)fileSizeHeaderInfo[2] << 16
                        | (uint)fileSizeHeaderInfo[1] << 8
                        | (uint)fileSizeHeaderInfo[0];

        return fileSize; ;
    }

    internal SignatureTypes BMPCheckType(ref byte[] signature)
    {
        var signatureString = fileEncoding.GetString(signature) ?? String.Empty;
        if (string.IsNullOrEmpty(signatureString)) throw new Exception("wrong bmp signature");
        return signatureString switch
        {
            "BM" => SignatureTypes.BM,
            "BA" => SignatureTypes.BA,
            "CI" => SignatureTypes.CI,
            "CP" => SignatureTypes.CP,
            "IC" => SignatureTypes.IC,
            "PT" => SignatureTypes.PT,
            _ => SignatureTypes.Invalid,
        };
    }


    #endregion
}
