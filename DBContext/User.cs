using System;
using System.Collections.Generic;

namespace DBContext;

public partial class User
{
    public long PkUser { get; set; }

    public string? Name { get; set; }

    public string? Surname { get; set; }

    public long FkClass { get; set; }

    public virtual Class FkClassNavigation { get; set; } = null!;

    public virtual ICollection<Image> Images { get; set; } = new List<Image>();
}
