namespace MoleculeByMakeover.Application.Features.Media;

public record GalleryImageDto(Guid Id, string Url, string? AltText, string FileName, long SizeBytes, string MimeType, DateTimeOffset CreatedAt);
