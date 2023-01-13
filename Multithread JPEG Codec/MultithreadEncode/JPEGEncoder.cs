using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MultithreadEncodeOpenCV
{
    public static class BmpToJpegConverter
    {
        public static readonly Dictionary<string, string> imageSignature = new Dictionary<string, string>()
        { 
            { "GIF8", "GIF"},
            {"PNG", "PNG"},
            {"II", "TIFF"},
            { "MM", "TIFF"},
            {"ÿØÿ", "JPEG" },
            { Encoding.ASCII.GetString(new byte[]{0x0, 0x0, 0x1, 0x0}), "ICO" },
            {"CWS", "SWF" },
            {"FWS", "SWF" },
            {"", "" }
        };
        public static void ConvertWithMoreRegions(string bmpFilePath, string jpegFilePath, int quality = 95, int regionCount = 4)
        {
            if (regionCount < 1) throw new ArgumentOutOfRangeException("number of regions must be positive");
            if (!File.Exists(bmpFilePath))
            {
                throw new FileNotFoundException($"Input file not found: {bmpFilePath}");
            }

            if (File.Exists(jpegFilePath))
            {
                File.Delete(jpegFilePath);
            }

            var bmpMat = Cv2.ImRead(bmpFilePath);

            if (bmpMat.Empty())
            {
                throw new Exception("Input BMP image is invalid");
            }
            //implementing for more regions than threads
            int threadLoop = regionCount / Environment.ProcessorCount + 1;
            int remainedRegions = regionCount;
            int actualRegionIndex = 0;
            var jpegMatList = new List<Mat>();

            for (int loop = 0; loop < threadLoop && remainedRegions > 0; loop++)
            {
                int innerLoop = (Environment.ProcessorCount > remainedRegions) ? remainedRegions : Environment.ProcessorCount;
                var tasks = new Task<Mat>[innerLoop];
                for (int i = 0; i < innerLoop; i++)
                {
                    int threadIndex = actualRegionIndex++;
                    tasks[i] = Task.Run(() => ConvertRegionWithMoreRegions(bmpMat, threadIndex, regionCount));
                }
                Task.WaitAll(tasks);

                // Concatenate the converted regions into a single image
                
                for (int i = 0; i < tasks.Length; i++)
                {

                    jpegMatList.Add(tasks[i].Result);
                }
                remainedRegions -= innerLoop;
            }


            var finalJpegMat = new Mat();

            // Use the Cv2.VConcat method with two arguments: the list of Mat objects to concatenate and the output Mat object
            Cv2.HConcat(jpegMatList, finalJpegMat);

            // Save the final combined image to the specified file path
            Cv2.ImWrite(jpegFilePath, finalJpegMat, new ImageEncodingParam(ImwriteFlags.JpegQuality, quality));

        }


        public static void Convert(string bmpFilePath, string jpegFilePath, int quality = 95, int resetInterval = 0)
        {
            if (!File.Exists(bmpFilePath))
            {
                throw new FileNotFoundException($"Input file not found: {bmpFilePath}");
            }

            if (File.Exists(jpegFilePath))
            {
                File.Delete(jpegFilePath);
            }

            var stream = new FileStream(bmpFilePath, FileMode.Open);
            var buffer = new byte[stream.Length];

            if (stream.Length == 0) throw new Exception("cannot get file type");

            stream.Read(buffer, 0, buffer.Length);

            var charbuffer = ASCIIEncoding.ASCII.GetString(buffer);

            var bmpMat = Cv2.ImRead(bmpFilePath);

            if (bmpMat.Empty())
            {
                string invalidType = string.Empty;
                foreach (var keyval in imageSignature)
                {
                    if (charbuffer.Contains(keyval.Key))
                    {
                        invalidType = keyval.Value;
                        break;
                    }
                }
                
                throw new ArgumentException("Input BMP image is invalid", new Exception($"file signature: {string.Join(" ",charbuffer.Take(10))})"+ ((invalidType == string.Empty)?"":$"\n possible file type: {invalidType}")));
            }

            var tasks = new Task<Mat>[Environment.ProcessorCount];
            for (int i = 0; i < tasks.Length; i++)
            {
                int threadIndex = i;
                tasks[i] = Task.Run(() => ConvertRegion(bmpMat, threadIndex));
            }

            Task.WaitAll(tasks);

            // Concatenate the converted regions into a single image
            var jpegMatList = new List<Mat>();
            for (int i = 0; i < tasks.Length; i++)
            {
                
                jpegMatList.Add(tasks[i].Result);
            }
            var finalJpegMat = new Mat();

            // Use the Cv2.VConcat method with two arguments: the list of Mat objects to concatenate and the output Mat object
            Cv2.HConcat(jpegMatList, finalJpegMat);

            // Save the final combined image to the specified file path
            Cv2.ImWrite(jpegFilePath, finalJpegMat, new ImageEncodingParam[]
                {
                    new ImageEncodingParam(ImwriteFlags.JpegQuality, quality),
                    new ImageEncodingParam(ImwriteFlags.JpegRstInterval, resetInterval)
                }
            );
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static Task<Mat> ConvertRegion(Mat bmpMat, int threadIndex)
        {
            var region = CalculateRegion(bmpMat.Size(), threadIndex);

            // Convert the region and return the result as a Task
            return Task.Run(() =>
            {
                var jpegMat = new Mat();
                Cv2.ImEncode(".jpg", bmpMat[region], out byte[] buf);
                jpegMat = Cv2.ImDecode(buf, ImreadModes.Unchanged);
                return jpegMat;
            });
        }

        private static Rect CalculateRegion(Size bmpSize, int threadIndex)
        {
            int regionWidth = bmpSize.Width / Environment.ProcessorCount;
            int remainder = bmpSize.Width % Environment.ProcessorCount;
            int regionX = (regionWidth * threadIndex) + Math.Min(threadIndex, remainder);
            int regionWidthWithRemainder = regionWidth + (threadIndex < remainder ? 1 : 0);
            return new Rect(regionX, 0, regionWidthWithRemainder, bmpSize.Height);
        }


        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static Task<Mat> ConvertRegionWithMoreRegions(Mat bmpMat, int threadIndex, int regionCount)
        {
            var region = CalculateRegionWithMoreRegions(bmpMat.Size(), threadIndex, regionCount);

            // Convert the region and return the result as a Task
            return Task.Run(() =>
            {
                var jpegMat = new Mat();
                Cv2.ImEncode(".jpg", bmpMat[region], out byte[] buf);
                jpegMat = Cv2.ImDecode(buf, ImreadModes.Unchanged);
                return jpegMat;
            });
        }

        private static Rect CalculateRegionWithMoreRegions(Size bmpSize, int threadIndex, int regionCount)
        {
            int regionWidth = bmpSize.Width / regionCount;
            int remainder = bmpSize.Width % regionCount;
            int regionX = (regionWidth * threadIndex) + Math.Min(threadIndex, remainder);
            int regionWidthWithRemainder = regionWidth + (threadIndex < remainder ? 1 : 0);
            return new Rect(regionX, 0, regionWidthWithRemainder, bmpSize.Height);
        }
    }
}
