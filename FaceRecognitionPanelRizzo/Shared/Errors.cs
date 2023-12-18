namespace face_recognition.Shared;

public class MyErrors{
    public static readonly Dictionary < int, string > Codes_errors = new Dictionary < int, string > (){
        { 200, "Request OK"},
        { 210, "User added"},
        { 211, "User already exist"},
        { 212, "Invalid name"},

        { 403, "Permission denied"},
        { 404, "Page doesn't found" }
    };
}