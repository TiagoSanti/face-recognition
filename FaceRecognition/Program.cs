using System.Drawing;
using System.Diagnostics;
using FaceRecognitionDotNet;
using OpenCvSharp;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
namespace FaceRec
{
    public class Program
    {
        public static void Main()
        {
            FaceRecognition? faceRecognition = FaceRecognition.Create(Path.GetFullPath("models"));
            List<Person> people = new();

            Console.Write("1. Load existing encodings\n" +
                            "2. Re-encode people images\n" +
                            "Option: ");
            int option = int.Parse(Console.ReadLine());
            var watch = Stopwatch.StartNew();
            switch (option)
            {
                case 1:
                    Console.WriteLine("\nStarting loading..");
                    LoadExistingEncodings(people);
                    break;

                case 2:
                    Console.WriteLine("\nStarting encoding..");
                    ReencodePeopleImages(people);
                    break;
            };
            watch.Stop();
            Console.Write("----- PEOPLE ENCODINGS LOADED ----- " + watch.ElapsedMilliseconds + " ms to load");
            Console.WriteLine();
            foreach (Person personInfo in people)
            {
                Console.WriteLine(personInfo.ToString());
            }

            Console.WriteLine("Press enter to start camera and recognition.");
            
            VideoCapture videoCapture = new(0);

            string modelsDirectory = @".\models\";
            Enum.TryParse(modelsDirectory, true, out Model model);

            OpenAndDetect(faceRecognition, videoCapture, model, people);

            Cv2.DestroyAllWindows();
        }

        private static void LoadExistingEncodings(List<Person> people)
        {
            using var faceRecognition = FaceRecognition.Create(Path.GetFullPath("models"));
            Person person;

            string encodingsPath = @".\data\encodings";
            string knownEncodingPath = encodingsPath + @"\known";
            string[] peopleEncodingFiles = Directory.GetFiles(knownEncodingPath);

            Console.WriteLine(peopleEncodingFiles.Length + " people encodings files where found.");
            if (peopleEncodingFiles.Any())
            {
                foreach (string personEncodingFile in peopleEncodingFiles)
                {
                    person = new(personEncodingFile.Split(Path.DirectorySeparatorChar).Last());
                    string[] encondings = File.ReadAllLines(personEncodingFile);
                    Console.WriteLine("\nShowing " + person.Name + "'s encondings:");
                    foreach (string encoding in encondings)
                    {
                        Console.WriteLine(encoding);
                    }
                }
            }
            else
            {
                Console.WriteLine("Skipping task..");
            }
        }

        public static void ReencodePeopleImages(List<Person> people)
        {
            using var faceRecognition = FaceRecognition.Create(Path.GetFullPath("models"));
            Person person;

            string imagesPath = @".\data\images";
            string knownImagesPath = imagesPath + @"\known";
            var peopleDir = Directory.EnumerateDirectories(knownImagesPath);

            Console.WriteLine(peopleDir.Count() + " people directories where found.");

            if (peopleDir.Any())
            {
                foreach (string personDir in peopleDir)
                {
                    string personName = personDir.Split(Path.DirectorySeparatorChar).Last();
                    person = new Person(personName);
                    Console.Write("\nStarting in " + person.Name + "'s directory.. ");

                    string[] personImages = Directory.GetFiles(personDir);
                    Console.WriteLine(personImages.Length + " images where found. Starting encoding..");

                    var totalEncodingTime = Stopwatch.StartNew();
                    foreach (string personImage in personImages)
                    {
                        var personLoadedImage = FaceRecognition.LoadImageFile(personImage);

                        var singleEncodingTime = Stopwatch.StartNew();
                        Console.Write("Encoding faces in image " + personImage + ".. ");
                        var facesEncodings = faceRecognition.FaceEncodings(personLoadedImage, model: Model.Hog, predictorModel: PredictorModel.Large);
                        singleEncodingTime.Stop();
                        Console.WriteLine("Time took to complete: " + singleEncodingTime.Elapsed.TotalMinutes + " min");

                        if (facesEncodings.Any())
                        {
                            foreach (FaceEncoding faceEncoding in facesEncodings)
                            {
                                person.AddEncoding(faceEncoding);
                            }
                        }
                    }
                    totalEncodingTime.Stop();
                    Console.WriteLine("Finished encoding in " + person.Name + "'s directory. Time to complete: " + totalEncodingTime.Elapsed.TotalMinutes + " min");

                    people.Add(person);

                    Console.WriteLine("Updating " + person.Name + "'s encoding file..");
                    var watch = Stopwatch.StartNew();
                    UpdatePersonEncodingFile(person);
                    watch.Stop();
                    Console.Write(" -> " + watch.ElapsedMilliseconds + " ms to complete\n");
                }
            }
        }

        private static void UpdatePersonEncodingFile(Person person)
        {
            IFormatter formatter = new BinaryFormatter();
            string personEncodingFilePath = @".\data\encodings\known\" + person.Name + ".encodings";

            if (File.Exists(personEncodingFilePath) == false)
            {
                
            }
            else
            {
            
            }
        }

        private static void SerializeEncoding(string fileName, IFormatter formatter, )
        {

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
            var faceLocations = faceRecognition.FaceLocations(unknownImage, 0, Model.Cnn).ToArray();

            Bitmap bitmap = unknownImage.ToBitmap();
            Mat mat = BitmapToMat(bitmap);

            if (faceLocations.Length > 0)
            {
                var faceEncodings = faceRecognition.FaceEncodings(unknownImage, faceLocations, model: Model.Hog, predictorModel: PredictorModel.Large);

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
            mat.PutText(person.Name, new OpenCvSharp.Point(faceLocation.Left, faceLocation.Bottom + 15), fontFace: HersheyFonts.HersheySimplex, fontScale: 0.5, color: Scalar.White);
        }
    }
}