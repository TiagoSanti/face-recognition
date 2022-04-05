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
        public static int Main()
        {
            FaceRecognition? faceRecognition = FaceRecognition.Create(Path.GetFullPath("models"));
            List<Person> people = new();

            Console.Write("1. Load existing encodings\n" +
                            "2. Encode database and reload existing encodings\n" +
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
                    Console.Write("\nChoose model to encode:\n" +
                        "1. Hog\n" +
                        "2. Cnn\n" +
                        "3. Custom model\n" +
                        "Option: ");
                    Model modelOption;
                    switch (int.Parse(Console.ReadLine()))
                    {
                        case 1:
                            modelOption = Model.Hog;
                            break;

                        case 2:
                            modelOption = Model.Cnn;
                            break;

                        case 3:
                            modelOption = Model.Custom;
                            break;

                        default:
                            Console.WriteLine("Invalid option, shutting down program..");
                            return 0;
                    }
                    Console.WriteLine("\nStarting encoding..");
                    ReencodePeopleImages(people, modelOption);
                    break;

                default:
                    Console.WriteLine("Invalid option, shutting down program..");
                    return 0;
            };
            watch.Stop();
            Console.Write("\n----- PEOPLE ENCODINGS LOADED ----- {0:N2} seconds to load", watch.Elapsed.TotalSeconds);
            Console.WriteLine();
            foreach (Person personInfo in people)
            {
                Console.Write(personInfo.ToString());
            }

            Console.Write("\nPress any key to start camera and recognition.");
            Console.ReadKey();
            Console.Write("Starting camera..");
            VideoCapture videoCapture = new(0);

            string modelsDirectory = @".\models\";
            Enum.TryParse(modelsDirectory, true, out Model model);

            OpenAndDetect(faceRecognition, videoCapture, model, people);

            Cv2.DestroyAllWindows();
            return 0;
        }

        private static void LoadExistingEncodings(List<Person> people)
        {
            using var faceRecognition = FaceRecognition.Create(Path.GetFullPath("models"));
            Person? person;
            bool personInstanceAlreadyExists = false;

            string encodingsPath = @".\data\encodings";
            string knownEncodingPath = encodingsPath + @"\known";
            try
            {
                string[] peopleEncodingDir = Directory.GetDirectories(knownEncodingPath);

                Console.WriteLine(peopleEncodingDir.Length + " people encodings directories where found.");

                foreach (string personEncodingDir in peopleEncodingDir)
                {
                    string[] personEncodingFiles = Directory.GetFiles(personEncodingDir);
                    string personName = personEncodingDir.Split(Path.DirectorySeparatorChar).Last();

                    person = people.Where(x => x.Name.Contains(personName)).FirstOrDefault();

                    if (person != null)
                    {
                        Console.WriteLine(person.Name);
                        personInstanceAlreadyExists = true;
                    }
                    else
                    {
                        person = new(personName);
                    }

                    foreach (string encodingFile in personEncodingFiles)
                    {
                        FaceEncoding? encoding = DeserializeEncoding(encodingFile);
                        if (encoding != null)
                        {
                            person.AddEncoding(encoding);
                        }
                    }

                    if (personInstanceAlreadyExists == false)
                    {
                        people.Add(person);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\nSkipping task..");
            }
        }

        public static void ReencodePeopleImages(List<Person> people, Model model)
        {
            using var faceRecognition = FaceRecognition.Create(Path.GetFullPath("models"));
            Person person;
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
                    string[] personEncodingsFilesFull;
                    List<string> personEncodingsFiles = new();

                    bool encodingsAlreadyExisted = false;
                    if (Directory.Exists(personEncodingsDir))
                    {
                        personEncodingsFilesFull = Directory.GetFiles(personEncodingsDir);
                        encodingsAlreadyExisted = true;

                        foreach (string personEncodingsFileFull in personEncodingsFilesFull)
                        {
                            personEncodingsFiles.Add(Path.GetFileName(personEncodingsFileFull));
                        }
                    }

                    person = new Person(personName);

                    Console.Write("\nStarting in " + person.Name + "'s directory.. ");
                    string[] personImages = Directory.GetFiles(personDir);
                    Console.WriteLine(personImages.Length + " images where found. Starting encoding..");

                    var totalEncodingTime = Stopwatch.StartNew();
                    foreach (string personImage in personImages)
                    {
                        string imageFile = personImage.Split(Path.DirectorySeparatorChar).Last();
                        Console.Write("Encoding faces in image " + imageFile + ".. ");
                        if (encodingsAlreadyExisted && CheckIfImageEncodingExists(personEncodingsFiles, modelName, personImage))
                        {
                            Console.WriteLine(modelName + " model encoding already exists, skipping to next..");
                        }
                        else
                        {
                            IEnumerable<FaceEncoding> facesEncodings = EncodeImage(personImage, model, faceRecognition);
                            UpdateEncodingDatabase(facesEncodings, person, personImage, modelName);
                        }

                    }
                    people.Add(person);
                    totalEncodingTime.Stop();
                    Console.WriteLine("Finished encoding in " + person.Name + "'s directory. Time to complete: {0:N2} seconds", totalEncodingTime.Elapsed.TotalSeconds);
                }
            }

            LoadExistingEncodings(people);
        }

        private static void UpdateEncodingDatabase(IEnumerable<FaceEncoding> facesEncodings, Person person, string personImage, string modelName)
        {
            if (facesEncodings.Any())
            {
                foreach (FaceEncoding faceEncoding in facesEncodings)
                {
                    person.AddEncoding(faceEncoding);

                    Console.Write("Adding " + person.Name + "'s encoding file..");
                    var watch = Stopwatch.StartNew();
                    string imageFile = personImage.Split(Path.DirectorySeparatorChar).Last();
                    string imageFileWithoutExtension = Path.GetFileNameWithoutExtension(imageFile);
                    UpdatePersonEncodingFile(faceEncoding, person, modelName, imageFileWithoutExtension);
                    watch.Stop();
                    Console.Write(" -> " + watch.ElapsedMilliseconds + " ms to complete\n");
                }
            }
        }

        public static IEnumerable<FaceEncoding> EncodeImage(string personImage, Model model, FaceRecognition faceRecognition)
        {
            var personLoadedImage = FaceRecognition.LoadImageFile(personImage);

            var singleEncodingTime = Stopwatch.StartNew();
            IEnumerable<FaceEncoding> facesEncodings = faceRecognition.FaceEncodings(personLoadedImage, model: model, predictorModel: PredictorModel.Large);
            singleEncodingTime.Stop();
            Console.WriteLine("Time took to complete: {0:N2} min", singleEncodingTime.Elapsed.TotalMinutes);

            return facesEncodings;
        }

        public static bool CheckIfImageEncodingExists(List<string> personEncodingsFiles, string modelName, string personImage)
        {
            return personEncodingsFiles.Contains(Path.GetFileNameWithoutExtension(personImage) + "_" + modelName + ".encoding");
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

            SerializeEncoding(personEncodingsFilesPath + imageFileName + "_" + modelName + ".encoding", encoding);
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

        public static Mat DetectFaces(FaceRecognition faceRecognition, Bitmap unknownBitmap, Model model, List<Person> people)
        {
            var unknownImage = FaceRecognition.LoadImage(unknownBitmap);
            Location[] faceLocations = faceRecognition.FaceLocations(unknownImage, 0, Model.Hog).ToArray();

            Bitmap bitmap = unknownImage.ToBitmap();
            Mat mat = BitmapToMat(bitmap);

            if (faceLocations.Length > 0)
            {
                List<FaceEncoding> faceEncodings = (List<FaceEncoding>)faceRecognition.FaceEncodings(unknownImage, faceLocations, model: Model.Hog, predictorModel: PredictorModel.Large);

                RecognizeFaces(faceEncodings, faceLocations, people, mat);
                DrawRect(mat, faceLocations);
            }

            return mat;
        }

        public static void RecognizeFaces(List<FaceEncoding> faceEncodings, Location[] faceLocations, List<Person> people, Mat mat)
        {
            var index = 0;

            while (index < faceEncodings.Count)
            {
                FaceEncoding encoding = faceEncodings[index];
                Location faceLocation = faceLocations[index];

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
                        Console.WriteLine("Best match distance person: " +
                        bestAvgMatchPerson.Name +
                        "\nWith average: " + bestAvgDistance +
                        "\nAnd minimal: " + minDistance +
                        "\n--------------------------------------------------");
                    }
                    else
                    {
                        Console.WriteLine("Best average distance match person: " +
                        bestAvgMatchPerson.Name +
                        "\nWith average: " + bestAvgDistance +
                        "\nAnd best minimal distance match person: " +
                        minDistancePerson.Name +
                        "\n--------------------------------------------------");
                    }
                }

                if (bestAvgMatchPerson != null)
                {
                    DrawName(mat, bestAvgMatchPerson, faceLocation);
                }

                index++;
            }
        }

        public static void SerializeEncoding(string fileName, FaceEncoding encoding)
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(fileName, FileMode.Create);

            formatter.Serialize(stream, encoding);
            stream.Close();
        }

        public static FaceEncoding? DeserializeEncoding(string fileName)
        {
            if (File.Exists(fileName))
            {
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(fileName, FileMode.Open);
                FaceEncoding encoding = (FaceEncoding)formatter.Deserialize(stream);
                stream.Close();

                return encoding;
            }

            return null;
        }

        public static Bitmap MatToBitmap(Mat mat)
        {
            return OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mat);
        }

        public static Mat BitmapToMat(Bitmap bitmap)
        {
            return OpenCvSharp.Extensions.BitmapConverter.ToMat(bitmap);
        }

        public static void DrawRect(Mat mat, Location[] faceLocations)
        {
            foreach (Location faceLocation in faceLocations)
            {
                Cv2.Rectangle(mat,
                new OpenCvSharp.Point(faceLocation.Left, faceLocation.Top),
                new OpenCvSharp.Point(faceLocation.Right, faceLocation.Bottom),
                Scalar.Red,
                2);
            }
        }

        public static void DrawName(Mat mat, Person person, Location faceLocation)
        {
            Cv2.Rectangle(mat,
                new OpenCvSharp.Point(faceLocation.Left, faceLocation.Bottom),
                new OpenCvSharp.Point(faceLocation.Right, faceLocation.Bottom + 20),
                Scalar.Red,
                -1);
            mat.PutText(person.Name, new OpenCvSharp.Point(faceLocation.Left + 3, faceLocation.Bottom + 15), fontFace: HersheyFonts.HersheyDuplex, fontScale: 0.5, color: Scalar.White);
        }
    }
}