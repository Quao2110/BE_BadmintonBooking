using Application.DTOs.RequestDTOs.User;
using Application.DTOs.ResponseDTOs.User;
using Application.Interfaces.IServices;
using Application.Interfaces.IUnitOfWork;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Application.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileService _fileService;

    public UserService(IUnitOfWork unitOfWork, IMapper mapper, IFileService fileService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _fileService = fileService;
    }

    public async Task<IEnumerable<UserResponse>> GetAllAsync()
    {
        var users = await _unitOfWork.UserRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<UserResponse>>(users);
    }

    public async Task<UserResponse> GetByIdAsync(Guid id)
    {
        var user = await _unitOfWork.UserRepository.GetByIdAsync(id)
            ?? throw new Exception("User not found.");

        return _mapper.Map<UserResponse>(user);
    }

    public async Task<UserResponse> UpdateAsync(Guid id, UpdateUserRequest request)
    {
        var user = await _unitOfWork.UserRepository.GetByIdAsync(id)
            ?? throw new Exception("User not found.");

        user.FullName = request.FullName;
        user.PhoneNumber = request.PhoneNumber;

        if (request.Avatar != null && request.Avatar.Length > 0)
        {
            user.AvatarUrl = await _fileService.SaveFileAsync(request.Avatar, "avatars");
        }

        _unitOfWork.UserRepository.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<UserResponse>(user);
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = await _unitOfWork.UserRepository.GetByIdAsync(id)
            ?? throw new Exception("User not found.");

        _unitOfWork.UserRepository.DeleteById(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ChangePasswordAsync(Guid id, ChangePasswordRequest request)
    {
        var user = await _unitOfWork.UserRepository.GetByIdAsync(id)
            ?? throw new Exception("User not found.");

        if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash))
            throw new Exception("Old password is incorrect.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

        _unitOfWork.UserRepository.Update(user);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<string> UploadAvatarAsync(Guid id, IFormFile file)
    {
        var user = await _unitOfWork.UserRepository.GetByIdAsync(id)
            ?? throw new Exception("User not found.");

        if (file == null || file.Length == 0)
            throw new Exception("Invalid image file.");

        user.AvatarUrl = await _fileService.SaveFileAsync(file, "avatars");

        _unitOfWork.UserRepository.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return user.AvatarUrl;
    }

    public async Task<UserResponse> CreateAsync(CreateUserRequest request)
    {
        if (await _unitOfWork.UserRepository.GetByEmailAsync(request.Email) != null)
            throw new Exception("This email is already in use.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FullName = request.FullName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            PhoneNumber = request.PhoneNumber,
            Role = request.Role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        if (request.Avatar != null && request.Avatar.Length > 0)
        {
            user.AvatarUrl = await _fileService.SaveFileAsync(request.Avatar, "avatars");
        }

        await _unitOfWork.UserRepository.CreateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<UserResponse>(user);
    }

    // ── helpers ───────────────────────────────────────────────────────────────


}
