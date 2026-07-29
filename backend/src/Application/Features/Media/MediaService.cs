using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MoleculeByMakeover.Application.Common.Interfaces;
using MoleculeByMakeover.Domain.Entities;
using MoleculeByMakeover.Domain.Interfaces;
using MoleculeByMakeover.Shared.Exceptions;

namespace MoleculeByMakeover.Application.Features.Media;

public class MediaService(IUnitOfWork unitOfWork, IFileStorageService fileStorageService, IMapper mapper) : IMediaService
{
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp", "image/gif"
    };
    private const long MaxSizeBytes = 10 * 1024 * 1024;

    public async Task<List<GalleryImageDto>> GetAllAsync(CancellationToken ct = default)
    {
        var images = await unitOfWork.GalleryImages.Query().OrderByDescending(i => i.CreatedAt).ToListAsync(ct);
        return mapper.Map<List<GalleryImageDto>>(images);
    }

    public async Task<GalleryImageDto> UploadAsync(Stream content, string fileName, string contentType, Guid? uploadedByUserId, string? altText, CancellationToken ct = default)
    {
        if (!AllowedContentTypes.Contains(contentType))
            throw new BadRequestException("Only JPEG, PNG, WebP, or GIF images are allowed.");
        if (content.Length > MaxSizeBytes)
            throw new BadRequestException("Image exceeds the 10 MB upload limit.");

        var result = await fileStorageService.SaveAsync(content, fileName, contentType, "gallery", ct);

        var image = new GalleryImage
        {
            Url = result.Url,
            FileName = result.FileName,
            SizeBytes = result.SizeBytes,
            MimeType = contentType,
            AltText = altText,
            UploadedByUserId = uploadedByUserId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await unitOfWork.GalleryImages.AddAsync(image, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return mapper.Map<GalleryImageDto>(image);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var image = await unitOfWork.GalleryImages.GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(GalleryImage), id);
        await fileStorageService.DeleteAsync(image.Url, ct);
        unitOfWork.GalleryImages.Remove(image);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
