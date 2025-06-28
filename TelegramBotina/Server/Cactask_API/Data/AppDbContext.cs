using System;
using System.Collections.Generic;
using Cactask_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Cactask_API.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Commentreciefe> Commentrecieves { get; set; }

    public virtual DbSet<Historyissue> Historyissues { get; set; }

    public virtual DbSet<Issue> Issues { get; set; }

    public virtual DbSet<Issueboard> Issueboards { get; set; }

    public virtual DbSet<Issuerelation> Issuerelations { get; set; }

    public virtual DbSet<Issuescomment> Issuescomments { get; set; }

    public virtual DbSet<Issuestype> Issuestypes { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<Status> Statuses { get; set; }

    public virtual DbSet<Team> Teams { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Database=cactask;Username=postgres;Password=cactus;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Commentreciefe>(entity =>
        {
            entity.HasKey(e => e.Commentrecieveid).HasName("commentrecieves_pkey");

            entity.ToTable("commentrecieves");

            entity.HasIndex(e => e.Commentchild, "idx_comment_child");

            entity.HasIndex(e => e.Commentparent, "idx_comment_parent");

            entity.Property(e => e.Commentrecieveid).HasColumnName("commentrecieveid");
            entity.Property(e => e.Commentchild).HasColumnName("commentchild");
            entity.Property(e => e.Commentparent).HasColumnName("commentparent");

            entity.HasOne(d => d.CommentchildNavigation).WithMany(p => p.CommentreciefeCommentchildNavigations)
                .HasForeignKey(d => d.Commentchild)
                .HasConstraintName("fk_commentrecieve_child");

            entity.HasOne(d => d.CommentparentNavigation).WithMany(p => p.CommentreciefeCommentparentNavigations)
                .HasForeignKey(d => d.Commentparent)
                .HasConstraintName("fk_commentrecieve_parent");
        });

        modelBuilder.Entity<Historyissue>(entity =>
        {
            entity.HasKey(e => e.Historyid).HasName("historyissue_pkey");

            entity.ToTable("historyissue");

            entity.HasIndex(e => e.Changedate, "idx_history_date");

            entity.HasIndex(e => e.Issueid, "idx_history_issue");

            entity.HasIndex(e => e.Changetype, "idx_history_type");

            entity.HasIndex(e => e.Changedbyuserid, "idx_history_user");

            entity.Property(e => e.Historyid).HasColumnName("historyid");
            entity.Property(e => e.Changecomment).HasColumnName("changecomment");
            entity.Property(e => e.Changedate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("changedate");
            entity.Property(e => e.Changedbyuserid).HasColumnName("changedbyuserid");
            entity.Property(e => e.Changetype).HasColumnName("changetype");
            entity.Property(e => e.Fieldchanged).HasColumnName("fieldchanged");
            entity.Property(e => e.Issueid).HasColumnName("issueid");
            entity.Property(e => e.Newvalue).HasColumnName("newvalue");
            entity.Property(e => e.Oldvalue).HasColumnName("oldvalue");

            entity.HasOne(d => d.Changedbyuser).WithMany(p => p.Historyissues)
                .HasForeignKey(d => d.Changedbyuserid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_history_user");

            entity.HasOne(d => d.Issue).WithMany(p => p.Historyissues)
                .HasForeignKey(d => d.Issueid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_history_issue");
        });

        modelBuilder.Entity<Issue>(entity =>
        {
            entity.HasKey(e => e.Issueid).HasName("issues_pkey");

            entity.ToTable("issues");

            entity.HasIndex(e => e.Issueavtor, "idx_issues_author");

            entity.HasIndex(e => e.Issueboardid, "idx_issues_board");

            entity.HasIndex(e => e.Issueexecutor, "idx_issues_executor");

            entity.HasIndex(e => e.Issuename, "idx_issues_name");

            entity.Property(e => e.Issueid).HasColumnName("issueid");
            entity.Property(e => e.Issueavtor).HasColumnName("issueavtor");
            entity.Property(e => e.Issueboardid).HasColumnName("issueboardid");
            entity.Property(e => e.Issuedeadline)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("issuedeadline");
            entity.Property(e => e.Issuedescription).HasColumnName("issuedescription");
            entity.Property(e => e.Issueexecutor).HasColumnName("issueexecutor");
            entity.Property(e => e.Issueisdeleted)
                .HasDefaultValue(false)
                .HasColumnName("issueisdeleted");
            entity.Property(e => e.Issuename).HasColumnName("issuename");
            entity.Property(e => e.Issuephotos).HasColumnName("issuephotos");
            entity.Property(e => e.Issuepriority).HasColumnName("issuepriority");
            entity.Property(e => e.Issuetype).HasColumnName("issuetype");
            entity.Property(e => e.Issuevideo).HasColumnName("issuevideo");

            entity.HasOne(d => d.Issueboard).WithMany(p => p.Issues)
                .HasForeignKey(d => d.Issueboardid)
                .HasConstraintName("fk_issueissueboard");

            entity.HasOne(d => d.IssueexecutorNavigation).WithMany(p => p.Issues)
                .HasForeignKey(d => d.Issueexecutor)
                .HasConstraintName("fk_issue_executor_user");

            entity.HasOne(d => d.IssuetypeNavigation).WithMany(p => p.Issues)
                .HasForeignKey(d => d.Issuetype)
                .HasConstraintName("fk_issue_issuetype");
        });

        modelBuilder.Entity<Issueboard>(entity =>
        {
            entity.HasKey(e => e.Issueboardid).HasName("issueboards_pkey");

            entity.ToTable("issueboards");

            entity.Property(e => e.Issueboardid).HasColumnName("issueboardid");
            entity.Property(e => e.Issueboardavatar).HasColumnName("issueboardavatar");
            entity.Property(e => e.Issueboardname).HasColumnName("issueboardname");
            entity.Property(e => e.Issueboardresponsible).HasColumnName("issueboardresponsible");
            entity.Property(e => e.Projectid).HasColumnName("projectid");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.IssueboardresponsibleNavigation).WithMany(p => p.Issueboards)
                .HasForeignKey(d => d.Issueboardresponsible)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_issueboardsresponsible");

            entity.HasOne(d => d.Project).WithMany(p => p.Issueboards)
                .HasForeignKey(d => d.Projectid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_issueboardsproj");
        });

        modelBuilder.Entity<Issuerelation>(entity =>
        {
            entity.HasKey(e => e.Issuerelationid).HasName("issuerelations_pkey");

            entity.ToTable("issuerelations");

            entity.HasIndex(e => e.Childissueid, "idx_issue_relations_child");

            entity.HasIndex(e => e.Parentissueid, "idx_issue_relations_parent");

            entity.HasIndex(e => e.Relationtype, "idx_issue_relations_type");

            entity.HasIndex(e => new { e.Parentissueid, e.Childissueid }, "unique_issue_relation").IsUnique();

            entity.Property(e => e.Issuerelationid).HasColumnName("issuerelationid");
            entity.Property(e => e.Childissueid).HasColumnName("childissueid");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Parentissueid).HasColumnName("parentissueid");
            entity.Property(e => e.Relationtype).HasColumnName("relationtype");

            entity.HasOne(d => d.Childissue).WithMany(p => p.IssuerelationChildissues)
                .HasForeignKey(d => d.Childissueid)
                .HasConstraintName("fk_child_issue");

            entity.HasOne(d => d.Parentissue).WithMany(p => p.IssuerelationParentissues)
                .HasForeignKey(d => d.Parentissueid)
                .HasConstraintName("fk_parent_issue");
        });

        modelBuilder.Entity<Issuescomment>(entity =>
        {
            entity.HasKey(e => e.Issuecommentid).HasName("issuescomments_pkey");

            entity.ToTable("issuescomments");

            entity.HasIndex(e => e.Issuecommentcreatedat, "idx_issue_comments_created");

            entity.HasIndex(e => e.Issueid, "idx_issue_comments_issue");

            entity.HasIndex(e => e.Issuecommentsender, "idx_issue_comments_sender");

            entity.Property(e => e.Issuecommentid).HasColumnName("issuecommentid");
            entity.Property(e => e.Issuecommentcreatedat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("issuecommentcreatedat");
            entity.Property(e => e.Issuecommentisdeleted)
                .HasDefaultValue(false)
                .HasColumnName("issuecommentisdeleted");
            entity.Property(e => e.Issuecommentphoto).HasColumnName("issuecommentphoto");
            entity.Property(e => e.Issuecommentsender).HasColumnName("issuecommentsender");
            entity.Property(e => e.Issuecommenttext).HasColumnName("issuecommenttext");
            entity.Property(e => e.Issuecommentvideo).HasColumnName("issuecommentvideo");
            entity.Property(e => e.Issueid).HasColumnName("issueid");

            entity.HasOne(d => d.IssuecommentsenderNavigation).WithMany(p => p.Issuescomments)
                .HasForeignKey(d => d.Issuecommentsender)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_issuecommentuser");

            entity.HasOne(d => d.Issue).WithMany(p => p.Issuescomments)
                .HasForeignKey(d => d.Issueid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_issuecommentsissues");
        });

        modelBuilder.Entity<Issuestype>(entity =>
        {
            entity.HasKey(e => e.Issuetypeid).HasName("issuestypes_pkey");

            entity.ToTable("issuestypes");

            entity.Property(e => e.Issuetypeid)
                .ValueGeneratedNever()
                .HasColumnName("issuetypeid");
            entity.Property(e => e.Issuetypename).HasColumnName("issuetypename");
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Projectid).HasName("projects_pkey");

            entity.ToTable("projects");

            entity.HasIndex(e => e.Projectcreatedat, "idx_projects_created_at");

            entity.HasIndex(e => new { e.Projectcountry, e.Projecttown }, "idx_projects_location");

            entity.HasIndex(e => e.Projectname, "idx_projects_name");

            entity.Property(e => e.Projectid).HasColumnName("projectid");
            entity.Property(e => e.Projectbio).HasColumnName("projectbio");
            entity.Property(e => e.Projectcountry).HasColumnName("projectcountry");
            entity.Property(e => e.Projectcreatedat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("projectcreatedat");
            entity.Property(e => e.Projectemail).HasColumnName("projectemail");
            entity.Property(e => e.Projectfoundeddate).HasColumnName("projectfoundeddate");
            entity.Property(e => e.Projectisdeleted)
                .HasDefaultValue(false)
                .HasColumnName("projectisdeleted");
            entity.Property(e => e.Projectlogo).HasColumnName("projectlogo");
            entity.Property(e => e.Projectname).HasColumnName("projectname");
            entity.Property(e => e.Projectphone).HasColumnName("projectphone");
            entity.Property(e => e.Projecttown).HasColumnName("projecttown");
        });

        modelBuilder.Entity<Status>(entity =>
        {
            entity.HasKey(e => e.Statusid).HasName("statuses_pkey");

            entity.ToTable("statuses");

            entity.Property(e => e.Statusid).HasColumnName("statusid");
            entity.Property(e => e.Statusname).HasColumnName("statusname");
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasKey(e => e.Teamid).HasName("teams_pkey");

            entity.ToTable("teams");

            entity.HasIndex(e => e.Teamfullness, "idx_teams_fullness");

            entity.HasIndex(e => e.Projectid, "idx_teams_project");

            entity.Property(e => e.Teamid).HasColumnName("teamid");
            entity.Property(e => e.Projectid).HasColumnName("projectid");
            entity.Property(e => e.Teamcreatedat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("teamcreatedat");
            entity.Property(e => e.Teamfullness).HasColumnName("teamfullness");
            entity.Property(e => e.Teamsize)
                .HasDefaultValue(1)
                .HasColumnName("teamsize");
            entity.Property(e => e.Teamusers).HasColumnName("teamusers");

            entity.HasMany(d => d.Projects).WithMany(p => p.Teams)
                .UsingEntity<Dictionary<string, object>>(
                    "Teamproject",
                    r => r.HasOne<Project>().WithMany()
                        .HasForeignKey("Projectid")
                        .HasConstraintName("fk_team_project_project"),
                    l => l.HasOne<Team>().WithMany()
                        .HasForeignKey("Teamid")
                        .HasConstraintName("fk_team_project_team"),
                    j =>
                    {
                        j.HasKey("Teamid", "Projectid").HasName("pk_team_project");
                        j.ToTable("teamprojects");
                        j.IndexerProperty<long>("Teamid").HasColumnName("teamid");
                        j.IndexerProperty<long>("Projectid").HasColumnName("projectid");
                    });
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Userid).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Useralias, "idx_users_alias");

            entity.HasIndex(e => new { e.Usercountry, e.Usertown }, "idx_users_country_town");

            entity.HasIndex(e => e.Usercreatedat, "idx_users_created_at");

            entity.HasIndex(e => e.Useralias, "users_useralias_key").IsUnique();

            entity.Property(e => e.Userid).HasColumnName("userid");
            entity.Property(e => e.Userage).HasColumnName("userage");
            entity.Property(e => e.Useralias).HasColumnName("useralias");
            entity.Property(e => e.Useravatar).HasColumnName("useravatar");
            entity.Property(e => e.Userbio).HasColumnName("userbio");
            entity.Property(e => e.Usercountry).HasColumnName("usercountry");
            entity.Property(e => e.Usercreatedat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("usercreatedat");
            entity.Property(e => e.Useremail).HasColumnName("useremail");
            entity.Property(e => e.Userisdeleted)
                .HasDefaultValue(false)
                .HasColumnName("userisdeleted");
            entity.Property(e => e.Usermiddlename).HasColumnName("usermiddlename");
            entity.Property(e => e.Username).HasColumnName("username");
            entity.Property(e => e.Userpassword).HasColumnName("userpassword");
            entity.Property(e => e.Userphone).HasColumnName("userphone");
            entity.Property(e => e.Userspecialization).HasColumnName("userspecialization");
            entity.Property(e => e.Usersurname).HasColumnName("usersurname");
            entity.Property(e => e.Usertown).HasColumnName("usertown");

            entity.HasMany(d => d.IssueboardsNavigation).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "Userissueboard",
                    r => r.HasOne<Issueboard>().WithMany()
                        .HasForeignKey("Issueboardid")
                        .HasConstraintName("fk_user_issue_board_board"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("Userid")
                        .HasConstraintName("fk_user_issue_board_user"),
                    j =>
                    {
                        j.HasKey("Userid", "Issueboardid").HasName("pk_user_issue_board");
                        j.ToTable("userissueboards");
                        j.HasIndex(new[] { "Issueboardid" }, "idx_user_issue_boards_board");
                        j.HasIndex(new[] { "Userid" }, "idx_user_issue_boards_user");
                        j.IndexerProperty<long>("Userid").HasColumnName("userid");
                        j.IndexerProperty<long>("Issueboardid").HasColumnName("issueboardid");
                    });

            entity.HasMany(d => d.Teams).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "Userteam",
                    r => r.HasOne<Team>().WithMany()
                        .HasForeignKey("Teamid")
                        .HasConstraintName("fk_user_team_team"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("Userid")
                        .HasConstraintName("fk_user_team_user"),
                    j =>
                    {
                        j.HasKey("Userid", "Teamid").HasName("pk_user_team");
                        j.ToTable("userteams");
                        j.IndexerProperty<long>("Userid").HasColumnName("userid");
                        j.IndexerProperty<long>("Teamid").HasColumnName("teamid");
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
