using OpenCvSharp;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MyNamespace
{
    public static class BmpToJpegConverter
    {
        public static void Convert(string bmpFilePath, string jpegFilePath, int quality = 95)
        {
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
            Cv2.ImWrite(jpegFilePath, finalJpegMat, new ImageEncodingParam(ImwriteFlags.JpegQuality, quality));

            using (var jpegMat = Cv2.ImRead(jpegFilePath))
            {
                if (jpegMat.Empty())
                {
                    throw new Exception("Output JPEG image is invalid");
                }

                if (bmpMat.Size() != jpegMat.Size())
                {
                    throw new Exception("Image size does not match after conversion");
                }
            }
        }

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
    }
}
