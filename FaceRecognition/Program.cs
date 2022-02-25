using System.Drawing;
using System.Diagnostics;
using FaceRecognitionDotNet;
using OpenCvSharp;
using System.Text.Json;
using System.Linq;

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
            Console.Write("\n----- PEOPLE ENCODINGS LOADED ----- " + watch.ElapsedMilliseconds + " ms to load");
            Console.WriteLine();
            foreach (Person personInfo in people)
            {
                Console.WriteLine(personInfo.ToString());
            }

            Console.Write("\nPress any key to start camera and recognition.");
            Console.ReadKey();

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
            string[] peopleEncodingDir = Directory.GetDirectories(knownEncodingPath);

            Console.WriteLine(peopleEncodingDir.Length + " people encodings directories where found.");
            if (peopleEncodingDir.Any())
            {
                foreach (string personEncodingDir in peopleEncodingDir)
                {
                    string[] personEncodingFiles = Directory.GetFiles(personEncodingDir);
                    person = new(personEncodingDir.Split(Path.DirectorySeparatorChar).Last());

                    foreach (var encodingFile in personEncodingFiles)
                    {
                        FaceEncoding? encoding = DeserializeEncoding(encodingFile);
                        if (encoding != null)
                        {
                            person.AddEncoding(encoding);
                        }
                    }

                    people.Add(person);
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
            Model model = Model.Cnn;
            string modelName = ModelName(model);

            string knownImagesPath = @".\data\images\known";
            string knownEncodingsPath = @".\data\encodings\known";

            var peopleDir = Directory.EnumerateDirectories(knownImagesPath);

            Console.WriteLine(peopleDir.Count() + " people directories where found.");

            if (peopleDir.Any())
            {
                foreach (string personDir in peopleDir)
                {
                    string personName = personDir.Split(Path.DirectorySeparatorChar).Last();
                    string personEncodingsDir = knownEncodingsPath + @"\" + personName;
                    bool encodingsAlreadyExisted = false;
                    if (Directory.Exists(personEncodingsDir))
                    {
                        encodingsAlreadyExisted = true;
                    }

                    person = new Person(personName);

                    Console.Write("\nStarting in " + person.Name + "'s directory.. ");
                    string[] personImages = Directory.GetFiles(personDir);
                    Console.WriteLine(personImages.Length + " images where found. Starting encoding..");

                    var totalEncodingTime = Stopwatch.StartNew();
                    foreach (string personImage in personImages)
                    {
                        if (encodingsAlreadyExisted)
                        {
                            string[] personEncodingsFilesFull = Directory.GetFiles(personEncodingsDir);
                            List<string> personEncodingsFiles = new();
                            
                            foreach (var personEncodingsFile in personEncodingsFilesFull)
                            {
                                personEncodingsFiles.Add(Path.GetFileName(personEncodingsFile));
                            }

                            if (personEncodingsFiles.Any((Path.GetFileName(personImage) + "_" + modelName + ".json").Contains))
                            {
                                Console.WriteLine("There is already an " + modelName + " model encoding file in this directory, skipping to next..");
                            }
                            else
                            {
                                var personLoadedImage = FaceRecognition.LoadImageFile(personImage);

                                var singleEncodingTime = Stopwatch.StartNew();
                                Console.Write("Encoding faces in image " + personImage + ".. ");
                                var facesEncodings = faceRecognition.FaceEncodings(personLoadedImage, model: model, predictorModel: PredictorModel.Large);
                                singleEncodingTime.Stop();
                                Console.WriteLine("Time took to complete: " + singleEncodingTime.Elapsed.TotalMinutes + " min");

                                if (facesEncodings.Any())
                                {
                                    foreach (FaceEncoding faceEncoding in facesEncodings)
                                    {
                                        person.AddEncoding(faceEncoding);

                                        Console.Write("Adding encoding file..");
                                        var watch = Stopwatch.StartNew();
                                        UpdatePersonEncodingFile(faceEncoding, person, modelName, personImage.Split(Path.DirectorySeparatorChar).Last());
                                        watch.Stop();
                                        Console.Write(" -> " + watch.ElapsedMilliseconds + " ms to complete\n\n");
                                    }
                                }
                            }
                        }
                        else
                        {
                            var personLoadedImage = FaceRecognition.LoadImageFile(personImage);

                            var singleEncodingTime = Stopwatch.StartNew();
                            Console.Write("Encoding faces in image " + personImage + ".. ");
                            var facesEncodings = faceRecognition.FaceEncodings(personLoadedImage, model: model, predictorModel: PredictorModel.Large);
                            singleEncodingTime.Stop();
                            Console.WriteLine("Time took to complete: " + singleEncodingTime.Elapsed.TotalMinutes + " min");

                            if (facesEncodings.Any())
                            {
                                foreach (FaceEncoding faceEncoding in facesEncodings)
                                {
                                    person.AddEncoding(faceEncoding);

                                    Console.Write("Adding " + person.Name + "'s encoding file..");
                                    var watch = Stopwatch.StartNew();
                                    UpdatePersonEncodingFile(faceEncoding, person, modelName, personImage.Split(Path.DirectorySeparatorChar).Last());
                                    watch.Stop();
                                    Console.Write(" -> " + watch.ElapsedMilliseconds + " ms to complete\n");
                                }
                            }
                        }

                    }
                    people.Add(person);
                    totalEncodingTime.Stop();
                    Console.WriteLine("Finished encoding in " + person.Name + "'s directory. Time to complete: " + totalEncodingTime.Elapsed.TotalMinutes + " min");
                }
            }
        }

        public static string ModelName(Model model)
        {
            string modelName;
            if (model.Equals(Model.Hog))
            {
                modelName = "hog";
            }
            else if (model.Equals(Model.Cnn))
            {
                modelName = "cnn";
            }
            else
            {
                modelName = "custom";
            }

            return modelName;
        }

        public static void UpdatePersonEncodingFile(FaceEncoding encoding, Person person, string modelName, string imageFileName)
        {
            string personEncodingsFilesPath = @".\data\encodings\known\" + person.Name + @"\";

            if (Directory.Exists(personEncodingsFilesPath) == false)
            {
                Directory.CreateDirectory(personEncodingsFilesPath);
            }

            SerializeEncoding(personEncodingsFilesPath + imageFileName + "_" + modelName + ".json", encoding);
        }

        public static void SerializeEncoding(string fileName, FaceEncoding encoding)
        {
            string jsonEncoding = JsonSerializer.Serialize(encoding);
            File.WriteAllText(fileName, jsonEncoding);
        }

        public static FaceEncoding? DeserializeEncoding(string fileName)
        {
            if (File.Exists(fileName))
            {
                var encoding = JsonSerializer.Deserialize<FaceEncoding>(fileName);

                if (encoding != null)
                {
                    return encoding;
                }
            }

            return null;
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
            var faceLocations = faceRecognition.FaceLocations(unknownImage, 0, Model.Hog).ToArray();

            Bitmap bitmap = unknownImage.ToBitmap();
            Mat mat = BitmapToMat(bitmap);

            if (faceLocations.Length > 0)
            {
                IEnumerable<FaceEncoding> faceEncodings = faceRecognition.FaceEncodings(unknownImage, faceLocations, model: Model.Hog, predictorModel: PredictorModel.Large);

                foreach (Location faceLocation in faceLocations)
                {
                    RecognizeFaces(faceEncodings, people, mat, faceLocation);
                    DrawRect(mat, faceLocation);
                }
            }

            return mat;
        }

        public static void RecognizeFaces(IEnumerable<FaceEncoding> faceEncodings, List<Person> people, Mat mat, Location faceLocation)
        {
            foreach (FaceEncoding encoding in faceEncodings)
            {
                double bestAvgDistance = 1;
                Person? bestAvgMatchPerson = null;

                double minDistance = 1;
                Person? minDistancePerson = null;

                foreach (Person person in people)
                {
                    IEnumerable<double> distances = FaceRecognition.FaceDistances(person.FaceEncodings, encoding);

                    double avgPersonDistance = distances.Average();
                    double minPersonDistance = distances.Min();

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

                if (bestAvgMatchPerson != null)
                {
                    DrawName(mat, bestAvgMatchPerson, faceLocation);
                }
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