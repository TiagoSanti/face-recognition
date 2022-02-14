using System.Drawing;
using Pnt = System.Drawing.Point;
using System.Text.RegularExpressions;
using FaceRecognitionDotNet;
using OpenCvSharp;

namespace FaceDetectionCustom
{
    public class Program
    {
        private static FaceRecognition? _faceRecognition;

        public static void Main()
        {
            VideoCapture videoCapture = new VideoCapture(0); // '0' to default system camera device
            CameraModule cameraModule = new CameraModule();

            string imagesToCheckDirectory = @".\test_images\";
            string modelsDirectory = @".\models\";

            Enum.TryParse<Model>(modelsDirectory, true, out var model);

            _faceRecognition = FaceRecognition.Create(Path.GetFullPath("models"));

            //OpenAndTest(videoCapture, cameraModule);
            
            foreach (var imageFile in ImageFilesInFolder(imagesToCheckDirectory))
            {
                TestImage(imageFile, model);
            }
        }

        private static void OpenAndTest(VideoCapture videoCapture, CameraModule cameraModule)
        {
            while (Window.WaitKey(10) != 27) // Esc
            {
                Mat mat = videoCapture.RetrieveMat();

                

                Cv2.ImShow("Image Show", mat);
            }
        }

        public static IEnumerable<string> ImageFilesInFolder(string folder)
        {
            return Directory.GetFiles(folder)
                            .Where(s => Regex.IsMatch(Path.GetExtension(s), "(jpg|jpeg|png)$", RegexOptions.Compiled));
        }

        private static void TestImage(string imageToCheck, Model model)
        {
            using (var unknownImage = FaceRecognition.LoadImageFile(imageToCheck))
            {
                var faceLocations = _faceRecognition.FaceLocations(unknownImage, 0, model).ToArray();

                string fileName = Path.GetFileName(imageToCheck);
                string outputDirectory = @".\output\";

                if (faceLocations.Count() == 0)
                {
                    Console.WriteLine("None face located in " + fileName);
                }
                else
                {
                    Bitmap bitmap = unknownImage.ToBitmap();

                    foreach (var faceLocation in faceLocations)
                    {
                        DrawRect(bitmap, faceLocation);

                        try
                        {
                            SaveAndPrint(bitmap, outputDirectory, fileName, faceLocation);
                        }
                        catch (DirectoryNotFoundException)
                        {
                            Console.WriteLine("Creating directory {0}...", outputDirectory);
                            Directory.CreateDirectory(outputDirectory);

                            SaveAndPrint(bitmap, outputDirectory, fileName, faceLocation);
                        }
                    }                
                }
            }
        }

        public static void SaveAndPrint(Bitmap bitmap, string outputDirectory, string fileName, Location faceLocation)
        {
            bitmap.Save(outputDirectory + fileName);

            Console.WriteLine($"{fileName}, {faceLocation.Top}, " +
                $"{faceLocation.Right}, {faceLocation.Bottom},{faceLocation.Left}");
        }

        private static void DrawRect(Bitmap bitmap, Location faceLocation)
        {
            int left = faceLocation.Left;
            int top = faceLocation.Top;
            int right = faceLocation.Right;
            int bottom = faceLocation.Bottom;

            Pnt topLeft = new Pnt(left, top),
                bottomLeft = new Pnt(left, bottom),
                topRight = new Pnt(right, top),
                bottomRight = new Pnt(right, bottom);

            DrawLine(bitmap, topLeft, topRight);
            DrawLine(bitmap, topRight, bottomRight);
            DrawLine(bitmap, bottomRight, bottomLeft);
            DrawLine(bitmap, bottomLeft, topLeft);
        }

        private static void DrawLine(Bitmap bitmap, Pnt point1, Pnt point2)
        {
            Pen pen = new Pen(Color.Red, 3);

            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.DrawLine(pen, point1, point2);
            }
        }
    }
}