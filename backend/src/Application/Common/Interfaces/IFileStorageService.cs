namespace MoleculeByMakeover.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<FileUploadResult> SaveAsync(Stream content, string fileName, string contentType, string folder, CancellationToken ct = default);
    Task DeleteAsync(string url, CancellationToken ct = default);
}

public record FileUploadResult(string Url, string FileName, long SizeBytes);
