namespace TMS.Utilities.Constants;

public static class RoleNames
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string HR = "HR";
    public const string Editor = "Editor";
    public const string ContributorView = "Contributor View";
    public const string ResourceManagerView = "Resource Manager View";
    public const string Candidate = "Candidate";
}

public static class PermissionCodes
{
    public const string ApproveTimesheets = "approve_timesheets";
    public const string EditTimesheets = "edit_timesheets";
    public const string BulkUpload = "bulk_upload";
    public const string ApproveOnboarding = "approve_onboarding";
    public const string ManageRolesUsers = "manage_roles_users";

    public static readonly string[] All =
        { ApproveTimesheets, EditTimesheets, BulkUpload, ApproveOnboarding, ManageRolesUsers };
}

public static class MenuCodes
{
    public static readonly string[] All =
    {
        "dashboard", "onboarding", "timesheet_approvals", "timesheets", "bulk_upload", "people",
        "roles_access", "activity_log", "my_timesheet", "upload_hours", "my_history", "my_profile",
    };
}

public static class DayStatus
{
    public const string Draft = "Draft";
    public const string Saved = "Saved";
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
}

public static class OnboardingStatus
{
    public const string Draft = "Draft";
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
}

public static class LogCategory
{
    public const string Submissions = "Submissions";
    public const string Approvals = "Approvals";
    public const string EditsRoles = "Edits & roles";
    public const string Alerts = "Alerts";
}

public static class Severity
{
    public const string Info = "Info";
    public const string Success = "Success";
    public const string Warning = "Warning";
    public const string Danger = "Danger";
}

public static class SettingKeys
{
    public const string ResourceManagerSeatLimit = "resource_manager_seat_limit";
    public const string WeeklyTargetHours = "weekly_target_hours_default";
    public const string DailyHoursCap = "daily_hours_cap";
    public const string BreakAlertMinutes = "break_alert_minutes_default";
    public const string MaxBackdateWeeks = "max_backdate_weeks";
}
