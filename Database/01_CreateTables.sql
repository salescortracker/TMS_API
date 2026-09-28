-- =====================================================================
-- TMS_DB — tables needed for login (matches the existing C# entities
-- in TMS.DataAccessLayer\Entities exactly — column names/types below
-- are taken directly from AppUser.cs, Role.cs, UserRole.cs, etc.
-- Run this against your fresh local database.
-- =====================================================================
USE TMS_DB;
GO

CREATE TABLE dbo.Company (
    CompanyId    INT IDENTITY(1,1) NOT NULL,
    CompanyName  NVARCHAR(150)     NOT NULL,
    EmailDomain  NVARCHAR(100)     NULL,
    IsActive     BIT               NOT NULL CONSTRAINT DF_Company_IsActive DEFAULT (1),
    CreatedAt    DATETIME2(0)      NOT NULL CONSTRAINT DF_Company_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_Company PRIMARY KEY (CompanyId),
    CONSTRAINT UQ_Company_CompanyName UNIQUE (CompanyName)
);
GO

CREATE TABLE dbo.Team (
    TeamId      INT IDENTITY(1,1) NOT NULL,
    TeamName    NVARCHAR(100)     NOT NULL,
    DialCode    VARCHAR(6)        NOT NULL,
    IsActive    BIT               NOT NULL CONSTRAINT DF_Team_IsActive DEFAULT (1),
    CreatedAt   DATETIME2(0)      NOT NULL CONSTRAINT DF_Team_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_Team PRIMARY KEY (TeamId),
    CONSTRAINT UQ_Team_TeamName UNIQUE (TeamName)
);
GO

CREATE TABLE dbo.AppUser (
    AppUserId     INT IDENTITY(1,1) NOT NULL,
    FullName      NVARCHAR(150)     NOT NULL,
    Email         NVARCHAR(254)     NOT NULL,
    PasswordHash  VARBINARY(256)    NULL,
    IsActive      BIT               NOT NULL CONSTRAINT DF_AppUser_IsActive DEFAULT (1),
    LastLoginAt   DATETIME2(0)      NULL,
    CreatedAt     DATETIME2(0)      NOT NULL CONSTRAINT DF_AppUser_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_AppUser PRIMARY KEY (AppUserId),
    CONSTRAINT UQ_AppUser_Email UNIQUE (Email)
);
GO

CREATE TABLE dbo.Role (
    RoleId       INT IDENTITY(1,1) NOT NULL,
    RoleName     NVARCHAR(50)      NOT NULL,
    Description  NVARCHAR(250)     NULL,
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
    CONSTRAINT FK_RolePermission_Role FOREIGN KEY (RoleId) REFERENCES dbo.Role (RoleId),
    CONSTRAINT FK_RolePermission_Permission FOREIGN KEY (PermissionId) REFERENCES dbo.Permission (PermissionId)
);
GO

CREATE TABLE dbo.UserRole (
    UserRoleId  INT IDENTITY(1,1) NOT NULL,
    AppUserId   INT               NOT NULL,
    RoleId      INT               NOT NULL,
    CompanyId   INT               NULL,
    TeamId      INT               NULL,
    GrantedAt   DATETIME2(0)      NOT NULL CONSTRAINT DF_UserRole_GrantedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_UserRole PRIMARY KEY (UserRoleId),
    CONSTRAINT FK_UserRole_User FOREIGN KEY (AppUserId) REFERENCES dbo.AppUser (AppUserId),
    CONSTRAINT FK_UserRole_Role FOREIGN KEY (RoleId) REFERENCES dbo.Role (RoleId),
    CONSTRAINT FK_UserRole_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company (CompanyId),
    CONSTRAINT FK_UserRole_Team FOREIGN KEY (TeamId) REFERENCES dbo.Team (TeamId),
    CONSTRAINT UX_UserRole_Scope UNIQUE (AppUserId, RoleId, CompanyId, TeamId)
);
GO
