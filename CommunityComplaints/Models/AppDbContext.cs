using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace CommunityComplaints.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Announcement> Announcements { get; set; }

    public virtual DbSet<AnnouncementRead> AnnouncementReads { get; set; }

    public virtual DbSet<Assignment> Assignments { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Complaint> Complaints { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Rating> Ratings { get; set; }

    public virtual DbSet<ResolutionStage> ResolutionStages { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    // FIX 7: Removed OnConfiguring() with hardcoded connection string.
    // Program.cs already registers the DbContext correctly via DI using appsettings.json.
    // The hardcoded fallback was a security risk if appsettings was misconfigured.

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Announcement>(entity =>
        {
            entity.HasKey(e => e.AnnouncementId).HasName("PK__Announce__9DE44574EE8650C9");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Announcements)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Announcements_User");
        });

        modelBuilder.Entity<AnnouncementRead>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Announce__3214EC07E46DC71D");

            entity.Property(e => e.ReadAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Announcement).WithMany(p => p.AnnouncementReads).HasConstraintName("FK_Reads_Announcement");

            entity.HasOne(d => d.User).WithMany(p => p.AnnouncementReads)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reads_User");
        });

        modelBuilder.Entity<Assignment>(entity =>
        {
            entity.HasKey(e => e.AssignmentId).HasName("PK__Assignme__32499E7762CCCBD0");

            entity.Property(e => e.AssignedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.AssignedByNavigation).WithMany(p => p.AssignmentAssignedByNavigations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Assignments_AssignedBy");

            entity.HasOne(d => d.Complaint).WithMany(p => p.Assignments).HasConstraintName("FK_Assignments_Complaint");

            entity.HasOne(d => d.Staff).WithMany(p => p.AssignmentStaffs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Assignments_Staff");
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PK__Comments__C3B4DFCA9EF283F8");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Complaint).WithMany(p => p.Comments).HasConstraintName("FK_Comments_Complaint");

            entity.HasOne(d => d.User).WithMany(p => p.Comments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Comments_User");
        });

        modelBuilder.Entity<Complaint>(entity =>
        {
            entity.HasKey(e => e.ComplaintId).HasName("PK__Complain__740D898F7BDD9E7F");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Status).HasDefaultValue("Open");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Urgency).HasDefaultValue("Medium");

            entity.HasOne(d => d.Department).WithMany(p => p.Complaints)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Complaints_Department");

            entity.HasOne(d => d.Resident).WithMany(p => p.Complaints)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Complaints_Resident");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.DepartmentId).HasName("PK__Departme__B2079BED9131919E");
        });

        modelBuilder.Entity<Rating>(entity =>
        {
            entity.HasKey(e => e.RatingId).HasName("PK__Ratings__FCCDF87CF369A096");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Complaint).WithOne(p => p.Rating).HasConstraintName("FK_Ratings_Complaint");

            entity.HasOne(d => d.Resident).WithMany(p => p.Ratings)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ratings_Resident");
        });

        modelBuilder.Entity<ResolutionStage>(entity =>
        {
            entity.HasKey(e => e.StageId).HasName("PK__Resoluti__03EB7AD8E107CDCE");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Complaint).WithMany(p => p.ResolutionStages).HasConstraintName("FK_Stages_Complaint");

            entity.HasOne(d => d.Staff).WithMany(p => p.ResolutionStages)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Stages_Staff");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1A508701EA");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4CB34D190B");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_Role");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}