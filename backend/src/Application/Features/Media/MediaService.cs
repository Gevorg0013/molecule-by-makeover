using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MoleculeByMakeover.Application.Common.Interfaces;
using MoleculeByMakeover.Domain.Entities;
using MoleculeByMakeover.Domain.Interfaces;
using MoleculeByMakeover.Shared.Exceptions;

namespace MoleculeByMakeover.Application.Features.Media;

public class MediaService(IUnitOfWork unitOfWork, IFileStorageService fileStorageService, IMapper mapper) : IMediaService
{
    private const long MaxSizeBytes = 10 * 1024 * 1024;

    // The value is the canonical extension the file is stored under; the client-supplied name is
    // never trusted for it, so a ".aspx" masquerading as an image can't land in wwwroot.
    private static readonly Dictionary<string, string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["image/jpeg"] = ".jpg",
        ["image/png"] = ".png",
        ["image/webp"] = ".webp",
        ["image/gif"] = ".gif"
    };

    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    public async Task<List<GalleryImageDto>> GetAllAsync(CancellationToken ct = default)
    {
        var images = await unitOfWork.GalleryImages.Query().OrderByDescending(i => i.CreatedAt).ToListAsync(ct);
        return mapper.Map<List<GalleryImageDto>>(images);
    }

    public async Task<GalleryImageDto> UploadAsync(Stream content, string fileName, string contentType, Guid? uploadedByUserId, string? altText, string folder, CancellationToken ct = default)
    {
        if (!AllowedContentTypes.TryGetValue(contentType ?? string.Empty, out var extension))
            throw new BadRequestException("Only JPG, PNG, WebP, or GIF images are allowed.");
        if (content.Length == 0)
            throw new BadRequestException("The uploaded file is empty.");
        if (content.Length > MaxSizeBytes)
            throw new BadRequestException("Image exceeds the 10 MB upload limit.");
        if (!await MatchesImageSignatureAsync(content, contentType!, ct))
            throw new BadRequestException("The uploaded file is not a valid image.");

        var storedName = Path.GetFileName(fileName) is { Length: > 0 } original
            ? Path.ChangeExtension(original, extension)
            : $"image{extension}";

        var result = await fileStorageService.SaveAsync(content, storedName, contentType!, folder, ct);

        var image = new GalleryImage
        {
            Url = result.Url,
            FileName = result.FileName,
            SizeBytes = result.SizeBytes,
            MimeType = contentType!,
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

    // A declared content type is just a client-supplied header, so confirm the bytes agree with it.
    private static async Task<bool> MatchesImageSignatureAsync(Stream content, string contentType, CancellationToken ct)
    {
        var header = new byte[12];
        content.Position = 0;
        var read = await content.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false, ct);
        content.Position = 0;

        if (read < header.Length)
            return false;

        return contentType.ToLowerInvariant() switch
        {
            "image/jpeg" => header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
            "image/png" => header.AsSpan(0, PngSignature.Length).SequenceEqual(PngSignature),
            "image/gif" => header.AsSpan(0, 4).SequenceEqual("GIF8"u8),
            "image/webp" => header.AsSpan(0, 4).SequenceEqual("RIFF"u8) && header.AsSpan(8, 4).SequenceEqual("WEBP"u8),
            _ => false
        };
    }
}
