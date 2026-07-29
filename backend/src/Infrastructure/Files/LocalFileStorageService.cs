using Microsoft.Extensions.Options;
using MoleculeByMakeover.Application.Common.Interfaces;
using MoleculeByMakeover.Application.Common.Options;

namespace MoleculeByMakeover.Infrastructure.Files;

// Writes to a local (Docker-volume-backed) directory served statically by the API/Nginx.
// Swapping to S3/MinIO/Spaces later is a new IFileStorageService implementation plus a
// DI registration change - callers only ever depend on the interface.
public class LocalFileStorageService(IOptions<FileStorageOptions> options) : IFileStorageService
{
    private readonly FileStorageOptions _options = options.Value;

    public async Task<FileUploadResult> SaveAsync(Stream content, string fileName, string contentType, string folder, CancellationToken ct = default)
    {
        var safeFileName = $"{Guid.NewGuid():N}{Path.GetExtension(fileName)}";
        var directory = Path.Combine(_options.RootPath, folder);
        Directory.CreateDirectory(directory);

        var fullPath = Path.Combine(directory, safeFileName);
        await using (var fileStream = File.Create(fullPath))
        {
            content.Position = 0;
            await content.CopyToAsync(fileStream, ct);
        }

        var url = $"{_options.PublicBaseUrl.TrimEnd('/')}/{folder}/{safeFileName}";
        var size = new FileInfo(fullPath).Length;

        return new FileUploadResult(url, safeFileName, size);
    }

    public Task DeleteAsync(string url, CancellationToken ct = default)
    {
        var relative = url.Replace(_options.PublicBaseUrl.TrimEnd('/'), string.Empty).TrimStart('/');
        var fullPath = Path.Combine(_options.RootPath, relative);
        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }
}
