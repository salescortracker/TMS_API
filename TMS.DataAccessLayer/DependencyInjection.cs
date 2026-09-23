using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TMS.DataAccessLayer.Context;
using TMS.DataAccessLayer.Repositories.Implementations;
using TMS.DataAccessLayer.Repositories.Interfaces;

namespace TMS.DataAccessLayer;

public static class DependencyInjection
{
    public static IServiceCollection AddDataAccessLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TmsDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IActivityLogRepository, ActivityLogRepository>();
        services.AddScoped<IActivityTypeRepository, ActivityTypeRepository>();
        services.AddScoped<IAppUserRepository, AppUserRepository>();
        services.AddScoped<IApproverRepository, ApproverRepository>();
        services.AddScoped<ICandidateApprovalRepository, CandidateApprovalRepository>();
        services.AddScoped<ICandidateOnboardingRepository, CandidateOnboardingRepository>();
        services.AddScoped<ICandidateWorkSettingRepository, CandidateWorkSettingRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
        services.AddScoped<ITeamRepository, TeamRepository>();
        services.AddScoped<ITimesheetAlertRepository, TimesheetAlertRepository>();
        services.AddScoped<ITimesheetBreakRepository, TimesheetBreakRepository>();
        services.AddScoped<ITimesheetDayRepository, TimesheetDayRepository>();
        services.AddScoped<ITimesheetStatusHistoryRepository, TimesheetStatusHistoryRepository>();
        services.AddScoped<ITimesheetUploadBatchRepository, TimesheetUploadBatchRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        services.AddScoped<ICandidateWeekSummaryRepository, CandidateWeekSummaryRepository>();

        return services;
    }
}
