USE TMS_DB;
GO

INSERT INTO dbo.Company (CompanyName) VALUES
    (N'Cortracker Inc'),
    (N'Perfect Solutions Group Inc');
GO

INSERT INTO dbo.Team (TeamName, DialCode) VALUES
    (N'USA team', '+1'),
    (N'India team', '+91');
GO

INSERT INTO dbo.Role (RoleName, Description) VALUES
    (N'Admin',     N'Full access to all companies and teams'),
    (N'Manager',   N'Approves timesheets for assigned companies/teams'),
    (N'HR',        N'Approves onboarding and manages people'),
    (N'Candidate', N'Clocks in and submits own timesheet');
GO

INSERT INTO dbo.Permission (PermissionCode, Description) VALUES
    ('approve_timesheets', N'Approve timesheets'),
    ('edit_timesheets',    N'Edit timesheets'),
    ('bulk_upload',        N'Bulk upload'),
    ('approve_onboarding', N'Approve onboarding'),
    ('manage_roles_users', N'Manage roles & users');
GO

-- Admin gets everything
INSERT INTO dbo.RolePermission (RoleId, PermissionId)
SELECT r.RoleId, p.PermissionId FROM dbo.Role r CROSS JOIN dbo.Permission p
WHERE r.RoleName = 'Admin';
GO
