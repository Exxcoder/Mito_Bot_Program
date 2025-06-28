using System;
using System.Collections.Generic;

namespace Cactask_API.Models;

public partial class Commentreciefe
{
    public long Commentrecieveid { get; set; }

    public long Commentparent { get; set; }

    public long Commentchild { get; set; }

    public virtual Issuescomment CommentchildNavigation { get; set; } = null!;

    public virtual Issuescomment CommentparentNavigation { get; set; } = null!;
}
