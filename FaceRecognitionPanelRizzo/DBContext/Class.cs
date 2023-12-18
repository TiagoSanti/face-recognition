namespace DBContext;

public class Class
{
    public int ClassId { get; set; }

    public string? ClassName { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
