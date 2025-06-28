using System;
using System.Collections.Generic;

namespace Cactask_API.Models;

public partial class Issue
{
    public long Issueid { get; set; }

    public string Issuename { get; set; } = null!;

    public long? Issueboardid { get; set; }

    public string? Issuedescription { get; set; }

    public string? Issuephotos { get; set; }

    public string? Issuevideo { get; set; }

    public long Issueavtor { get; set; }

    public long? Issueexecutor { get; set; }

    public string? Issuepriority { get; set; }

    public DateTime? Issuedeadline { get; set; }

    public long? Issuetype { get; set; }

    public bool? Issueisdeleted { get; set; }

    public virtual ICollection<Historyissue> Historyissues { get; set; } = new List<Historyissue>();

    public virtual Issueboard? Issueboard { get; set; }

    public virtual User? IssueexecutorNavigation { get; set; }

    public virtual ICollection<Issuerelation> IssuerelationChildissues { get; set; } = new List<Issuerelation>();

    public virtual ICollection<Issuerelation> IssuerelationParentissues { get; set; } = new List<Issuerelation>();

    public virtual ICollection<Issuescomment> Issuescomments { get; set; } = new List<Issuescomment>();

    public virtual Issuestype? IssuetypeNavigation { get; set; }
}
