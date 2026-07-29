namespace MoleculeByMakeover.Application.Common.Interfaces;

public interface ICurrentLanguageService
{
    int LanguageId { get; }
    string LanguageCode { get; }
}
