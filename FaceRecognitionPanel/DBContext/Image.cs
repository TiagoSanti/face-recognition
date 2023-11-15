namespace DBContext;

public class Image
{
    public int ImageId { get; set; }

    public string? ImagePath { get; set; }

    public User FkUser {get; set;} = new();
}
