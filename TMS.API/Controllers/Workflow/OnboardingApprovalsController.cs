using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TMS.BusinessLayer.Common;
using TMS.BusinessLayer.DTOs.Workflow;
using TMS.BusinessLayer.Services.Workflow;

namespace TMS.API.Controllers.Workflow;

[ApiController]
[Authorize(Policy = "perm:approve_onboarding")]
[Route("api/onboarding")]
public class OnboardingApprovalsController : ControllerBase
{
    private readonly IOnboardingService _service;
    private readonly ICurrentUser _user;

    public OnboardingApprovalsController(IOnboardingService service, ICurrentUser user)
    {
        _service = service;
        _user = user;
    }

    [HttpGet]
    public async Task<ActionResult<ApplicantListDto>> List() => Ok(await _service.ListAsync(_user.AppUserId));

    [HttpPost("{id:int}/approve")]
    public async Task<ActionResult<ActivationResultDto>> Approve(int id) =>
        Ok(await _service.ApproveAsync(_user.AppUserId, id));

    [HttpPost("{id:int}/reject")]
    public async Task<ActionResult<ActivationResultDto>> Reject(int id, [FromBody] RejectRequestDto dto) =>
        Ok(await _service.RejectAsync(_user.AppUserId, id, dto.Reason));

    [HttpPost("approve-all")]
    public async Task<ActionResult<BulkActivationResultDto>> ApproveAll() =>
        Ok(await _service.ApproveAllPendingAsync(_user.AppUserId));
}
