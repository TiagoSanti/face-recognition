namespace DBContext;

public class User
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Surname { get; set; }

    public Class Fkclass { get; set; } = new();

    public virtual ICollection<Image> Images { get; set; } = new List<Image>();
}
