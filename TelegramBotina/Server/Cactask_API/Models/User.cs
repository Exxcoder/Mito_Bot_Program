using System;
using System.Collections.Generic;

namespace Cactask_API.Models;

public partial class User
{
    public long Userid { get; set; }

    public string Username { get; set; } = null!;

    public string Usersurname { get; set; } = null!;

    public string? Usermiddlename { get; set; }

    public string? Useralias { get; set; }

    public int Userage { get; set; }

    public string? Useravatar { get; set; }

    public string? Userbio { get; set; }

    public string Usercountry { get; set; } = null!;

    public string Usertown { get; set; } = null!;

    public string? Userspecialization { get; set; }

    public string? Userphone { get; set; }

    public string? Useremail { get; set; }

    public DateTime? Usercreatedat { get; set; }

    public bool? Userisdeleted { get; set; }

    public string Userpassword { get; set; } = null!;

    public virtual ICollection<Historyissue> Historyissues { get; set; } = new List<Historyissue>();

    public virtual ICollection<Issueboard> Issueboards { get; set; } = new List<Issueboard>();

    public virtual ICollection<Issue> Issues { get; set; } = new List<Issue>();

    public virtual ICollection<Issuescomment> Issuescomments { get; set; } = new List<Issuescomment>();

    public virtual ICollection<Issueboard> IssueboardsNavigation { get; set; } = new List<Issueboard>();

    public virtual ICollection<Team> Teams { get; set; } = new List<Team>();
}
