using Microsoft.Extensions.DependencyInjection;
using TMS.BusinessLayer.Common;
using TMS.BusinessLayer.Mappings;
using TMS.BusinessLayer.Services.Workflow;
using TMS.BusinessLayer.Services.Implementations;
using TMS.BusinessLayer.Services.Interfaces;

namespace TMS.BusinessLayer;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLayer(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => { }, typeof(MappingProfile));

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IActivityLogService, ActivityLogService>();
        services.AddScoped<IActivityTypeService, ActivityTypeService>();
        services.AddScoped<IAppUserService, AppUserService>();
        services.AddScoped<IApproverService, ApproverService>();
        services.AddScoped<ICandidateApprovalService, CandidateApprovalService>();
        services.AddScoped<ICandidateOnboardingService, CandidateOnboardingService>();
        services.AddScoped<ICandidateWorkSettingService, CandidateWorkSettingService>();
        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IRolePermissionService, RolePermissionService>();
        services.AddScoped<ITeamService, TeamService>();
        services.AddScoped<ITimesheetAlertService, TimesheetAlertService>();
        services.AddScoped<ITimesheetBreakService, TimesheetBreakService>();
        services.AddScoped<ITimesheetDayService, TimesheetDayService>();
        services.AddScoped<ITimesheetStatusHistoryService, TimesheetStatusHistoryService>();
        services.AddScoped<ITimesheetUploadBatchService, TimesheetUploadBatchService>();
        services.AddScoped<IUserRoleService, UserRoleService>();
        services.AddScoped<ICandidateWeekSummaryService, CandidateWeekSummaryService>();

        services.AddScoped<IScopeService, ScopeService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<ITimesheetCore, TimesheetCore>();
        services.AddScoped<ILookupService, LookupService>();
        services.AddScoped<IOnboardingService, OnboardingService>();
        services.AddScoped<IMyTimesheetService, MyTimesheetService>();
        services.AddScoped<IClockService, ClockService>();
        services.AddScoped<IApprovalService, ApprovalService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IPeopleService, PeopleService>();
        services.AddScoped<IAccessAdminService, AccessAdminService>();
        services.AddScoped<IActivityService, ActivityService>();
        services.AddScoped<IBulkUploadService, BulkUploadService>();

        return services;
    }
}
