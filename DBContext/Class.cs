using System;
using System.Collections.Generic;

namespace DBContext;

public partial class Class
{
    public long PkClass { get; set; }

    public string? ClassName { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
