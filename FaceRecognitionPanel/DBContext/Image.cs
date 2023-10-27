using System;
using System.Collections.Generic;

namespace DBContext;

public partial class Image
{
    public long PkImage { get; set; }

    public long FkUser { get; set; }

    public string? ImagePath { get; set; }

    public virtual User FkUserNavigation { get; set; } = null!;
}
