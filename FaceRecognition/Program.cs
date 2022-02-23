using System.Drawing;
using Pnt = System.Drawing.Point;
using FaceRecognitionDotNet;
using OpenCvSharp;

namespace FaceRec
{
    public class Program
    {
        private static readonly FaceRecognition? _faceRecognition = FaceRecognition.Create(Path.GetFullPath("models"));
        
        public static void Main()
        {
            var people = LoadPeopleEncodings();
            VideoCapture videoCapture = new(0); // '0' to default system camera device

            string modelsDirectory = @".\models\";
            Enum.TryParse<Model>(modelsDirectory, true, out var model);

            OpenAndTest(videoCapture, model);

            Cv2.DestroyAllWindows();
        }

        public static List<Person> LoadPeopleEncodings()
        {
            List<Person> people = new();
            Person person;

            var imagesPath = @".\images";
            var knownImages = imagesPath + @"\known";
            var unknownEncodings = imagesPath + @"\unknown";

            var peopleDir = Directory.EnumerateDirectories(knownImages);

            if (peopleDir.Any())
            {
                foreach (var personDir in peopleDir)
                {
                    var personName = personDir.Split(Path.DirectorySeparatorChar).Last();
                    person = new Person(personName);

                    var personImages = Directory.GetFiles(personDir);

                    foreach (var personImage in personImages)
                    {
                        var personLoadedImage = FaceRecognition.LoadImageFile(personImage);

                        var facesEncodings = _faceRecognition.FaceEncodings(personLoadedImage);

                        if (facesEncodings.Any())
                        {
                            foreach (FaceEncoding faceEncoding in facesEncodings)
                            {
                                person.AddEncoding(faceEncoding);
                            }
                        }
                    }

                    people.Add(person);
                }
            }

            Console.WriteLine("----- PEOPLE ENCODINGS LOADED -----");
            foreach (var personInfo in people)
            {
                Console.WriteLine(personInfo.ToString());
            }

            return people;
        }

        private static void OpenAndTest(VideoCapture videoCapture, Model model)
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

                if (faceLocations.Count() > 0)
                {
                    foreach (var faceLocation in faceLocations)
                    {
                        DrawRect(bitmap, faceLocation);
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