/* =====================================================================
   Cortracker Timesheet Management System — complete database
   SQL Server (T-SQL).  Run this whole file once in SSMS on a fresh instance.

   What it does
     1. Creates the database TMS_DB (only if it does not exist yet)
     2. Creates every table, key, index and view the Angular screens need
     3. Seeds companies, teams, roles, menus, permissions and settings
     4. Inserts ONE Super Admin login (edit the two variables in section 5)

   Table / column names match the existing EF Core entities in
   TMS.DataAccessLayer (PascalCase, singular).  Columns marked [NEW] are
   additions the screens need that the current entities don't have yet.

   To start over from zero, run this first (destroys ALL data):
       USE master;
       ALTER DATABASE TMS_DB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
       DROP DATABASE TMS_DB;
   ===================================================================== */

IF DB_ID(N'TMS_DB') IS NULL
    CREATE DATABASE TMS_DB;
GO

USE TMS_DB;
GO

-- Required for filtered indexes / persisted computed columns (SSMS has these on by default)
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* =====================================================================
   1. REFERENCE DATA
   ===================================================================== */

CREATE TABLE dbo.Company (
    CompanyId    INT IDENTITY(1,1) NOT NULL,
    CompanyName  NVARCHAR(150)     NOT NULL,
    EmailDomain  NVARCHAR(100)     NULL,
    IsActive     BIT               NOT NULL CONSTRAINT DF_Company_IsActive  DEFAULT (1),
    CreatedAt    DATETIME2(0)      NOT NULL CONSTRAINT DF_Company_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_Company PRIMARY KEY (CompanyId),
    CONSTRAINT UQ_Company_CompanyName UNIQUE (CompanyName)
);
GO

CREATE TABLE dbo.Team (
    TeamId     INT IDENTITY(1,1) NOT NULL,
    TeamName   NVARCHAR(100)     NOT NULL,
    DialCode   VARCHAR(6)        NOT NULL,
    IsActive   BIT               NOT NULL CONSTRAINT DF_Team_IsActive  DEFAULT (1),
    CreatedAt  DATETIME2(0)      NOT NULL CONSTRAINT DF_Team_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_Team PRIMARY KEY (TeamId),
    CONSTRAINT UQ_Team_TeamName UNIQUE (TeamName)
);
GO

-- [NEW] Key/value app settings (Register Manager "8 of 10 seats", caps, defaults)
CREATE TABLE dbo.SystemSetting (
    SettingKey    VARCHAR(60)    NOT NULL,
    SettingValue  NVARCHAR(500)  NOT NULL,
    Description   NVARCHAR(250)  NULL,
    UpdatedAt     DATETIME2(0)   NOT NULL CONSTRAINT DF_SystemSetting_UpdatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_SystemSetting PRIMARY KEY (SettingKey)
);
GO

CREATE TABLE dbo.ActivityType (
    ActivityTypeId  INT IDENTITY(1,1) NOT NULL,
    ActivityName    NVARCHAR(100)     NOT NULL,
    IsActive        BIT               NOT NULL CONSTRAINT DF_ActivityType_IsActive DEFAULT (1),
    CONSTRAINT PK_ActivityType PRIMARY KEY (ActivityTypeId),
    CONSTRAINT UQ_ActivityType_Name UNIQUE (ActivityName)
);
GO

/* =====================================================================
   2. USERS, ROLES, MENUS, PERMISSIONS  (Login, Roles & Access, People)
   ===================================================================== */

CREATE TABLE dbo.AppUser (
    AppUserId           INT IDENTITY(1,1) NOT NULL,
    FullName            NVARCHAR(150)     NOT NULL,
    Email               NVARCHAR(254)     NOT NULL,
    PasswordHash        VARBINARY(256)    NULL,
    MustChangePassword  BIT               NOT NULL CONSTRAINT DF_AppUser_MustChangePassword DEFAULT (0), -- [NEW]
    IsActive            BIT               NOT NULL CONSTRAINT DF_AppUser_IsActive  DEFAULT (1),
    LastLoginAt         DATETIME2(0)      NULL,
    CreatedAt           DATETIME2(0)      NOT NULL CONSTRAINT DF_AppUser_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_AppUser PRIMARY KEY (AppUserId),
    CONSTRAINT UQ_AppUser_Email UNIQUE (Email)
);
GO

CREATE TABLE dbo.Role (
    RoleId       INT IDENTITY(1,1) NOT NULL,
    RoleName     NVARCHAR(50)      NOT NULL,
    Description  NVARCHAR(250)     NULL,
    IsBuiltIn    BIT               NOT NULL CONSTRAINT DF_Role_IsBuiltIn DEFAULT (0), -- [NEW] "Fixed - cannot be changed"
    CONSTRAINT PK_Role PRIMARY KEY (RoleId),
    CONSTRAINT UQ_Role_Name UNIQUE (RoleName)
);
GO

CREATE TABLE dbo.Permission (
    PermissionId    INT IDENTITY(1,1) NOT NULL,
    PermissionCode  VARCHAR(60)       NOT NULL,
    Description     NVARCHAR(250)     NULL,
    CONSTRAINT PK_Permission PRIMARY KEY (PermissionId),
    CONSTRAINT UQ_Permission_Code UNIQUE (PermissionCode)
);
GO

CREATE TABLE dbo.RolePermission (
    RoleId        INT NOT NULL,
    PermissionId  INT NOT NULL,
    CONSTRAINT PK_RolePermission PRIMARY KEY (RoleId, PermissionId),
    CONSTRAINT FK_RolePermission_Role       FOREIGN KEY (RoleId)       REFERENCES dbo.Role (RoleId),
    CONSTRAINT FK_RolePermission_Permission FOREIGN KEY (PermissionId) REFERENCES dbo.Permission (PermissionId)
);
GO

-- [NEW] Menus a role can open (the "Menus" checkbox grid on Roles & Access)
CREATE TABLE dbo.Menu (
    MenuId        INT IDENTITY(1,1) NOT NULL,
    MenuCode      VARCHAR(40)       NOT NULL,
    MenuName      NVARCHAR(100)     NOT NULL,
    Description   NVARCHAR(250)     NULL,
    DisplayOrder  INT               NOT NULL CONSTRAINT DF_Menu_DisplayOrder DEFAULT (0),
    CONSTRAINT PK_Menu PRIMARY KEY (MenuId),
    CONSTRAINT UQ_Menu_Code UNIQUE (MenuCode)
);
GO

CREATE TABLE dbo.RoleMenu (
    RoleId  INT NOT NULL,
    MenuId  INT NOT NULL,
    CONSTRAINT PK_RoleMenu PRIMARY KEY (RoleId, MenuId),
    CONSTRAINT FK_RoleMenu_Role FOREIGN KEY (RoleId) REFERENCES dbo.Role (RoleId),
    CONSTRAINT FK_RoleMenu_Menu FOREIGN KEY (MenuId) REFERENCES dbo.Menu (MenuId)
);
GO

-- A user can hold several roles. CompanyId/TeamId NULL = all companies/teams.
-- A Manager scoped to (Company X, Team Y) approves timesheets for candidates in that scope.
CREATE TABLE dbo.UserRole (
    UserRoleId  INT IDENTITY(1,1) NOT NULL,
    AppUserId   INT               NOT NULL,
    RoleId      INT               NOT NULL,
    CompanyId   INT               NULL,
    TeamId      INT               NULL,
    GrantedAt   DATETIME2(0)      NOT NULL CONSTRAINT DF_UserRole_GrantedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_UserRole PRIMARY KEY (UserRoleId),
    CONSTRAINT FK_UserRole_User    FOREIGN KEY (AppUserId) REFERENCES dbo.AppUser (AppUserId),
    CONSTRAINT FK_UserRole_Role    FOREIGN KEY (RoleId)    REFERENCES dbo.Role (RoleId),
    CONSTRAINT FK_UserRole_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company (CompanyId),
    CONSTRAINT FK_UserRole_Team    FOREIGN KEY (TeamId)    REFERENCES dbo.Team (TeamId),
    CONSTRAINT UX_UserRole_Scope UNIQUE (AppUserId, RoleId, CompanyId, TeamId)
);
GO

-- [NEW] "Reset password" on Roles & Access
CREATE TABLE dbo.PasswordResetToken (
    PasswordResetTokenId  BIGINT IDENTITY(1,1) NOT NULL,
    AppUserId             INT                  NOT NULL,
    TokenHash             VARBINARY(64)        NOT NULL,
    ExpiresAt             DATETIME2(0)         NOT NULL,
    UsedAt                DATETIME2(0)         NULL,
    CreatedAt             DATETIME2(0)         NOT NULL CONSTRAINT DF_PwdReset_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_PasswordResetToken PRIMARY KEY (PasswordResetTokenId),
    CONSTRAINT FK_PwdReset_User FOREIGN KEY (AppUserId) REFERENCES dbo.AppUser (AppUserId)
);
GO

/* =====================================================================
   3. CANDIDATES, ONBOARDING & APPROVERS
      (Onboarding form, Register Manager, Onboarding Approvals, People)
   ===================================================================== */

-- Named approvers in the onboarding chain (e.g. Reporting Manager, Team Lead, HR)
CREATE TABLE dbo.Approver (
    ApproverId     INT IDENTITY(1,1) NOT NULL,
    FullName       NVARCHAR(150)     NOT NULL,
    Email          NVARCHAR(254)     NULL,
    RoleLabel      NVARCHAR(50)      NULL,
    ApprovalOrder  TINYINT           NOT NULL CONSTRAINT DF_Approver_Order DEFAULT (1),
    IsActive       BIT               NOT NULL CONSTRAINT DF_Approver_IsActive DEFAULT (1),
    AppUserId      INT               NULL,
    CONSTRAINT PK_Approver PRIMARY KEY (ApproverId),
    CONSTRAINT FK_Approver_AppUser FOREIGN KEY (AppUserId) REFERENCES dbo.AppUser (AppUserId)
);
GO

-- One row per person who self-onboards or registers. AppUserId is filled on activation.
CREATE TABLE dbo.CandidateOnboarding (
    CandidateId    INT IDENTITY(1,1) NOT NULL,
    RequestType    VARCHAR(20)       NOT NULL CONSTRAINT DF_CandOnb_RequestType DEFAULT ('Candidate'), -- [NEW]
    FirstName      NVARCHAR(100)     NOT NULL,
    LastName       NVARCHAR(100)     NOT NULL,
    DateOfBirth    DATE              NULL,
    JoiningDate    DATE              NULL,
    TeamId         INT               NULL,
    CompanyId      INT               NULL,
    PhoneDialCode  VARCHAR(6)        NULL,
    PhoneNumber    VARCHAR(20)       NULL,
    Email          NVARCHAR(254)     NULL,
    Status         VARCHAR(20)       NOT NULL CONSTRAINT DF_CandOnb_Status DEFAULT ('Draft'),
    FlagReason     NVARCHAR(200)     NULL,                                                             -- [NEW] "Name contains digits"
    SubmittedAt    DATETIME2(0)      NULL,
    ActivatedAt    DATETIME2(0)      NULL,
    CreatedAt      DATETIME2(0)      NOT NULL CONSTRAINT DF_CandOnb_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAt      DATETIME2(0)      NOT NULL CONSTRAINT DF_CandOnb_UpdatedAt DEFAULT (SYSUTCDATETIME()),
    AppUserId      INT               NULL,
    IsActive       BIT               NOT NULL CONSTRAINT DF_CandOnb_IsActive DEFAULT (1),
    CONSTRAINT PK_CandidateOnboarding PRIMARY KEY (CandidateId),
    CONSTRAINT FK_CandidateOnboarding_AppUser FOREIGN KEY (AppUserId) REFERENCES dbo.AppUser (AppUserId),
    CONSTRAINT FK_CandidateOnboarding_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company (CompanyId),
    CONSTRAINT FK_CandidateOnboarding_Team    FOREIGN KEY (TeamId)    REFERENCES dbo.Team (TeamId),
    CONSTRAINT CK_CandOnb_Status      CHECK (Status IN ('Draft','Pending','Approved','Rejected')),
    CONSTRAINT CK_CandOnb_RequestType CHECK (RequestType IN ('Candidate','Resource Manager'))
);
GO
CREATE INDEX IX_CandidateOnboarding_CompanyTeam ON dbo.CandidateOnboarding (CompanyId, TeamId);
CREATE INDEX IX_CandidateOnboarding_Status      ON dbo.CandidateOnboarding (Status);
CREATE UNIQUE INDEX UX_CandidateOnboarding_Email ON dbo.CandidateOnboarding (Email) WHERE Email IS NOT NULL;
GO

-- Per-candidate weekly target (40h), break-alert limit (60 min), timezone
CREATE TABLE dbo.CandidateWorkSetting (
    CandidateId        INT           NOT NULL,
    WeeklyTargetHours  DECIMAL(5,2)  NOT NULL CONSTRAINT DF_CWS_Target   DEFAULT (40),
    BreakAlertMinutes  INT           NOT NULL CONSTRAINT DF_CWS_Break    DEFAULT (60),
    WeekStartDay       TINYINT       NOT NULL CONSTRAINT DF_CWS_WeekDay  DEFAULT (1),   -- 1 = Monday
    TimeZoneId         VARCHAR(64)   NOT NULL CONSTRAINT DF_CWS_TimeZone DEFAULT ('UTC'),
    CONSTRAINT PK_CandidateWorkSetting PRIMARY KEY (CandidateId),
    CONSTRAINT FK_CWS_Candidate FOREIGN KEY (CandidateId) REFERENCES dbo.CandidateOnboarding (CandidateId)
);
GO

CREATE TABLE dbo.CandidateApproval (
    CandidateApprovalId  INT IDENTITY(1,1) NOT NULL,
    CandidateId          INT               NOT NULL,
    ApproverId           INT               NOT NULL,
    Decision             VARCHAR(10)       NOT NULL CONSTRAINT DF_CandApp_Decision DEFAULT ('Pending'),
    Comments             NVARCHAR(500)     NULL,
    DecidedAt            DATETIME2(0)      NULL,
    CreatedAt            DATETIME2(0)      NOT NULL CONSTRAINT DF_CandApp_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_CandidateApproval PRIMARY KEY (CandidateApprovalId),
    CONSTRAINT FK_CandidateApproval_Candidate FOREIGN KEY (CandidateId) REFERENCES dbo.CandidateOnboarding (CandidateId),
    CONSTRAINT FK_CandidateApproval_Approver  FOREIGN KEY (ApproverId)  REFERENCES dbo.Approver (ApproverId),
    CONSTRAINT UQ_CandidateApproval UNIQUE (CandidateId, ApproverId),
    CONSTRAINT CK_CandApp_Decision CHECK (Decision IN ('Pending','Approved','Rejected'))
);
GO

/* =====================================================================
   4. TIMESHEETS  (My Timesheet, Timesheet Approvals, Timesheets,
                   Dashboard, Bulk / Upload Hours, My History)
      One TimesheetDay row per candidate per calendar date.
      "Not entered" / "Upcoming" are NOT stored - they are derived
      (no row yet, or date is in the future).
   ===================================================================== */

CREATE TABLE dbo.TimesheetUploadBatch (
    UploadBatchId        INT IDENTITY(1,1) NOT NULL,
    CandidateId          INT               NULL,   -- NULL when an admin uploads for many candidates
    UploadedByAppUserId  INT               NULL,   -- [NEW]
    FileName             NVARCHAR(260)     NOT NULL,
    RowCountTotal        INT               NOT NULL CONSTRAINT DF_UB_Total    DEFAULT (0),
    RowCountImported     INT               NOT NULL CONSTRAINT DF_UB_Imported DEFAULT (0),
    RowCountError        INT               NOT NULL CONSTRAINT DF_UB_Error    DEFAULT (0), -- [NEW]
    Status               VARCHAR(15)       NOT NULL CONSTRAINT DF_UB_Status   DEFAULT ('Processing'),
    ErrorMessage         NVARCHAR(1000)    NULL,
    UploadedAt           DATETIME2(0)      NOT NULL CONSTRAINT DF_UB_UploadedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_TimesheetUploadBatch PRIMARY KEY (UploadBatchId),
    CONSTRAINT FK_UploadBatch_Candidate FOREIGN KEY (CandidateId)         REFERENCES dbo.CandidateOnboarding (CandidateId),
    CONSTRAINT FK_UploadBatch_UploadedBy FOREIGN KEY (UploadedByAppUserId) REFERENCES dbo.AppUser (AppUserId),
    CONSTRAINT CK_UB_Status CHECK (Status IN ('Processing','Completed','Failed'))
);
GO

CREATE TABLE dbo.TimesheetDay (
    TimesheetDayId        BIGINT IDENTITY(1,1) NOT NULL,
    CandidateId           INT                  NOT NULL,
    WorkDate              DATE                 NOT NULL,
    WeekStartDate         DATE                 NOT NULL,
    ClockInAt             DATETIME2(0)         NULL,   -- UTC
    ClockOutAt            DATETIME2(0)         NULL,   -- UTC
    ActivityTypeId        INT                  NULL,
    Task                  NVARCHAR(80)         NULL,   -- [NEW] free-text task (UI: up to 80 chars)
    Description           NVARCHAR(500)        NULL,   -- [NEW] what was done (UI: 10-500 chars)
    WorkedMinutes         INT                  NOT NULL CONSTRAINT DF_TD_Worked    DEFAULT (0),
    BreakMinutes          INT                  NOT NULL CONSTRAINT DF_TD_Break     DEFAULT (0),
    BreakCount            INT                  NOT NULL CONSTRAINT DF_TD_BreakCnt  DEFAULT (0),
    Notes                 NVARCHAR(1000)       NULL,
    EntrySource           VARCHAR(10)          NOT NULL CONSTRAINT DF_TD_Source    DEFAULT ('Clock'),
    UploadBatchId         INT                  NULL,
    Status                VARCHAR(20)          NOT NULL CONSTRAINT DF_TD_Status    DEFAULT ('Draft'),
    IsFlagged             BIT                  NOT NULL CONSTRAINT DF_TD_Flagged   DEFAULT (0), -- [NEW] Checks: Clean/Flagged
    FlagReason            NVARCHAR(200)        NULL,                                            -- [NEW]
    SubmittedAt           DATETIME2(0)         NULL,
    ReviewedAt            DATETIME2(0)         NULL,
    ReviewedByApproverId  INT                  NULL,
    ReviewedByAppUserId   INT                  NULL,   -- [NEW] the manager/admin account that decided
    RejectionReason       NVARCHAR(500)        NULL,
    CreatedAt             DATETIME2(0)         NOT NULL CONSTRAINT DF_TD_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAt             DATETIME2(0)         NOT NULL CONSTRAINT DF_TD_UpdatedAt DEFAULT (SYSUTCDATETIME()),
    WorkedHours           AS (CONVERT(DECIMAL(5,2), WorkedMinutes / (60.0))) PERSISTED,
    CONSTRAINT PK_TimesheetDay PRIMARY KEY (TimesheetDayId),
    CONSTRAINT FK_TimesheetDay_Candidate   FOREIGN KEY (CandidateId)          REFERENCES dbo.CandidateOnboarding (CandidateId),
    CONSTRAINT FK_TimesheetDay_Activity    FOREIGN KEY (ActivityTypeId)       REFERENCES dbo.ActivityType (ActivityTypeId),
    CONSTRAINT FK_TimesheetDay_UploadBatch FOREIGN KEY (UploadBatchId)        REFERENCES dbo.TimesheetUploadBatch (UploadBatchId),
    CONSTRAINT FK_TimesheetDay_Reviewer    FOREIGN KEY (ReviewedByApproverId) REFERENCES dbo.Approver (ApproverId),
    CONSTRAINT FK_TimesheetDay_ReviewerUser FOREIGN KEY (ReviewedByAppUserId) REFERENCES dbo.AppUser (AppUserId),
    CONSTRAINT UQ_TimesheetDay_CandidateDate UNIQUE (CandidateId, WorkDate),
    CONSTRAINT CK_TD_Status CHECK (Status IN ('Draft','Saved','Pending','Approved','Rejected')),
    CONSTRAINT CK_TD_Source CHECK (EntrySource IN ('Clock','Manual','Upload')),
    CONSTRAINT CK_TD_Minutes CHECK (WorkedMinutes >= 0 AND BreakMinutes >= 0 AND BreakCount >= 0)
);
GO
CREATE INDEX IX_TimesheetDay_CandidateWeek ON dbo.TimesheetDay (CandidateId, WeekStartDate);
CREATE INDEX IX_TimesheetDay_Status        ON dbo.TimesheetDay (Status);
CREATE INDEX IX_TimesheetDay_WorkDate      ON dbo.TimesheetDay (WorkDate);
GO

-- [NEW] Row-level results of a bulk upload (validation errors per Excel row)
CREATE TABLE dbo.TimesheetUploadRow (
    UploadRowId       BIGINT IDENTITY(1,1) NOT NULL,
    UploadBatchId     INT                  NOT NULL,
    RowNumber         INT                  NOT NULL,
    CandidateEmail    NVARCHAR(254)        NULL,
    WorkDate          DATE                 NULL,
    LogInText         VARCHAR(20)          NULL,   -- as typed, e.g. '9:00 AM'
    LogOutText        VARCHAR(20)          NULL,
    BreakMinutes      INT                  NULL,
    Task              NVARCHAR(80)         NULL,
    Description       NVARCHAR(500)        NULL,
    ValidationStatus  VARCHAR(15)          NOT NULL CONSTRAINT DF_UR_Status DEFAULT ('Pending'),
    ErrorMessage      NVARCHAR(300)        NULL,
    TimesheetDayId    BIGINT               NULL,   -- set once the row is imported
    CONSTRAINT PK_TimesheetUploadRow PRIMARY KEY (UploadRowId),
    CONSTRAINT FK_UploadRow_Batch FOREIGN KEY (UploadBatchId)  REFERENCES dbo.TimesheetUploadBatch (UploadBatchId) ON DELETE CASCADE,
    CONSTRAINT FK_UploadRow_Day   FOREIGN KEY (TimesheetDayId) REFERENCES dbo.TimesheetDay (TimesheetDayId),
    CONSTRAINT CK_UR_Status CHECK (ValidationStatus IN ('Pending','Valid','Error','Imported'))
);
GO
CREATE INDEX IX_TimesheetUploadRow_Batch ON dbo.TimesheetUploadRow (UploadBatchId);
GO

-- Each break punched during a day (clock card: Break in / Break out)
CREATE TABLE dbo.TimesheetBreak (
    TimesheetBreakId  BIGINT IDENTITY(1,1) NOT NULL,
    TimesheetDayId    BIGINT               NOT NULL,
    BreakStartAt      DATETIME2(0)         NOT NULL,
    BreakEndAt        DATETIME2(0)         NULL,
    DurationMinutes   AS (CASE WHEN BreakEndAt IS NULL THEN NULL ELSE DATEDIFF(MINUTE, BreakStartAt, BreakEndAt) END),
    AlertRaised       BIT                  NOT NULL CONSTRAINT DF_TB_Alert DEFAULT (0),
    CONSTRAINT PK_TimesheetBreak PRIMARY KEY (TimesheetBreakId),
    CONSTRAINT FK_TimesheetBreak_Day FOREIGN KEY (TimesheetDayId) REFERENCES dbo.TimesheetDay (TimesheetDayId) ON DELETE CASCADE
);
GO
CREATE INDEX IX_TimesheetBreak_Day ON dbo.TimesheetBreak (TimesheetDayId);
GO

-- Break over 60 min, daily/weekly cap exceeded, missing submission ...
CREATE TABLE dbo.TimesheetAlert (
    TimesheetAlertId  BIGINT IDENTITY(1,1) NOT NULL,
    CandidateId       INT                  NOT NULL,
    TimesheetDayId    BIGINT               NULL,
    TimesheetBreakId  BIGINT               NULL,
    AlertType         VARCHAR(30)          NOT NULL,   -- BreakOverLimit | DailyCapExceeded | WeeklyCapExceeded | MissingSubmission
    Message           NVARCHAR(500)        NOT NULL,
    NotifyCandidate   BIT                  NOT NULL CONSTRAINT DF_TA_NotifyC DEFAULT (1),
    NotifyAdmin       BIT                  NOT NULL CONSTRAINT DF_TA_NotifyA DEFAULT (1),
    IsRead            BIT                  NOT NULL CONSTRAINT DF_TA_IsRead  DEFAULT (0),
    CreatedAt         DATETIME2(0)         NOT NULL CONSTRAINT DF_TA_Created DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_TimesheetAlert PRIMARY KEY (TimesheetAlertId),
    CONSTRAINT FK_TimesheetAlert_Candidate FOREIGN KEY (CandidateId)      REFERENCES dbo.CandidateOnboarding (CandidateId),
    CONSTRAINT FK_TimesheetAlert_Day       FOREIGN KEY (TimesheetDayId)   REFERENCES dbo.TimesheetDay (TimesheetDayId),
    CONSTRAINT FK_TimesheetAlert_Break     FOREIGN KEY (TimesheetBreakId) REFERENCES dbo.TimesheetBreak (TimesheetBreakId)
);
GO
CREATE INDEX IX_TimesheetAlert_Candidate ON dbo.TimesheetAlert (CandidateId, IsRead);
GO

-- Every status change: who moved a day Draft -> Pending -> Approved / Rejected
CREATE TABLE dbo.TimesheetStatusHistory (
    TimesheetStatusHistoryId  BIGINT IDENTITY(1,1) NOT NULL,
    TimesheetDayId            BIGINT               NOT NULL,
    FromStatus                VARCHAR(20)          NULL,
    ToStatus                  VARCHAR(20)          NOT NULL,
    ChangedByCandidateId      INT                  NULL,
    ChangedByApproverId       INT                  NULL,
    ChangedByAppUserId        INT                  NULL,   -- [NEW]
    Comments                  NVARCHAR(500)        NULL,
    ChangedAt                 DATETIME2(0)         NOT NULL CONSTRAINT DF_TSH_ChangedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_TimesheetStatusHistory PRIMARY KEY (TimesheetStatusHistoryId),
    CONSTRAINT FK_TSHistory_Day       FOREIGN KEY (TimesheetDayId)       REFERENCES dbo.TimesheetDay (TimesheetDayId) ON DELETE CASCADE,
    CONSTRAINT FK_TSHistory_Candidate FOREIGN KEY (ChangedByCandidateId) REFERENCES dbo.CandidateOnboarding (CandidateId),
    CONSTRAINT FK_TSHistory_Approver  FOREIGN KEY (ChangedByApproverId)  REFERENCES dbo.Approver (ApproverId),
    CONSTRAINT FK_TSHistory_AppUser   FOREIGN KEY (ChangedByAppUserId)   REFERENCES dbo.AppUser (AppUserId)
);
GO
CREATE INDEX IX_TSHistory_Day ON dbo.TimesheetStatusHistory (TimesheetDayId, ChangedAt);
GO

/* =====================================================================
   5. AUDIT & NOTIFICATIONS  (Activity Log, Email alerts tab, bell icon)
   ===================================================================== */

CREATE TABLE dbo.ActivityLog (
    ActivityLogId      BIGINT IDENTITY(1,1) NOT NULL,
    ActorAppUserId     INT                  NULL,   -- NULL = system generated
    ActionType         VARCHAR(40)          NOT NULL,
    EntityType         VARCHAR(40)          NOT NULL,
    EntityId           BIGINT               NULL,
    TargetCandidateId  INT                  NULL,
    Category           VARCHAR(20)          NOT NULL CONSTRAINT DF_AL_Category DEFAULT ('Edits & roles'), -- [NEW] filter chips
    Severity           VARCHAR(10)          NOT NULL CONSTRAINT DF_AL_Severity DEFAULT ('Info'),
    Description        NVARCHAR(500)        NOT NULL,
    DetailsJson        NVARCHAR(MAX)        NULL,   -- e.g. {"field":"logOut","from":"17:00","to":"17:30"}
    CreatedAt          DATETIME2(0)         NOT NULL CONSTRAINT DF_AL_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_ActivityLog PRIMARY KEY (ActivityLogId),
    CONSTRAINT FK_ActivityLog_Actor     FOREIGN KEY (ActorAppUserId)    REFERENCES dbo.AppUser (AppUserId),
    CONSTRAINT FK_ActivityLog_Candidate FOREIGN KEY (TargetCandidateId) REFERENCES dbo.CandidateOnboarding (CandidateId),
    CONSTRAINT CK_AL_Category CHECK (Category IN ('Submissions','Approvals','Edits & roles','Alerts')),
    CONSTRAINT CK_AL_Severity CHECK (Severity IN ('Info','Success','Warning','Danger'))
);
GO
CREATE INDEX IX_ActivityLog_CreatedAt ON dbo.ActivityLog (CreatedAt DESC);
CREATE INDEX IX_ActivityLog_Candidate ON dbo.ActivityLog (TargetCandidateId, CreatedAt DESC);
GO

-- [NEW] Simulated emails shown on Activity Log > Email alerts
CREATE TABLE dbo.EmailAlert (
    EmailAlertId      BIGINT IDENTITY(1,1) NOT NULL,
    RecipientAppUserId INT                 NULL,   -- NULL if recipient isn't a user (e.g. rejected applicant)
    RecipientLabel    NVARCHAR(200)        NOT NULL,
    Subject           NVARCHAR(200)        NOT NULL,
    ActivityLogId     BIGINT               NULL,
    SentAt            DATETIME2(0)         NOT NULL CONSTRAINT DF_EA_SentAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_EmailAlert PRIMARY KEY (EmailAlertId),
    CONSTRAINT FK_EmailAlert_User FOREIGN KEY (RecipientAppUserId) REFERENCES dbo.AppUser (AppUserId),
    CONSTRAINT FK_EmailAlert_Log  FOREIGN KEY (ActivityLogId)      REFERENCES dbo.ActivityLog (ActivityLogId)
);
GO

-- [NEW] In-app notifications (the bell icon and its red count badge)
CREATE TABLE dbo.Notification (
    NotificationId  BIGINT IDENTITY(1,1) NOT NULL,
    AppUserId       INT                  NOT NULL,
    Title           NVARCHAR(200)        NOT NULL,
    Message         NVARCHAR(500)        NULL,
    LinkUrl         VARCHAR(200)         NULL,
    IsRead          BIT                  NOT NULL CONSTRAINT DF_N_IsRead  DEFAULT (0),
    CreatedAt       DATETIME2(0)         NOT NULL CONSTRAINT DF_N_Created DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_Notification PRIMARY KEY (NotificationId),
    CONSTRAINT FK_Notification_User FOREIGN KEY (AppUserId) REFERENCES dbo.AppUser (AppUserId)
);
GO
CREATE INDEX IX_Notification_User ON dbo.Notification (AppUserId, IsRead);
GO

/* =====================================================================
   6. VIEWS  (derived screens - nothing stored twice)
   ===================================================================== */

-- My History / Candidate Dashboard "Week progress" + stat cards
CREATE VIEW dbo.vw_CandidateWeekSummary AS
SELECT
    td.CandidateId,
    td.WeekStartDate,
    SUM(CASE WHEN td.Status IN ('Pending','Approved','Rejected') THEN 1 ELSE 0 END) AS SubmittedThisWeek,
    SUM(CASE WHEN td.Status = 'Pending'  THEN 1 ELSE 0 END)                         AS PendingApproval,
    SUM(CASE WHEN td.Status = 'Approved' THEN 1 ELSE 0 END)                         AS Approved,
    SUM(CASE WHEN td.Status = 'Rejected' THEN 1 ELSE 0 END)                         AS Rejected,
    SUM(CASE WHEN td.Status IN ('Draft','Saved') THEN 1 ELSE 0 END)                 AS Drafts,
    CONVERT(DECIMAL(6,2), SUM(td.WorkedHours))                                      AS WorkedHours,
    ISNULL(cws.WeeklyTargetHours, 40)                                               AS TargetHours
FROM dbo.TimesheetDay td
LEFT JOIN dbo.CandidateWorkSetting cws ON cws.CandidateId = td.CandidateId
GROUP BY td.CandidateId, td.WeekStartDate, cws.WeeklyTargetHours;
GO

-- Admin Dashboard "By company and team" + Timesheets team table (filter by WeekStartDate)
CREATE VIEW dbo.vw_TeamWeekSummary AS
SELECT
    c.CompanyId,
    c.TeamId,
    td.WeekStartDate,
    COUNT(DISTINCT td.CandidateId)                                        AS Candidates,
    CONVERT(DECIMAL(8,2), SUM(td.WorkedHours))                            AS Hours,
    SUM(CASE WHEN td.Status IN ('Pending','Approved','Rejected') THEN 1 ELSE 0 END) AS Submitted,
    SUM(CASE WHEN td.Status = 'Pending'  THEN 1 ELSE 0 END)               AS Pending,
    SUM(CASE WHEN td.Status = 'Approved' THEN 1 ELSE 0 END)               AS Approved,
    SUM(CASE WHEN td.Status = 'Rejected' THEN 1 ELSE 0 END)               AS Rejected
FROM dbo.TimesheetDay td
JOIN dbo.CandidateOnboarding c ON c.CandidateId = td.CandidateId
GROUP BY c.CompanyId, c.TeamId, td.WeekStartDate;
GO

/* =====================================================================
   7. SEED DATA
   ===================================================================== */

INSERT INTO dbo.Company (CompanyName, EmailDomain) VALUES
    (N'Cortracker Inc', N'cortracker360.com'),
    (N'Perfect Solutions Group Inc', NULL),
    (N'Others', NULL);

INSERT INTO dbo.Team (TeamName, DialCode) VALUES
    (N'USA team',   '+1'),
    (N'India team', '+91'),
    (N'UK team',    '+44');

INSERT INTO dbo.ActivityType (ActivityName) VALUES
    (N'Development'), (N'Testing / QA'), (N'Documentation'), (N'Code review'),
    (N'Bug fixing'),  (N'Meeting'),      (N'Support'),       (N'Design');

INSERT INTO dbo.SystemSetting (SettingKey, SettingValue, Description) VALUES
    ('resource_manager_seat_limit', N'10', N'Self-registration seats for Resource Managers'),
    ('weekly_target_hours_default', N'40', N'Weekly hour cap for new candidates'),
    ('daily_hours_cap',             N'8',  N'Hours per day before an entry is flagged'),
    ('break_alert_minutes_default', N'60', N'Break length that raises an alert'),
    ('max_backdate_weeks',          N'8',  N'How far back a day can be entered / uploaded');

-- Menus (every nav tab across every role)
INSERT INTO dbo.Menu (MenuCode, MenuName, Description, DisplayOrder) VALUES
    ('dashboard',           N'Dashboard',           N'Submitted, pending and approved counts',   1),
    ('onboarding',          N'Onboarding',          N'Review self-registered candidates',        2),
    ('timesheet_approvals', N'Timesheet Approvals', N'Daily timesheets waiting for a manager',   3),
    ('timesheets',          N'Timesheets',          N'Weekly and monthly totals per candidate',  4),
    ('bulk_upload',         N'Bulk Upload',         N'Upload many candidates'' hours from Excel',5),
    ('people',              N'People',              N'Candidate details and submission timeline',6),
    ('roles_access',        N'Roles & Access',      N'Create users, roles, menus and permissions',7),
    ('activity_log',        N'Activity Log',        N'Every change and simulated email alerts',  8),
    ('my_timesheet',        N'My Timesheet',        N'Enter and submit your own days',           9),
    ('upload_hours',        N'Upload Hours',        N'Upload your own days from Excel',         10),
    ('my_history',          N'My History',          N'Your weekly totals and approval status',  11),
    ('my_profile',          N'My Profile',          N'Your personal details',                   12);

INSERT INTO dbo.Permission (PermissionCode, Description) VALUES
    ('approve_timesheets', N'Approve or reject submitted days'),
    ('edit_timesheets',    N'Correct a candidate''s time entries'),
    ('bulk_upload',        N'Import days for any candidate'),
    ('approve_onboarding', N'Approve or reject new candidates'),
    ('manage_roles_users', N'Create users, roles and permissions');

-- Role names are what the JWT carries; keep in sync with ROLE_HOME_ROUTES in login.ts
INSERT INTO dbo.Role (RoleName, Description, IsBuiltIn) VALUES
    (N'Admin',                 N'Super Administrator - full access to all companies and teams', 1),
    (N'Manager',               N'Approves timesheets for assigned companies/teams',             0),
    (N'HR',                    N'Approves onboarding and manages people',                       0),
    (N'Editor',                N'Edits day-to-day timesheet entries',                           0),
    (N'Contributor View',      N'Read-only view of timesheets and approvals',                   0),
    (N'Resource Manager View', N'Read-only view of resourcing',                                 0),
    (N'Candidate',             N'Clocks in and submits own timesheet',                          1);

-- Role -> permissions
INSERT INTO dbo.RolePermission (RoleId, PermissionId)
SELECT r.RoleId, p.PermissionId FROM dbo.Role r CROSS JOIN dbo.Permission p WHERE r.RoleName = N'Admin';

INSERT INTO dbo.RolePermission (RoleId, PermissionId)
SELECT r.RoleId, p.PermissionId FROM dbo.Role r JOIN dbo.Permission p
  ON p.PermissionCode IN ('approve_timesheets','edit_timesheets') WHERE r.RoleName = N'Manager';

INSERT INTO dbo.RolePermission (RoleId, PermissionId)
SELECT r.RoleId, p.PermissionId FROM dbo.Role r JOIN dbo.Permission p
  ON p.PermissionCode IN ('approve_onboarding','edit_timesheets') WHERE r.RoleName = N'HR';

INSERT INTO dbo.RolePermission (RoleId, PermissionId)
SELECT r.RoleId, p.PermissionId FROM dbo.Role r JOIN dbo.Permission p
  ON p.PermissionCode IN ('edit_timesheets','bulk_upload') WHERE r.RoleName = N'Editor';
-- Contributor View, Resource Manager View, Candidate: no permissions = view only.

-- Role -> menus
INSERT INTO dbo.RoleMenu (RoleId, MenuId)
SELECT r.RoleId, m.MenuId FROM dbo.Role r JOIN dbo.Menu m
  ON m.MenuCode IN ('dashboard','onboarding','timesheet_approvals','timesheets','bulk_upload','people','roles_access','activity_log')
 WHERE r.RoleName = N'Admin';

INSERT INTO dbo.RoleMenu (RoleId, MenuId)
SELECT r.RoleId, m.MenuId FROM dbo.Role r JOIN dbo.Menu m
  ON m.MenuCode IN ('dashboard','timesheet_approvals','timesheets','people','activity_log')
 WHERE r.RoleName = N'Manager';

INSERT INTO dbo.RoleMenu (RoleId, MenuId)
SELECT r.RoleId, m.MenuId FROM dbo.Role r JOIN dbo.Menu m
  ON m.MenuCode IN ('dashboard','onboarding','timesheet_approvals','timesheets','people','activity_log')
 WHERE r.RoleName = N'HR';

INSERT INTO dbo.RoleMenu (RoleId, MenuId)
SELECT r.RoleId, m.MenuId FROM dbo.Role r JOIN dbo.Menu m
  ON m.MenuCode IN ('dashboard','timesheet_approvals','timesheets','bulk_upload','activity_log')
 WHERE r.RoleName = N'Editor';

INSERT INTO dbo.RoleMenu (RoleId, MenuId)
SELECT r.RoleId, m.MenuId FROM dbo.Role r JOIN dbo.Menu m
  ON m.MenuCode IN ('dashboard','timesheet_approvals','timesheets','activity_log')
 WHERE r.RoleName = N'Contributor View';

INSERT INTO dbo.RoleMenu (RoleId, MenuId)
SELECT r.RoleId, m.MenuId FROM dbo.Role r JOIN dbo.Menu m
  ON m.MenuCode IN ('dashboard','timesheets')
 WHERE r.RoleName = N'Resource Manager View';

INSERT INTO dbo.RoleMenu (RoleId, MenuId)
SELECT r.RoleId, m.MenuId FROM dbo.Role r JOIN dbo.Menu m
  ON m.MenuCode IN ('dashboard','my_timesheet','upload_hours','my_history','my_profile')
 WHERE r.RoleName = N'Candidate';
GO

/* =====================================================================
   8. YOUR SUPER ADMIN LOGIN  -  EDIT the two values below, then run.
      Password is stored as SHA2_256 to match AuthService.cs.
      Admin is unscoped (CompanyId/TeamId NULL = every company and team).
   ===================================================================== */
DECLARE @Email    NVARCHAR(254) = N'superadmin@cortracker360.com';   -- <-- change
DECLARE @Password NVARCHAR(100) = N'Demo@123';                       -- <-- change

DECLARE @RoleId INT = (SELECT RoleId FROM dbo.Role WHERE RoleName = N'Admin');

INSERT INTO dbo.AppUser (FullName, Email, PasswordHash, IsActive)
VALUES (N'Super Admin', @Email, HASHBYTES('SHA2_256', @Password), 1);

INSERT INTO dbo.UserRole (AppUserId, RoleId, CompanyId, TeamId)
VALUES (SCOPE_IDENTITY(), @RoleId, NULL, NULL);

SELECT u.AppUserId, u.FullName, u.Email, r.RoleName
FROM dbo.AppUser u
JOIN dbo.UserRole ur ON ur.AppUserId = u.AppUserId
JOIN dbo.Role r ON r.RoleId = ur.RoleId
WHERE u.Email = @Email;
GO
