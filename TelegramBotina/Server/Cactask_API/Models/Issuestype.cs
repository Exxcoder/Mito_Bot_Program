using System;
using System.Collections.Generic;

namespace Cactask_API.Models;

public partial class Issuestype
{
    public long Issuetypeid { get; set; }

    public string Issuetypename { get; set; } = null!;

    public virtual ICollection<Issue> Issues { get; set; } = new List<Issue>();
}
