using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TMS.BusinessLayer.DTOs.Workflow;
using TMS.BusinessLayer.Services.Workflow;

namespace TMS.API.Controllers.Workflow;

/// <summary>Pages that work without logging in: onboarding form, manager registration, dropdown data.</summary>
[ApiController]
[AllowAnonymous]
[Route("api/public")]
public class PublicController : ControllerBase
{
    private readonly ILookupService _lookups;
    private readonly IOnboardingService _onboarding;

    public PublicController(ILookupService lookups, IOnboardingService onboarding)
    {
        _lookups = lookups;
        _onboarding = onboarding;
    }

    [HttpGet("lookups")]
    public async Task<ActionResult<LookupsDto>> Lookups() => Ok(await _lookups.GetLookupsAsync());

    [HttpGet("seats")]
    public async Task<ActionResult<SeatInfoDto>> Seats() => Ok(await _onboarding.GetSeatsAsync());

    [HttpPost("onboarding/candidate")]
    public async Task<ActionResult<OnboardingResultDto>> Candidate([FromBody] OnboardingSubmitDto dto) =>
        Ok(await _onboarding.SubmitCandidateAsync(dto));

    [HttpPost("onboarding/manager")]
    public async Task<ActionResult<OnboardingResultDto>> Manager([FromBody] ManagerRegistrationDto dto) =>
        Ok(await _onboarding.RegisterManagerAsync(dto));
}
