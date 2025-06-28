using System;
using System.Collections.Generic;

namespace Cactask_API.Models;

public partial class Team
{
    public long Teamid { get; set; }

    public long? Teamusers { get; set; }

    public long Projectid { get; set; }

    public int Teamsize { get; set; }

    public int Teamfullness { get; set; }

    public DateTime? Teamcreatedat { get; set; }

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
