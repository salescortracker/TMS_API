using AutoMapper;
using TMS.BusinessLayer.DTOs;
using TMS.BusinessLayer.Services.Interfaces;
using TMS.DataAccessLayer.Repositories.Interfaces;

namespace TMS.BusinessLayer.Services.Implementations;

public class CandidateWeekSummaryService : ICandidateWeekSummaryService
{
    private readonly ICandidateWeekSummaryRepository _repository;
    private readonly IMapper _mapper;

    public CandidateWeekSummaryService(ICandidateWeekSummaryRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CandidateWeekSummaryDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<CandidateWeekSummaryDto>>(entities);
    }

    public async Task<IEnumerable<CandidateWeekSummaryDto>> GetByCandidateIdAsync(int candidateId)
    {
        var entities = await _repository.GetByCandidateIdAsync(candidateId);
        return _mapper.Map<IEnumerable<CandidateWeekSummaryDto>>(entities);
    }
}
