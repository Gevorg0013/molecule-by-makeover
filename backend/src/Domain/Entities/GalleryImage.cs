using MoleculeByMakeover.Domain.Common;

namespace MoleculeByMakeover.Domain.Entities;

public class GalleryImage : BaseEntity
{
    public string Url { get; set; } = default!;
    public string? AltText { get; set; }
    public string FileName { get; set; } = default!;
    public long SizeBytes { get; set; }
    public string MimeType { get; set; } = default!;

    public Guid? UploadedByUserId { get; set; }
    public User? UploadedByUser { get; set; }
}
