using System;
using System.Collections.Generic;

namespace Cactask_API.Models;

public partial class Historyissue
{
    public long Historyid { get; set; }

    public long Issueid { get; set; }

    public long Changedbyuserid { get; set; }

    public DateTime? Changedate { get; set; }

    public string Changetype { get; set; } = null!;

    public string? Fieldchanged { get; set; }

    public string? Oldvalue { get; set; }

    public string? Newvalue { get; set; }

    public string? Changecomment { get; set; }

    public virtual User Changedbyuser { get; set; } = null!;

    public virtual Issue Issue { get; set; } = null!;
}
