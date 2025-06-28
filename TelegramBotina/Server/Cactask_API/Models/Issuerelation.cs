using System;
using System.Collections.Generic;

namespace Cactask_API.Models;

public partial class Issuerelation
{
    public long Issuerelationid { get; set; }

    public long Parentissueid { get; set; }

    public long Childissueid { get; set; }

    public string? Relationtype { get; set; }

    public DateTime? Createdat { get; set; }

    public virtual Issue Childissue { get; set; } = null!;

    public virtual Issue Parentissue { get; set; } = null!;
}
