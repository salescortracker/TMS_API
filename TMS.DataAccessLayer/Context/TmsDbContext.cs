using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TMS.DataAccessLayer.Entities;

namespace TMS.DataAccessLayer.Context;

public partial class TmsDbContext : DbContext
{
    public TmsDbContext(DbContextOptions<TmsDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ActivityLog> ActivityLogs { get; set; }

    public virtual DbSet<ActivityType> ActivityTypes { get; set; }

    public virtual DbSet<AppUser> AppUsers { get; set; }

    public virtual DbSet<Approver> Approvers { get; set; }

    public virtual DbSet<CandidateApproval> CandidateApprovals { get; set; }

    public virtual DbSet<CandidateOnboarding> CandidateOnboardings { get; set; }

    public virtual DbSet<CandidateWorkSetting> CandidateWorkSettings { get; set; }

    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<RolePermission> RolePermissions { get; set; }

    public virtual DbSet<Team> Teams { get; set; }

    public virtual DbSet<TimesheetAlert> TimesheetAlerts { get; set; }

    public virtual DbSet<TimesheetBreak> TimesheetBreaks { get; set; }

    public virtual DbSet<TimesheetDay> TimesheetDays { get; set; }

    public virtual DbSet<TimesheetStatusHistory> TimesheetStatusHistories { get; set; }

    public virtual DbSet<TimesheetUploadBatch> TimesheetUploadBatches { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<VwCandidateWeekSummary> VwCandidateWeekSummaries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ActivityLog>(entity =>
        {
            entity.ToTable("ActivityLog");

            entity.HasIndex(e => new { e.TargetCandidateId, e.CreatedAt }, "IX_ActivityLog_Candidate").IsDescending(false, true);

            entity.HasIndex(e => e.CreatedAt, "IX_ActivityLog_CreatedAt").IsDescending();

            entity.Property(e => e.ActionType)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.EntityType)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.Severity)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValue("Info");

            entity.HasOne(d => d.ActorAppUser).WithMany(p => p.ActivityLogs)
                .HasForeignKey(d => d.ActorAppUserId)
                .HasConstraintName("FK_ActivityLog_Actor");

            entity.HasOne(d => d.TargetCandidate).WithMany(p => p.ActivityLogs)
                .HasForeignKey(d => d.TargetCandidateId)
                .HasConstraintName("FK_ActivityLog_Candidate");
        });

        modelBuilder.Entity<ActivityType>(entity =>
        {
            entity.ToTable("ActivityType");

            entity.HasIndex(e => e.ActivityName, "UQ_ActivityType_Name").IsUnique();

            entity.Property(e => e.ActivityName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("AppUser");

            entity.HasIndex(e => e.Email, "UQ_AppUser_Email").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Email).HasMaxLength(254);
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LastLoginAt).HasPrecision(0);
            entity.Property(e => e.PasswordHash).HasMaxLength(256);
        });

        modelBuilder.Entity<Approver>(entity =>
        {
            entity.ToTable("Approver");

            entity.Property(e => e.Email).HasMaxLength(254);
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.RoleLabel).HasMaxLength(50);

            entity.HasOne(d => d.AppUser).WithMany(p => p.Approvers)
                .HasForeignKey(d => d.AppUserId)
                .HasConstraintName("FK_Approver_AppUser");
        });

        modelBuilder.Entity<CandidateApproval>(entity =>
        {
            entity.ToTable("CandidateApproval");

            entity.HasIndex(e => new { e.CandidateId, e.ApproverId }, "UQ_CandidateApproval").IsUnique();

            entity.Property(e => e.Comments).HasMaxLength(500);
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.DecidedAt).HasPrecision(0);
            entity.Property(e => e.Decision)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValue("Pending");

            entity.HasOne(d => d.Approver).WithMany(p => p.CandidateApprovals)
                .HasForeignKey(d => d.ApproverId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CandidateApproval_Approver");

            entity.HasOne(d => d.Candidate).WithMany(p => p.CandidateApprovals)
                .HasForeignKey(d => d.CandidateId)
                .HasConstraintName("FK_CandidateApproval_Candidate");
        });

        modelBuilder.Entity<CandidateOnboarding>(entity =>
        {
            entity.HasKey(e => e.CandidateId);

            entity.ToTable("CandidateOnboarding");

            entity.HasIndex(e => new { e.CompanyId, e.TeamId }, "IX_CandidateOnboarding_CompanyTeam");

            entity.HasIndex(e => e.Status, "IX_CandidateOnboarding_Status");

            entity.HasIndex(e => e.Email, "UX_CandidateOnboarding_Email")
                .IsUnique()
                .HasFilter("([Email] IS NOT NULL)");

            entity.Property(e => e.ActivatedAt).HasPrecision(0);
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Email).HasMaxLength(254);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.PhoneDialCode)
                .HasMaxLength(6)
                .IsUnicode(false);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Draft");
            entity.Property(e => e.SubmittedAt).HasPrecision(0);
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.AppUser).WithMany(p => p.CandidateOnboardings)
                .HasForeignKey(d => d.AppUserId)
                .HasConstraintName("FK_CandidateOnboarding_AppUser");

            entity.HasOne(d => d.Company).WithMany(p => p.CandidateOnboardings)
                .HasForeignKey(d => d.CompanyId)
                .HasConstraintName("FK_CandidateOnboarding_Company");

            entity.HasOne(d => d.Team).WithMany(p => p.CandidateOnboardings)
                .HasForeignKey(d => d.TeamId)
                .HasConstraintName("FK_CandidateOnboarding_Team");
        });

        modelBuilder.Entity<CandidateWorkSetting>(entity =>
        {
            entity.HasKey(e => e.CandidateId);

            entity.ToTable("CandidateWorkSetting");

            entity.Property(e => e.CandidateId).ValueGeneratedNever();
            entity.Property(e => e.BreakAlertMinutes).HasDefaultValue(60);
            entity.Property(e => e.TimeZoneId)
                .HasMaxLength(64)
                .IsUnicode(false)
                .HasDefaultValue("UTC");
            entity.Property(e => e.WeekStartDay).HasDefaultValue((byte)1);
            entity.Property(e => e.WeeklyTargetHours)
                .HasDefaultValue(40m)
                .HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.Candidate).WithOne(p => p.CandidateWorkSetting)
                .HasForeignKey<CandidateWorkSetting>(d => d.CandidateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CWS_Candidate");
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.ToTable("Company");

            entity.HasIndex(e => e.CompanyName, "UQ_Company_CompanyName").IsUnique();

            entity.Property(e => e.CompanyName).HasMaxLength(150);
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.EmailDomain).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("Permission");

            entity.HasIndex(e => e.PermissionCode, "UQ_Permission_Code").IsUnique();

            entity.Property(e => e.Description).HasMaxLength(250);
            entity.Property(e => e.PermissionCode)
                .HasMaxLength(60)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Role");

            entity.HasIndex(e => e.RoleName, "UQ_Role_Name").IsUnique();

            entity.Property(e => e.Description).HasMaxLength(250);
            entity.Property(e => e.RoleName).HasMaxLength(50);

            entity.HasMany(d => d.Permissions).WithMany(p => p.Roles)
                .UsingEntity<RolePermission>(
                    r => r.HasOne(rp => rp.Permission).WithMany()
                        .HasForeignKey(rp => rp.PermissionId)
                        .HasConstraintName("FK_RolePermission_Permission"),
                    l => l.HasOne(rp => rp.Role).WithMany()
                        .HasForeignKey(rp => rp.RoleId)
                        .HasConstraintName("FK_RolePermission_Role"),
                    j =>
                    {
                        j.HasKey(rp => new { rp.RoleId, rp.PermissionId });
                        j.ToTable("RolePermission");
                    });
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity.ToTable("Team");

            entity.HasIndex(e => e.TeamName, "UQ_Team_TeamName").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.DialCode)
                .HasMaxLength(6)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.TeamName).HasMaxLength(100);
        });

        modelBuilder.Entity<TimesheetAlert>(entity =>
        {
            entity.ToTable("TimesheetAlert");

            entity.HasIndex(e => new { e.CandidateId, e.IsRead }, "IX_TimesheetAlert_Candidate");

            entity.Property(e => e.AlertType)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Message).HasMaxLength(500);
            entity.Property(e => e.NotifyCandidate).HasDefaultValue(true);

            entity.HasOne(d => d.Candidate).WithMany(p => p.TimesheetAlerts)
                .HasForeignKey(d => d.CandidateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TimesheetAlert_Candidate");

            entity.HasOne(d => d.TimesheetBreak).WithMany(p => p.TimesheetAlerts)
                .HasForeignKey(d => d.TimesheetBreakId)
                .HasConstraintName("FK_TimesheetAlert_Break");

            entity.HasOne(d => d.TimesheetDay).WithMany(p => p.TimesheetAlerts)
                .HasForeignKey(d => d.TimesheetDayId)
                .HasConstraintName("FK_TimesheetAlert_Day");
        });

        modelBuilder.Entity<TimesheetBreak>(entity =>
        {
            entity.ToTable("TimesheetBreak");

            entity.HasIndex(e => e.TimesheetDayId, "IX_TimesheetBreak_Day");

            entity.Property(e => e.BreakEndAt).HasPrecision(0);
            entity.Property(e => e.BreakStartAt).HasPrecision(0);
            entity.Property(e => e.DurationMinutes).HasComputedColumnSql("(case when [BreakEndAt] IS NULL then NULL else datediff(minute,[BreakStartAt],[BreakEndAt]) end)", false);

            entity.HasOne(d => d.TimesheetDay).WithMany(p => p.TimesheetBreaks)
                .HasForeignKey(d => d.TimesheetDayId)
                .HasConstraintName("FK_TimesheetBreak_Day");
        });

        modelBuilder.Entity<TimesheetDay>(entity =>
        {
            entity.ToTable("TimesheetDay");

            entity.HasIndex(e => new { e.CandidateId, e.WeekStartDate }, "IX_TimesheetDay_CandidateWeek");

            entity.HasIndex(e => e.Status, "IX_TimesheetDay_Status");

            entity.HasIndex(e => new { e.CandidateId, e.WorkDate }, "UQ_TimesheetDay_CandidateDate").IsUnique();

            entity.Property(e => e.ClockInAt).HasPrecision(0);
            entity.Property(e => e.ClockOutAt).HasPrecision(0);
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.EntrySource)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValue("Clock");
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.RejectionReason).HasMaxLength(500);
            entity.Property(e => e.ReviewedAt).HasPrecision(0);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Draft");
            entity.Property(e => e.SubmittedAt).HasPrecision(0);
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.WorkedHours)
                .HasComputedColumnSql("(CONVERT([decimal](5,2),[WorkedMinutes]/(60.0)))", true)
                .HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.ActivityType).WithMany(p => p.TimesheetDays)
                .HasForeignKey(d => d.ActivityTypeId)
                .HasConstraintName("FK_TimesheetDay_Activity");

            entity.HasOne(d => d.Candidate).WithMany(p => p.TimesheetDays)
                .HasForeignKey(d => d.CandidateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TimesheetDay_Candidate");

            entity.HasOne(d => d.ReviewedByApprover).WithMany(p => p.TimesheetDays)
                .HasForeignKey(d => d.ReviewedByApproverId)
                .HasConstraintName("FK_TimesheetDay_Reviewer");

            entity.HasOne(d => d.UploadBatch).WithMany(p => p.TimesheetDays)
                .HasForeignKey(d => d.UploadBatchId)
                .HasConstraintName("FK_TimesheetDay_UploadBatch");
        });

        modelBuilder.Entity<TimesheetStatusHistory>(entity =>
        {
            entity.ToTable("TimesheetStatusHistory");

            entity.HasIndex(e => new { e.TimesheetDayId, e.ChangedAt }, "IX_TSHistory_Day");

            entity.Property(e => e.ChangedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Comments).HasMaxLength(500);
            entity.Property(e => e.FromStatus)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.ToStatus)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.ChangedByApprover).WithMany(p => p.TimesheetStatusHistories)
                .HasForeignKey(d => d.ChangedByApproverId)
                .HasConstraintName("FK_TSHistory_Approver");

            entity.HasOne(d => d.ChangedByCandidate).WithMany(p => p.TimesheetStatusHistories)
                .HasForeignKey(d => d.ChangedByCandidateId)
                .HasConstraintName("FK_TSHistory_Candidate");

            entity.HasOne(d => d.TimesheetDay).WithMany(p => p.TimesheetStatusHistories)
                .HasForeignKey(d => d.TimesheetDayId)
                .HasConstraintName("FK_TSHistory_Day");
        });

        modelBuilder.Entity<TimesheetUploadBatch>(entity =>
        {
            entity.HasKey(e => e.UploadBatchId);

            entity.ToTable("TimesheetUploadBatch");

            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
            entity.Property(e => e.FileName).HasMaxLength(260);
            entity.Property(e => e.Status)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasDefaultValue("Processing");
            entity.Property(e => e.UploadedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Candidate).WithMany(p => p.TimesheetUploadBatches)
                .HasForeignKey(d => d.CandidateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UploadBatch_Candidate");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("UserRole");

            entity.HasIndex(e => new { e.AppUserId, e.RoleId, e.CompanyId, e.TeamId }, "UX_UserRole_Scope").IsUnique();

            entity.Property(e => e.GrantedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.AppUser).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.AppUserId)
                .HasConstraintName("FK_UserRole_User");

            entity.HasOne(d => d.Company).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.CompanyId)
                .HasConstraintName("FK_UserRole_Company");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserRole_Role");

            entity.HasOne(d => d.Team).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.TeamId)
                .HasConstraintName("FK_UserRole_Team");
        });

        modelBuilder.Entity<VwCandidateWeekSummary>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_CandidateWeekSummary");

            entity.Property(e => e.TargetHours).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.WorkedHours).HasColumnType("decimal(6, 2)");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
