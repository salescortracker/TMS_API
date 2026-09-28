using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TMS.BusinessLayer.Common;
using TMS.BusinessLayer.DTOs.Workflow;
using TMS.BusinessLayer.Services.Workflow;
using TMS.Utilities.Constants;

namespace TMS.API.Controllers.Workflow;

[ApiController]
[Route("api/approvals")]
public class ApprovalsController : ControllerBase
{
    private readonly IApprovalService _service;
    private readonly ICurrentUser _user;

    public ApprovalsController(IApprovalService service, ICurrentUser user)
    {
        _service = service;
        _user = user;
    }

    [HttpGet]
    [Authorize(Policy = "menu:timesheet_approvals")]
    public async Task<ActionResult<ApprovalListDto>> List([FromQuery] int? companyId, [FromQuery] int? teamId, [FromQuery] string? search) =>
        Ok(await _service.ListAsync(_user.AppUserId, companyId, teamId, search));

    [HttpPost("{dayId:long}/approve")]
    [Authorize(Policy = "perm:approve_timesheets")]
    public async Task<IActionResult> Approve(long dayId)
    {
        await _service.ApproveAsync(_user.AppUserId, dayId);
        return NoContent();
    }

    [HttpPost("{dayId:long}/reject")]
    [Authorize(Policy = "perm:approve_timesheets")]
    public async Task<IActionResult> Reject(long dayId, [FromBody] ApprovalRejectDto dto)
    {
        await _service.RejectAsync(_user.AppUserId, dayId, dto.Reason);
        return NoContent();
    }

    [HttpPut("{dayId:long}")]
    [Authorize(Policy = "perm:edit_timesheets")]
    public async Task<ActionResult<ApprovalEntryDto>> Edit(long dayId, [FromBody] ApprovalEditDto dto) =>
        Ok(await _service.EditAsync(_user.AppUserId, dayId, dto));

    [HttpPost("approve-many")]
    [Authorize(Policy = "perm:approve_timesheets")]
    public async Task<ActionResult<ApproveManyResultDto>> ApproveMany([FromBody] ApproveManyDto dto) =>
        Ok(await _service.ApproveManyAsync(_user.AppUserId, dto.Ids));
}

[ApiController]
[Route("api/dashboard")]
[Authorize(Policy = "menu:dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _service;
    private readonly ICurrentUser _user;

    public DashboardController(IDashboardService service, ICurrentUser user)
    {
        _service = service;
        _user = user;
    }

    [HttpGet]
    public async Task<ActionResult<DashboardDto>> Get([FromQuery] int? companyId, [FromQuery] int? teamId) =>
        Ok(await _service.GetAsync(_user.AppUserId, companyId, teamId));
}

[ApiController]
[Route("api/timesheets")]
[Authorize(Policy = "menu:timesheets")]
public class TimesheetReportController : ControllerBase
{
    private readonly IReportService _service;
    private readonly ICurrentUser _user;

    public TimesheetReportController(IReportService service, ICurrentUser user)
    {
        _service = service;
        _user = user;
    }

    [HttpGet]
    public async Task<ActionResult<TimesheetReportDto>> Get([FromQuery] string? mode, [FromQuery] string? period,
        [FromQuery] int? companyId, [FromQuery] int? teamId, [FromQuery] string? search) =>
        Ok(await _service.GetAsync(_user.AppUserId, mode, period, companyId, teamId, search));

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] string? mode, [FromQuery] string? period,
        [FromQuery] int? companyId, [FromQuery] int? teamId, [FromQuery] string? search)
    {
        var bytes = await _service.ExportAsync(_user.AppUserId, mode, period, companyId, teamId, search);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "timesheets.xlsx");
    }
}

[ApiController]
[Route("api/people")]
[Authorize(Policy = "menu:people")]
public class PeopleController : ControllerBase
{
    private readonly IPeopleService _service;
    private readonly ICurrentUser _user;

    public PeopleController(IPeopleService service, ICurrentUser user)
    {
        _service = service;
        _user = user;
    }

    [HttpGet]
    public async Task<ActionResult<List<PersonListItemDto>>> List([FromQuery] string? search, [FromQuery] int? companyId, [FromQuery] int? teamId) =>
        Ok(await _service.ListAsync(_user.AppUserId, search, companyId, teamId));

    [HttpGet("staff")]
    public async Task<ActionResult<List<StaffMemberDto>>> Staff([FromQuery] string? search) =>
        Ok(await _service.StaffAsync(search));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PersonDetailDto>> Get(int id) => Ok(await _service.GetAsync(_user.AppUserId, id));
}

[ApiController]
[Route("api/activity")]
public class ActivityController : ControllerBase
{
    private readonly IActivityService _service;
    private readonly ICurrentUser _user;

    public ActivityController(IActivityService service, ICurrentUser user)
    {
        _service = service;
        _user = user;
    }

    [HttpGet]
    [Authorize(Policy = "menu:activity_log")]
    public async Task<ActionResult<List<ActivityEntryDto>>> List([FromQuery] string? category, [FromQuery] string? search, [FromQuery] int take = 100) =>
        Ok(await _service.ListAsync(_user.AppUserId, category, search, take));

    [HttpGet("emails")]
    [Authorize(Policy = "menu:activity_log")]
    public async Task<ActionResult<List<EmailAlertDto>>> Emails([FromQuery] int take = 50) => Ok(await _service.EmailsAsync(take));

    [HttpGet("~/api/notifications")]
    public async Task<ActionResult<NotificationsDto>> Notifications() => Ok(await _service.NotificationsAsync(_user.AppUserId));

    [HttpPost("~/api/notifications/read")]
    public async Task<IActionResult> MarkAllRead()
    {
        await _service.MarkReadAsync(_user.AppUserId, null);
        return NoContent();
    }

    [HttpPost("~/api/notifications/{id:long}/read")]
    public async Task<IActionResult> MarkRead(long id)
    {
        await _service.MarkReadAsync(_user.AppUserId, id);
        return NoContent();
    }
}

[ApiController]
[Route("api/access")]
[Authorize(Policy = "perm:manage_roles_users")]
public class AccessController : ControllerBase
{
    private readonly IAccessAdminService _service;
    private readonly ICurrentUser _user;

    public AccessController(IAccessAdminService service, ICurrentUser user)
    {
        _service = service;
        _user = user;
    }

    [HttpGet("roles")]
    public async Task<ActionResult<RolesOverviewDto>> Roles() => Ok(await _service.GetRolesAsync());

    [HttpPost("roles")]
    public async Task<ActionResult<RoleSummaryDto>> CreateRole([FromBody] RoleSaveDto dto) =>
        Ok(await _service.CreateRoleAsync(_user.AppUserId, dto));

    [HttpPut("roles/{id:int}")]
    public async Task<ActionResult<RoleSummaryDto>> UpdateRole(int id, [FromBody] RoleSaveDto dto) =>
        Ok(await _service.UpdateRoleAsync(_user.AppUserId, id, dto));

    [HttpDelete("roles/{id:int}")]
    public async Task<IActionResult> DeleteRole(int id)
    {
        await _service.DeleteRoleAsync(_user.AppUserId, id);
        return NoContent();
    }

    [HttpGet("users")]
    public async Task<ActionResult<List<StaffUserDto>>> Users([FromQuery] string? search) => Ok(await _service.GetUsersAsync(search));

    [HttpPost("users")]
    public async Task<ActionResult<UserCreatedDto>> AddUser([FromBody] UserAddDto dto) =>
        Ok(await _service.AddUserAsync(_user.AppUserId, dto));

    [HttpPut("users/{id:int}/role")]
    public async Task<IActionResult> ChangeRole(int id, [FromBody] UserChangeRoleDto dto)
    {
        await _service.ChangeRoleAsync(_user.AppUserId, id, dto);
        return NoContent();
    }

    [HttpPost("users/{id:int}/activate")]
    public async Task<IActionResult> Activate(int id)
    {
        await _service.SetActiveAsync(_user.AppUserId, id, true);
        return NoContent();
    }

    [HttpPost("users/{id:int}/deactivate")]
    public async Task<IActionResult> Deactivate(int id)
    {
        await _service.SetActiveAsync(_user.AppUserId, id, false);
        return NoContent();
    }

    [HttpPost("users/{id:int}/reset-password")]
    public async Task<ActionResult<UserCreatedDto>> ResetPassword(int id) =>
        Ok(await _service.ResetPasswordAsync(_user.AppUserId, id));
}

[ApiController]
[Route("api/uploads")]
public class UploadsController : ControllerBase
{
    private const string Xlsx = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private readonly IBulkUploadService _service;
    private readonly ICurrentUser _user;

    public UploadsController(IBulkUploadService service, ICurrentUser user)
    {
        _service = service;
        _user = user;
    }

    private bool IsCandidate => _user.IsInRole(RoleNames.Candidate);

    private bool CanUpload => IsCandidate || _user.HasPermission(PermissionCodes.BulkUpload);

    [HttpGet("template")]
    public async Task<IActionResult> Template([FromQuery] string kind = "blank")
    {
        if (!CanUpload) return Forbid();
        var name = kind is "valid" or "errors" ? $"timesheet-{kind}-sample.xlsx" : "timesheet-template.xlsx";
        return File(await _service.BuildTemplateAsync(_user.AppUserId, IsCandidate, kind), Xlsx, name);
    }

    [HttpPost]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<ActionResult<UploadResultDto>> Upload(IFormFile file)
    {
        if (!CanUpload) return Forbid();
        if (file is null || file.Length == 0) return BadRequest(new { message = "Choose an Excel file first." });
        await using var stream = file.OpenReadStream();
        using var copy = new MemoryStream();
        await stream.CopyToAsync(copy);
        copy.Position = 0;
        return Ok(await _service.ImportAsync(_user.AppUserId, IsCandidate, file.FileName, copy));
    }

    [HttpGet("history")]
    public async Task<ActionResult<List<UploadBatchDto>>> History()
    {
        if (!CanUpload) return Forbid();
        return Ok(await _service.HistoryAsync(_user.AppUserId, IsCandidate));
    }
}
