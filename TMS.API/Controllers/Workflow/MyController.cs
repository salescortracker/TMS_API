using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TMS.BusinessLayer.Common;
using TMS.BusinessLayer.DTOs.Workflow;
using TMS.BusinessLayer.Services.Workflow;

namespace TMS.API.Controllers.Workflow;

/// <summary>Everything a candidate does with their own timesheet, clock and profile.</summary>
[ApiController]
[Authorize(Policy = "Candidate")]
[Route("api/my")]
public class MyController : ControllerBase
{
    private readonly IMyTimesheetService _timesheets;
    private readonly IClockService _clock;
    private readonly ICurrentUser _user;

    public MyController(IMyTimesheetService timesheets, IClockService clock, ICurrentUser user)
    {
        _timesheets = timesheets;
        _clock = clock;
        _user = user;
    }

    [HttpGet("week")]
    public async Task<ActionResult<WeekDto>> Week([FromQuery] DateOnly? weekStart) =>
        Ok(await _timesheets.GetWeekAsync(_user.AppUserId, weekStart));

    [HttpPut("days/{date}")]
    public async Task<ActionResult<DayDto>> SaveDay(DateOnly date, [FromBody] SaveDayRequestDto dto) =>
        Ok(await _timesheets.SaveDayAsync(_user.AppUserId, date, dto));

    [HttpPost("days/{date}/submit")]
    public async Task<ActionResult<DayDto>> SubmitDay(DateOnly date) =>
        Ok(await _timesheets.SubmitDayAsync(_user.AppUserId, date));

    [HttpPost("submit-ready")]
    public async Task<ActionResult<SubmitResultDto>> SubmitReady([FromQuery] DateOnly? weekStart) =>
        Ok(await _timesheets.SubmitReadyAsync(_user.AppUserId, weekStart));

    [HttpGet("history")]
    public async Task<ActionResult<List<HistoryWeekDto>>> History() =>
        Ok(await _timesheets.GetHistoryAsync(_user.AppUserId));

    [HttpGet("dashboard")]
    public async Task<ActionResult<CandidateDashboardDto>> Dashboard() =>
        Ok(await _timesheets.GetDashboardAsync(_user.AppUserId));

    [HttpGet("profile")]
    public async Task<ActionResult<ProfileDto>> Profile() => Ok(await _timesheets.GetProfileAsync(_user.AppUserId));

    [HttpPut("profile")]
    public async Task<ActionResult<ProfileDto>> UpdateProfile([FromBody] ProfileUpdateDto dto) =>
        Ok(await _timesheets.UpdateProfileAsync(_user.AppUserId, dto));

    [HttpGet("clock")]
    public async Task<ActionResult<ClockStateDto>> ClockState() => Ok(await _clock.GetStateAsync(_user.AppUserId));

    [HttpPost("clock/in")]
    public async Task<ActionResult<ClockStateDto>> ClockIn() => Ok(await _clock.ClockInAsync(_user.AppUserId));

    [HttpPost("clock/out")]
    public async Task<ActionResult<ClockStateDto>> ClockOut() => Ok(await _clock.ClockOutAsync(_user.AppUserId));

    [HttpPost("clock/break-start")]
    public async Task<ActionResult<ClockStateDto>> BreakStart() => Ok(await _clock.BreakStartAsync(_user.AppUserId));

    [HttpPost("clock/break-end")]
    public async Task<ActionResult<ClockStateDto>> BreakEnd() => Ok(await _clock.BreakEndAsync(_user.AppUserId));
}
