using System;
using System.Collections.Generic;

namespace Cactask_API.Models;

public partial class Issuescomment
{
    public long Issuecommentid { get; set; }

    public long Issuecommentsender { get; set; }

    public long Issueid { get; set; }

    public string Issuecommenttext { get; set; } = null!;

    public string? Issuecommentvideo { get; set; }

    public string? Issuecommentphoto { get; set; }

    public DateTime? Issuecommentcreatedat { get; set; }

    public bool? Issuecommentisdeleted { get; set; }

    public virtual ICollection<Commentreciefe> CommentreciefeCommentchildNavigations { get; set; } = new List<Commentreciefe>();

    public virtual ICollection<Commentreciefe> CommentreciefeCommentparentNavigations { get; set; } = new List<Commentreciefe>();

    public virtual Issue Issue { get; set; } = null!;

    public virtual User IssuecommentsenderNavigation { get; set; } = null!;
}
