using System.Globalization;
using Microsoft.EntityFrameworkCore;
using TMS.BusinessLayer.Common;
using TMS.BusinessLayer.DTOs.Workflow;
using TMS.DataAccessLayer.Context;
using TMS.DataAccessLayer.Entities;
using TMS.Utilities.Constants;
using TMS.Utilities.Exceptions;

namespace TMS.BusinessLayer.Services.Workflow;

public interface IOnboardingService
{
    // Public (no login)
    Task<OnboardingResultDto> SubmitCandidateAsync(OnboardingSubmitDto dto);

    Task<OnboardingResultDto> RegisterManagerAsync(ManagerRegistrationDto dto);

    Task<SeatInfoDto> GetSeatsAsync();

    // Staff (approve_onboarding)
    Task<ApplicantListDto> ListAsync(int actorUserId);

    Task<ActivationResultDto> ApproveAsync(int actorUserId, int candidateId);

    Task<ActivationResultDto> RejectAsync(int actorUserId, int candidateId, string? reason);

    Task<BulkActivationResultDto> ApproveAllPendingAsync(int actorUserId);
}

public class OnboardingService : IOnboardingService
{
    private static readonly string[] TempDomains =
    {
        "tempmail.com", "mailinator.com", "10minutemail.com", "guerrillamail.com",
        "yopmail.com", "trashmail.com", "throwawaymail.com",
    };

    private readonly TmsDbContext _db;
    private readonly IScopeService _scope;
    private readonly IAuditService _audit;
    private readonly ITimesheetCore _core;

    public OnboardingService(TmsDbContext db, IScopeService scope, IAuditService audit, ITimesheetCore core)
    {
        _db = db;
        _scope = scope;
        _audit = audit;
        _core = core;
    }

    // ---------------- public submissions ----------------

    public async Task<OnboardingResultDto> SubmitCandidateAsync(OnboardingSubmitDto dto)
    {
        if (dto.SubmitForApproval)
        {
            if (dto.DateOfBirth is null || dto.DateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-16)))
            {
                throw new BusinessRuleException("Enter a valid date of birth (you must be at least 16).");
            }

            if (dto.JoiningDate is null)
            {
                throw new BusinessRuleException("Joining date is required.");
            }

            if (dto.TeamId is null || dto.CompanyId is null)
            {
                throw new BusinessRuleException("Team and company are required.");
            }

            if (string.IsNullOrWhiteSpace(dto.PhoneNumber))
            {
                throw new BusinessRuleException("Phone number is required.");
            }
        }

        return await SaveRequestAsync(
            "Candidate", dto.FirstName, dto.LastName, dto.Email, dto.DateOfBirth, dto.JoiningDate,
            dto.TeamId, dto.CompanyId, dto.PhoneDialCode, dto.PhoneNumber, dto.SubmitForApproval);
    }

    public async Task<OnboardingResultDto> RegisterManagerAsync(ManagerRegistrationDto dto)
    {
        if (dto.SubmitForApproval)
        {
            var seats = await GetSeatsAsync();
            if (seats.Used >= seats.Total)
            {
                throw new BusinessRuleException($"All {seats.Total} self-registration seats are taken. Ask an administrator to add you directly.");
            }
        }

        return await SaveRequestAsync(
            "Resource Manager", dto.FirstName, dto.LastName, dto.Email, null, null,
            dto.TeamId, dto.CompanyId, dto.PhoneDialCode, dto.PhoneNumber, dto.SubmitForApproval);
    }

    public async Task<SeatInfoDto> GetSeatsAsync()
    {
        var total = await _core.GetIntSettingAsync(SettingKeys.ResourceManagerSeatLimit, 10);
        var used = await _db.CandidateOnboardings.CountAsync(c =>
            c.RequestType == "Resource Manager" &&
            (c.Status == OnboardingStatus.Pending || c.Status == OnboardingStatus.Approved));
        return new SeatInfoDto { Used = used, Total = total };
    }

    private async Task<OnboardingResultDto> SaveRequestAsync(
        string requestType, string firstName, string lastName, string email,
        DateOnly? dob, DateOnly? joining, int? teamId, int? companyId,
        string? dialCode, string? phone, bool submit)
    {
        email = email.Trim();
        firstName = firstName.Trim();
        lastName = lastName.Trim();

        if (await _db.AppUsers.AnyAsync(u => u.Email == email))
        {
            throw new BusinessRuleException("An account with this email already exists. Sign in instead.");
        }

        var request = await _db.CandidateOnboardings.FirstOrDefaultAsync(c => c.Email == email);
        if (request is not null && request.Status != OnboardingStatus.Draft && request.Status != OnboardingStatus.Rejected)
        {
            throw new BusinessRuleException($"A request for this email is already {request.Status.ToLowerInvariant()}.");
        }

        if (request is null)
        {
            request = new CandidateOnboarding { Email = email, CreatedAt = DateTime.UtcNow };
            _db.CandidateOnboardings.Add(request);
        }

        request.RequestType = requestType;
        request.FirstName = firstName;
        request.LastName = lastName;
        request.DateOfBirth = dob;
        request.JoiningDate = joining;
        request.TeamId = teamId;
        request.CompanyId = companyId;
        request.PhoneDialCode = dialCode;
        request.PhoneNumber = phone;
        request.UpdatedAt = DateTime.UtcNow;
        request.IsActive = true;
        request.Status = submit ? OnboardingStatus.Pending : OnboardingStatus.Draft;
        request.SubmittedAt = submit ? DateTime.UtcNow : null;
        request.FlagReason = await DetectFlagAsync(firstName, lastName, email, companyId);
        await _db.SaveChangesAsync();

        if (submit)
        {
            var name = $"{firstName} {lastName}";
            var logId = await _audit.LogAsync(new AuditEntry(
                null, LogCategory.Submissions, Severity.Info, "onboarding_submitted", "CandidateOnboarding",
                request.CandidateId, request.CandidateId,
                $"{name} submitted a {requestType.ToLowerInvariant()} request for approval",
                request.FlagReason));
            await _audit.EmailAsync(null, $"{name} <{email}>", "Your request was received and is awaiting approval", logId);
            await _audit.NotifyPermissionHoldersAsync(
                PermissionCodes.ApproveOnboarding, request.CandidateId,
                $"New {requestType.ToLowerInvariant()} request", $"{name} is waiting for approval.", "/super-admin/onboarding");
        }

        return new OnboardingResultDto
        {
            CandidateId = request.CandidateId,
            Status = request.Status,
            Message = submit
                ? "Submitted. You'll get an email once it is reviewed."
                : "Draft saved. Submit it when you're ready.",
        };
    }

    private async Task<string?> DetectFlagAsync(string first, string last, string email, int? companyId)
    {
        if (first.Any(char.IsDigit) || last.Any(char.IsDigit))
        {
            return "Name contains digits";
        }

        var domain = email[(email.LastIndexOf('@') + 1)..].ToLowerInvariant();
        if (TempDomains.Contains(domain))
        {
            return "Personal or temporary email domain";
        }

        if (companyId is not null)
        {
            var companyDomain = await _db.Companies.Where(c => c.CompanyId == companyId)
                .Select(c => c.EmailDomain).FirstOrDefaultAsync();
            if (!string.IsNullOrWhiteSpace(companyDomain) &&
                !domain.Equals(companyDomain, StringComparison.OrdinalIgnoreCase))
            {
                return "Personal or temporary email domain";
            }
        }

        return null;
    }

    // ---------------- staff side ----------------

    public async Task<ApplicantListDto> ListAsync(int actorUserId)
    {
        var allowed = await _scope.AllowedCandidateIdsAsync(actorUserId);
        var query = _db.CandidateOnboardings
            .Include(c => c.Team).Include(c => c.Company)
            .Where(c => c.Status != OnboardingStatus.Draft);

        if (allowed is not null)
        {
            query = query.Where(c => allowed.Contains(c.CandidateId));
        }

        var rows = await query.OrderByDescending(c => c.SubmittedAt).ToListAsync();
        var seats = await GetSeatsAsync();

        return new ApplicantListDto
        {
            SeatsUsed = seats.Used,
            SeatsTotal = seats.Total,
            Items = rows.Select(c => new ApplicantDto
            {
                Id = c.CandidateId.ToString(),
                Name = $"{c.FirstName} {c.LastName}",
                Email = c.Email ?? string.Empty,
                Phone = $"{c.PhoneDialCode} {c.PhoneNumber}".Trim(),
                Type = $"{c.RequestType} · {c.Team?.TeamName}".TrimEnd(' ', '·'),
                Team = c.Team?.TeamName ?? string.Empty,
                Company = c.Company?.CompanyName ?? "Others",
                Submitted = (c.SubmittedAt ?? c.CreatedAt).ToString("MMM d, yyyy", CultureInfo.InvariantCulture),
                Flag = c.FlagReason,
                Status = c.Status,
            }).ToList(),
        };
    }

    public async Task<ActivationResultDto> ApproveAsync(int actorUserId, int candidateId)
    {
        var c = await LoadPendingAsync(actorUserId, candidateId);
        var name = $"{c.FirstName} {c.LastName}";
        var email = c.Email ?? throw new BusinessRuleException("This request has no email address.");

        if (await _db.AppUsers.AnyAsync(u => u.Email == email))
        {
            throw new BusinessRuleException("An account with this email already exists.");
        }

        var temporary = PasswordHasher.GenerateTemporary();
        var roleName = c.RequestType == "Resource Manager" ? RoleNames.ResourceManagerView : RoleNames.Candidate;
        var role = await _db.Roles.FirstAsync(r => r.RoleName == roleName);

        var user = new AppUser
        {
            FullName = name,
            Email = email,
            PasswordHash = PasswordHasher.Hash(temporary),
            MustChangePassword = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };
        user.UserRoles.Add(new UserRole
        {
            RoleId = role.RoleId, CompanyId = c.CompanyId, TeamId = c.TeamId, GrantedAt = DateTime.UtcNow,
        });
        _db.AppUsers.Add(user);

        c.AppUser = user;
        c.Status = OnboardingStatus.Approved;
        c.ActivatedAt = DateTime.UtcNow;
        c.UpdatedAt = DateTime.UtcNow;

        if (c.RequestType == "Candidate" && !await _db.CandidateWorkSettings.AnyAsync(s => s.CandidateId == c.CandidateId))
        {
            _db.CandidateWorkSettings.Add(new CandidateWorkSetting
            {
                Candidate = c,
                WeeklyTargetHours = await _core.GetIntSettingAsync(SettingKeys.WeeklyTargetHours, 40),
                BreakAlertMinutes = await _core.GetIntSettingAsync(SettingKeys.BreakAlertMinutes, 60),
                WeekStartDay = 1,
                TimeZoneId = ZoneFor(c.Team?.TeamName),
            });
        }

        var approverId = await _core.EnsureApproverAsync(actorUserId);
        _db.CandidateApprovals.Add(new CandidateApproval
        {
            CandidateId = c.CandidateId, ApproverId = approverId, Decision = "Approved",
            DecidedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow,
        });
        await _db.SaveChangesAsync();

        var actor = await _db.AppUsers.Where(u => u.AppUserId == actorUserId).Select(u => u.FullName).FirstAsync();
        var logId = await _audit.LogAsync(new AuditEntry(
            actorUserId, LogCategory.Approvals, Severity.Success, "onboarding_approved", "CandidateOnboarding",
            c.CandidateId, c.CandidateId, $"{actor} approved candidate {name}", null));
        await _audit.EmailAsync(user.AppUserId, $"{name} <{email}>", "Your account is approved - you can now sign in", logId);

        return new ActivationResultDto
        {
            Id = c.CandidateId.ToString(), Name = name, Email = email, Status = c.Status,
            TemporaryPassword = temporary,
            Message = $"{name} approved. Share the temporary password - they must change it at first sign-in.",
        };
    }

    public async Task<ActivationResultDto> RejectAsync(int actorUserId, int candidateId, string? reason)
    {
        var c = await LoadPendingAsync(actorUserId, candidateId);
        var name = $"{c.FirstName} {c.LastName}";
        c.Status = OnboardingStatus.Rejected;
        c.UpdatedAt = DateTime.UtcNow;

        var approverId = await _core.EnsureApproverAsync(actorUserId);
        _db.CandidateApprovals.Add(new CandidateApproval
        {
            CandidateId = c.CandidateId, ApproverId = approverId, Decision = "Rejected",
            Comments = reason, DecidedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow,
        });
        await _db.SaveChangesAsync();

        var actor = await _db.AppUsers.Where(u => u.AppUserId == actorUserId).Select(u => u.FullName).FirstAsync();
        var logId = await _audit.LogAsync(new AuditEntry(
            actorUserId, LogCategory.Approvals, Severity.Danger, "onboarding_rejected", "CandidateOnboarding",
            c.CandidateId, c.CandidateId, $"{actor} rejected candidate \"{name}\"",
            reason ?? c.FlagReason));
        await _audit.EmailAsync(null, $"\"{name}\" <{c.Email}>", "Your onboarding request was not approved", logId);

        return new ActivationResultDto
        {
            Id = c.CandidateId.ToString(), Name = name, Email = c.Email ?? string.Empty,
            Status = c.Status, Message = $"{name} was rejected.",
        };
    }

    public async Task<BulkActivationResultDto> ApproveAllPendingAsync(int actorUserId)
    {
        var allowed = await _scope.AllowedCandidateIdsAsync(actorUserId);
        var query = _db.CandidateOnboardings.Where(c => c.Status == OnboardingStatus.Pending);
        if (allowed is not null)
        {
            query = query.Where(c => allowed.Contains(c.CandidateId));
        }

        var pending = await query.ToListAsync();
        var result = new BulkActivationResultDto();

        foreach (var c in pending)
        {
            if (c.FlagReason is not null)
            {
                result.SkippedFlagged++;
                continue;
            }

            try
            {
                result.Results.Add(await ApproveAsync(actorUserId, c.CandidateId));
                result.Approved++;
            }
            catch (BusinessRuleException)
            {
                result.SkippedFlagged++;
            }
        }

        return result;
    }

    private async Task<CandidateOnboarding> LoadPendingAsync(int actorUserId, int candidateId)
    {
        var c = await _db.CandidateOnboardings.Include(x => x.Team).Include(x => x.Company)
                    .FirstOrDefaultAsync(x => x.CandidateId == candidateId)
                ?? throw new NotFoundException("Request not found.");

        if (!await _scope.CanAccessCandidateAsync(actorUserId, candidateId))
        {
            throw new ForbiddenException("This request is outside your company/team scope.");
        }

        if (c.Status != OnboardingStatus.Pending)
        {
            throw new BusinessRuleException($"This request is already {c.Status.ToLowerInvariant()}.");
        }

        return c;
    }

    private static string ZoneFor(string? teamName) => teamName switch
    {
        not null when teamName.Contains("India", StringComparison.OrdinalIgnoreCase) => "Asia/Kolkata",
        not null when teamName.Contains("USA", StringComparison.OrdinalIgnoreCase) => "America/New_York",
        not null when teamName.Contains("UK", StringComparison.OrdinalIgnoreCase) => "Europe/London",
        _ => "UTC",
    };
}
