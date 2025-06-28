using System;
using System.Collections.Generic;

namespace Cactask_API.Models;

public partial class Project
{
    public long Projectid { get; set; }

    public string Projectname { get; set; } = null!;

    public string? Projectbio { get; set; }

    public string? Projectcountry { get; set; }

    public string? Projecttown { get; set; }

    public DateOnly? Projectfoundeddate { get; set; }

    public string? Projectlogo { get; set; }

    public string? Projectphone { get; set; }

    public string? Projectemail { get; set; }

    public DateTime? Projectcreatedat { get; set; }

    public bool? Projectisdeleted { get; set; }

    public virtual ICollection<Issueboard> Issueboards { get; set; } = new List<Issueboard>();

    public virtual ICollection<Team> Teams { get; set; } = new List<Team>();
}
