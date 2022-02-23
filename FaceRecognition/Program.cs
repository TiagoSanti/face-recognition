using System.Drawing;
using FaceRecognitionDotNet;
using OpenCvSharp;

namespace FaceRec
{
    public class Program
    {
        public static void Main()
        {
            FaceRecognition? faceRecognition = FaceRecognition.Create(Path.GetFullPath("models"));
            var people = LoadPeopleEncodings();
            VideoCapture videoCapture = new(0);
            
            string modelsDirectory = @".\models\";
            Enum.TryParse<Model>(modelsDirectory, true, out var model);

            OpenAndDetect(faceRecognition, videoCapture, model, people);

            Cv2.DestroyAllWindows();
        }

        public static List<Person> LoadPeopleEncodings()
        {
            using var faceRecognition = FaceRecognition.Create(Path.GetFullPath("models"));
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

                        var facesEncodings = faceRecognition.FaceEncodings(personLoadedImage);

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

        public static void OpenAndDetect(FaceRecognition faceRecognition, VideoCapture videoCapture, Model model, List<Person> people)
        {
            while (Window.WaitKey(10) != 27) // Esc
            {
                Mat mat = videoCapture.RetrieveMat();
                Bitmap bitmap = MatToBitmap(mat);
                mat = DetectFaces(faceRecognition, bitmap, model, people);

                Cv2.ImShow("Image Show", mat);
            }
        }

        public static Bitmap MatToBitmap(Mat mat)
        {
            return OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mat);
        }

        public static Mat BitmapToMat(Bitmap bitmap)
        {
            return OpenCvSharp.Extensions.BitmapConverter.ToMat(bitmap);
        }

        public static Mat DetectFaces(FaceRecognition faceRecognition, Bitmap unknownBitmap, Model model, List<Person> people)
        {
            var unknownImage = FaceRecognition.LoadImage(unknownBitmap);
            var faceLocations = faceRecognition.FaceLocations(unknownImage, 0, model).ToArray();

            Bitmap bitmap = unknownImage.ToBitmap();
            Mat mat = BitmapToMat(bitmap);

            if (faceLocations.Length > 0)
            {
                var faceEncodings = faceRecognition.FaceEncodings(unknownImage, faceLocations);

                foreach (var faceLocation in faceLocations)
                {
                    RecognizeFaces(faceEncodings, people, mat, faceLocation);
                    DrawRect(mat, faceLocation);
                 }
            }

            return mat;
        }

        public static void RecognizeFaces(IEnumerable<FaceEncoding> faceEncodings, List<Person> people, Mat mat, Location faceLocation)
        {            
            foreach (var encoding in faceEncodings)
            {
                double bestAvgDistance = 1;
                Person bestAvgMatchPerson = null;

                double minDistance = 1;
                Person minDistancePerson = null;

                foreach (var person in people)
                {
                    var distances = FaceRecognition.FaceDistances(person.FaceEncodings, encoding);

                    var avgPersonDistance = distances.Average();
                    var minPersonDistance = distances.Min();

                    if (avgPersonDistance < bestAvgDistance)
                    {
                        bestAvgDistance = avgPersonDistance;
                        bestAvgMatchPerson = person;
                    }
                    if (minPersonDistance < minDistance)
                    {
                        minDistance = minPersonDistance;
                        minDistancePerson = person;
                    }
                }

                if (bestAvgMatchPerson != null && minDistancePerson != null)
                {
                    if (bestAvgMatchPerson.Equals(minDistancePerson))
                    {
                        Console.WriteLine("Best match distance person: \n" +
                        bestAvgMatchPerson.ToString() +
                        "With average: " + bestAvgDistance +
                        "\nAnd minimal: " + minDistance +
                        "\n--------------------------------------------------");
                    }
                    else
                    {
                        Console.WriteLine("Best average distance match person: \n" +
                        bestAvgMatchPerson.ToString() +
                        "With average: " + bestAvgDistance +
                        "\nAnd best minimal distance match person: " + 
                        minDistancePerson.ToString() +
                        "\n--------------------------------------------------");
                    }
                }

                DrawName(mat, bestAvgMatchPerson, faceLocation);
            }
        }

        public static void DrawRect(Mat mat, Location faceLocation)
        {
            Cv2.Rectangle(mat,
                new OpenCvSharp.Point(faceLocation.Left, faceLocation.Top),
                new OpenCvSharp.Point(faceLocation.Right, faceLocation.Bottom),
                Scalar.Red,
                2);
        }

        public static void DrawName(Mat mat, Person person, Location faceLocation)
        {
            mat.PutText(person.Name, new OpenCvSharp.Point(faceLocation.Left, faceLocation.Bottom+15), fontFace: HersheyFonts.HersheySimplex, fontScale: 0.5, color: Scalar.White);
        }
    }
}