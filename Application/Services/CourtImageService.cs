using Application.DTOs.RequestDTOs.CourtImage;
using Application.DTOs.ResponseDTOs.CourtImage;
using Application.Interfaces.IServices;
using Application.Interfaces.IUnitOfWork;
using AutoMapper;
using Domain.Entities;

namespace Application.Services;

public class CourtImageService : ICourtImageService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CourtImageService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CourtImageResponse> CreateAsync(CourtImageCreateRequest request)
    {
        var court = await _unitOfWork.CourtRepository.GetByIdAsync(request.CourtId);
        if (court == null) throw new Exception("Court not found");

        var courtImage = new CourtImage
        {
            Id = Guid.NewGuid(),
            CourtId = request.CourtId,
            ImageUrl = request.ImageUrl,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.CourtImageRepository.CreateAsync(courtImage);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<CourtImageResponse>(courtImage);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var courtImage = await _unitOfWork.CourtImageRepository.GetByIdAsync(id);
        if (courtImage == null) return false;

        _unitOfWork.CourtImageRepository.Delete(courtImage);
        return await _unitOfWork.SaveChangesAsync() > 0;
    }
}
