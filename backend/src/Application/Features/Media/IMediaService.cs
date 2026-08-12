namespace MoleculeByMakeover.Application.Features.Media;

public interface IMediaService
{
    Task<List<GalleryImageDto>> GetAllAsync(CancellationToken ct = default);
    Task<GalleryImageDto> UploadAsync(Stream content, string fileName, string contentType, Guid? uploadedByUserId, string? altText, string folder, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
