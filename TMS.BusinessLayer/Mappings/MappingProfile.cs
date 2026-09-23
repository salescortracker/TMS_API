using AutoMapper;
using TMS.BusinessLayer.DTOs;
using TMS.DataAccessLayer.Entities;

namespace TMS.BusinessLayer.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<ActivityLog, ActivityLogDto>().ReverseMap();
        CreateMap<ActivityType, ActivityTypeDto>().ReverseMap();
        CreateMap<AppUser, AppUserDto>().ReverseMap();
        CreateMap<Approver, ApproverDto>().ReverseMap();
        CreateMap<CandidateApproval, CandidateApprovalDto>().ReverseMap();
        CreateMap<CandidateOnboarding, CandidateOnboardingDto>().ReverseMap();
        CreateMap<CandidateWorkSetting, CandidateWorkSettingDto>().ReverseMap();
        CreateMap<Company, CompanyDto>().ReverseMap();
        CreateMap<Permission, PermissionDto>().ReverseMap();
        CreateMap<Role, RoleDto>().ReverseMap();
        CreateMap<RolePermission, RolePermissionDto>().ReverseMap();
        CreateMap<Team, TeamDto>().ReverseMap();
        CreateMap<TimesheetAlert, TimesheetAlertDto>().ReverseMap();
        CreateMap<TimesheetBreak, TimesheetBreakDto>().ReverseMap();
        CreateMap<TimesheetDay, TimesheetDayDto>().ReverseMap();
        CreateMap<TimesheetStatusHistory, TimesheetStatusHistoryDto>().ReverseMap();
        CreateMap<TimesheetUploadBatch, TimesheetUploadBatchDto>().ReverseMap();
        CreateMap<UserRole, UserRoleDto>().ReverseMap();
        CreateMap<VwCandidateWeekSummary, CandidateWeekSummaryDto>();
    }
}
