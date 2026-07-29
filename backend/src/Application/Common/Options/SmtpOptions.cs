namespace MoleculeByMakeover.Application.Common.Options;

public class SmtpOptions
{
    public const string SectionName = "Smtp";

    public string Host { get; set; } = default!;
    public int Port { get; set; } = 587;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool EnableSsl { get; set; } = true;
    public string FromAddress { get; set; } = "no-reply@moleculebymakeover.com";
    public string FromName { get; set; } = "Molecule by Makeover";
}
