using Application.DTOs.RequestDTOs.Court;
using Application.DTOs.ResponseDTOs.Court;
using Application.Interfaces.IServices;
using Application.Interfaces.IUnitOfWork;
using AutoMapper;
using Domain.Entities;

namespace Application.Services;

public class CourtService : ICourtService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CourtService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CourtResponse>> GetAllAsync()
    {
        var courts = await _unitOfWork.CourtRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<CourtResponse>>(courts);
    }

    public async Task<CourtResponse?> GetByIdAsync(Guid id)
    {
        var court = await _unitOfWork.CourtRepository.GetByIdAsync(id);
        return _mapper.Map<CourtResponse>(court);
    }

    public async Task<CourtResponse> CreateAsync(CourtCreateRequest request)
    {
        var court = new Court
        {
            Id = Guid.NewGuid(),
            CourtName = request.CourtName,
            Description = request.Description,
            Status = request.Status
        };

        await _unitOfWork.CourtRepository.CreateAsync(court);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<CourtResponse>(court);
    }

    public async Task<CourtResponse> UpdateAsync(Guid id, CourtUpdateRequest request)
    {
        var court = await _unitOfWork.CourtRepository.GetByIdAsync(id);
        if (court == null) throw new Exception("Court not found");

        court.CourtName = request.CourtName;
        court.Description = request.Description;
        court.Status = request.Status;

        _unitOfWork.CourtRepository.Update(court);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<CourtResponse>(court);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var court = await _unitOfWork.CourtRepository.GetByIdAsync(id);
        if (court == null) return false;

        _unitOfWork.CourtRepository.Delete(court);
        return await _unitOfWork.SaveChangesAsync() > 0;
    }
}
