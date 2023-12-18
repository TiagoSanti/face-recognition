namespace DBContext;

public partial class SawUsers
{
    public int Id { get; set; }
    
    public User User { get; set; } = null!;
    
    public bool Seen { get; set; }
    
    public DateTime LastTimeSeen { get; set; }
}
