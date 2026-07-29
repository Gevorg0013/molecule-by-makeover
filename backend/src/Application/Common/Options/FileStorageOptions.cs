namespace MoleculeByMakeover.Application.Common.Options;

public class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    /// <summary>Absolute or container-relative path where uploaded files are written.</summary>
    public string RootPath { get; set; } = "wwwroot/uploads";

    /// <summary>Public base URL clients use to fetch uploaded files (e.g. behind Nginx/CDN).</summary>
    public string PublicBaseUrl { get; set; } = "/uploads";
}
