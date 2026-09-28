USE TMS_DB;
GO

-- EDIT the email and password below, then run this whole block.
-- The password is hashed with SHA2_256 here so it matches exactly how
-- AuthService.cs will verify it later — never store it as plain text.

DECLARE @Email    NVARCHAR(254) = N'superadmin@cortracker360.com';
DECLARE @Password NVARCHAR(100) = N'Demo@123';   -- <-- change this

DECLARE @CompanyId INT = (SELECT CompanyId FROM dbo.Company WHERE CompanyName = N'Cortracker Inc');
DECLARE @TeamId    INT = (SELECT TeamId FROM dbo.Team WHERE TeamName = N'USA team');
DECLARE @RoleId    INT = (SELECT RoleId FROM dbo.Role WHERE RoleName = 'Admin');

INSERT INTO dbo.AppUser (FullName, Email, PasswordHash, IsActive)
VALUES (N'Super Admin', @Email, HASHBYTES('SHA2_256', @Password), 1);

DECLARE @AppUserId INT = SCOPE_IDENTITY();

INSERT INTO dbo.UserRole (AppUserId, RoleId, CompanyId, TeamId)
VALUES (@AppUserId, @RoleId, @CompanyId, @TeamId);

-- Verify:
SELECT u.AppUserId, u.FullName, u.Email, r.RoleName
FROM dbo.AppUser u
JOIN dbo.UserRole ur ON ur.AppUserId = u.AppUserId
JOIN dbo.Role r ON r.RoleId = ur.RoleId
WHERE u.Email = @Email;
