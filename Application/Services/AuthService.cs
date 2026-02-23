using Application.DTOs.RequestDTOs.Auth;
using Application.DTOs.ResponseDTOs.Auth;
using Application.Interfaces.IRepositories;
using Application.Interfaces.IServices;
using Application.Interfaces.IUnitOfWork;
using AutoMapper;
using BCrypt.Net;
using Domain.Entities;
using Google.Apis.Auth;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _configuration;

    public AuthService(
        IUnitOfWork unitOfWork, IJwtService jwtService,
        IEmailService emailService, IMemoryCache cache,
        IMapper mapper, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _emailService = emailService;
        _cache = cache;
        _mapper = mapper;
        _configuration = configuration;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _unitOfWork.UserRepository.GetByEmailAsync(request.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new Exception("Email or password is incorrect.");

        if (user.IsActive == false)
            throw new Exception("Your account has been disabled.");

        var otp = new Random().Next(100000, 999999).ToString();
        _cache.Set($"2FA_Login_{user.Email}", otp, TimeSpan.FromMinutes(3));
        await _emailService.SendOtpEmailAsync(user.Email, otp);

        return new AuthResponse
        {
            Email = user.Email,
            Message = "A verification code has been sent to your email."
        };
    }

    public async Task<AuthResponse> Verify2FALoginAsync(Verify2FALoginRequest request)
    {
        if (!_cache.TryGetValue($"2FA_Login_{request.Email}", out string cachedOtp) || cachedOtp != request.Otp)
            throw new Exception("OTP code is incorrect or has expired.");

        var user = await _unitOfWork.UserRepository.GetByEmailAsync(request.Email);
        _cache.Remove($"2FA_Login_{request.Email}");

        return GenerateAuthResponse(user, "Two-factor authentication successful!");
    }

    public async Task RegisterAsync(RegisterRequest request)
    {
        if (await _unitOfWork.UserRepository.GetByEmailAsync(request.Email) != null)
            throw new Exception("This email is already in use.");

        var otp = new Random().Next(100000, 999999).ToString();
        var cacheOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

        _cache.Set($"Registration_{request.Email}", (request, otp), cacheOptions);
        await _emailService.SendOtpEmailAsync(request.Email, otp);
    }

    public async Task<AuthResponse> VerifyOtpAndRegisterAsync(OtpVerificationRequest request)
    {
        if (!_cache.TryGetValue($"Registration_{request.Email}", out (RegisterRequest registration, string otp) cachedData))
            throw new Exception("Registration session has expired or was not found.");

        if (cachedData.otp != request.Otp)
            throw new Exception("OTP code is incorrect.");

        var user = new User
        {
            Email = cachedData.registration.Email,
            FullName = cachedData.registration.FullName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(cachedData.registration.Password),
            PhoneNumber = cachedData.registration.PhoneNumber,
            Role = "Customer",
            IsActive = true,
            IsTwoFactorEnabled = false,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.UserRepository.CreateAsync(user);
        await _unitOfWork.SaveChangesAsync();
        _cache.Remove($"Registration_{request.Email}");

        return GenerateAuthResponse(user, "Registration successful!");
    }

    public async Task<AuthResponse> GoogleLoginAsync(GoogleLoginRequest request)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new List<string> { _configuration["GoogleAuth:ClientId"]! }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
            var user = await _unitOfWork.UserRepository.GetByEmailAsync(payload.Email);

            if (user == null)
            {
                user = new User
                {
                    Email = payload.Email,
                    FullName = payload.Name,
                    AvatarUrl = payload.Picture,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
                    Role = "Customer",
                    IsActive = true,
                    IsTwoFactorEnabled = false,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.UserRepository.CreateAsync(user);
                await _unitOfWork.SaveChangesAsync();
            }
            else if (user.IsActive == false)
            {
                throw new Exception("Your Google account has been disabled by the system.");
            }

            return GenerateAuthResponse(user, "Google login successful!");
        }
        catch (InvalidJwtException)
        {
            throw new Exception("Invalid Google token.");
        }
    }

    public async Task<string> RegisterNormalAsync(RegisterRequest request)
    {
        if (await _unitOfWork.UserRepository.GetByEmailAsync(request.Email) != null)
            throw new Exception("This email is already in use!");

        var newUser = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            Role = "Customer",
            IsActive = true,
            IsTwoFactorEnabled = false,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.UserRepository.CreateAsync(newUser);
        await _unitOfWork.SaveChangesAsync();

        return "Account registered successfully!";
    }

    private AuthResponse GenerateAuthResponse(User user, string message)
    {
        return new AuthResponse
        {
            UserId = user.Id.ToString(),
            Token = _jwtService.GenerateToken(user),
            Email = user.Email,
            FullName = user.FullName ?? "",
            Role = user.Role ?? "Customer",
            AvatarUrl = user.AvatarUrl,
            Message = message
        };
    }
}