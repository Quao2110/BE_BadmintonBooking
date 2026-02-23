using Microsoft.AspNetCore.Http;

namespace Application.Interfaces.IServices;

public interface IFileService
{
    /// <summary>
    /// Lưu file vào thư mục chỉ định trong wwwroot.
    /// </summary>
    /// <param name="file">File từ request.</param>
    /// <param name="folderName">Tên thư mục con (ví dụ: avatars, courts, products).</param>
    /// <returns>Đường dẫn tương đối của file sau khi lưu.</returns>
    Task<string> SaveFileAsync(IFormFile file, string folderName);

    /// <summary>
    /// Xóa file từ wwwroot.
    /// </summary>
    /// <param name="relativeUrl">Đường dẫn tương đối của file.</param>
    void DeleteFile(string relativeUrl);
}
