using Application.Interfaces.IServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Application.Services;

public class FileService : IFileService
{
    private readonly IWebHostEnvironment _env;

    public FileService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> SaveFileAsync(IFormFile file, string folderName)
    {
        if (file == null || file.Length == 0)
            throw new Exception("File không hợp lệ.");

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(ext))
            throw new Exception("Chỉ chấp nhận các định dạng ảnh: jpg, jpeg, png, gif, webp.");

        var uploadFolder = Path.Combine(_env.WebRootPath ?? "wwwroot", folderName);
        if (!Directory.Exists(uploadFolder))
        {
            Directory.CreateDirectory(uploadFolder);
        }

        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/{folderName}/{fileName}";
    }

    public void DeleteFile(string relativeUrl)
    {
        if (string.IsNullOrEmpty(relativeUrl)) return;

        // Xóa dấu / ở đầu nếu có
        var path = relativeUrl.TrimStart('/');
        var filePath = Path.Combine(_env.WebRootPath ?? "wwwroot", path);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}
