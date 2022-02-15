using System.Drawing;
using Pnt = System.Drawing.Point;
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

            OpenAndTest(videoCapture, cameraModule, model);
        }

        private static void OpenAndTest(VideoCapture videoCapture, CameraModule cameraModule, Model model)
        {
            while (Window.WaitKey(10) != 27) // Esc
            {
                Mat mat = videoCapture.RetrieveMat();

                Bitmap bitmap = MatToBitmap(mat);

                bitmap = TestImage(bitmap, model);

                mat = BitmapToMat(bitmap);

                Cv2.ImShow("Image Show", mat);
            }
        }

        private static Bitmap MatToBitmap(Mat mat)
        {
            return OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mat);
        }

        private static Mat BitmapToMat(Bitmap bitmap)
        {
            return OpenCvSharp.Extensions.BitmapConverter.ToMat(bitmap);
        }

        private static Bitmap TestImage(Bitmap unknownBitmap, Model model)
        {
            using (var unknownImage = FaceRecognition.LoadImage(unknownBitmap))
            {
                var faceLocations = _faceRecognition.FaceLocations(unknownImage, 0, model).ToArray();
                
                Bitmap bitmap = unknownImage.ToBitmap();

                if (faceLocations.Count() == 0)
                {
                    Console.WriteLine("None");
                }
                else
                {
                    foreach (var faceLocation in faceLocations)
                    {
                        DrawRect(bitmap, faceLocation);
                        Console.WriteLine($"{faceLocation.Top}, " +
                                          $"{faceLocation.Right}, {faceLocation.Bottom},{faceLocation.Left}");
                    }
                }

                return bitmap;
            }
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