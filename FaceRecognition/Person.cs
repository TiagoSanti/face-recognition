using FaceRecognitionDotNet;

namespace FaceRec
{
    public class Person
    {
        public string Name { get; set; }
        public List<FaceEncoding>? FaceEncodings { get; set; }

        public Person(string name)
        {
            Name = name;
        }

        public void AddEncoding(FaceEncoding encoding)
        {
            if (FaceEncodings == null)
            {
                FaceEncodings = new List<FaceEncoding>();
            }
            FaceEncodings.Add(encoding);
        }

        public override string ToString()
        {
            return "Name: " + Name +
                "\nEncoding count: " + FaceEncondingsCount() + "\n";
        }

        public string FaceEncondingsCount()
        {
            if (FaceEncodings != null)
            {
                return FaceEncodings.Count.ToString();
            }
            else
            {
                return "" + 0;
            }
        }
    }
}
