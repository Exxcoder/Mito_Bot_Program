using System;
using System.Collections.Generic;

namespace Cactask_API.Models;

public partial class Issueboard
{
    public long Issueboardid { get; set; }

    public long Projectid { get; set; }

    public long Userid { get; set; }

    public string Issueboardname { get; set; } = null!;

    public string? Issueboardavatar { get; set; }

    public long Issueboardresponsible { get; set; }

    public virtual User IssueboardresponsibleNavigation { get; set; } = null!;

    public virtual ICollection<Issue> Issues { get; set; } = new List<Issue>();

    public virtual Project Project { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
