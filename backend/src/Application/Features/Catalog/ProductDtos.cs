using MoleculeByMakeover.Domain.Enums;

namespace MoleculeByMakeover.Application.Features.Catalog;

public record ProductImageDto(Guid Id, string Url, string? AltText, int SortOrder, bool IsPrimary);

public record ProductListItemDto(
    Guid Id,
    string Sku,
    string Name,
    string Slug,
    string? ShortDescription,
    decimal Price,
    decimal FinalPrice,
    DiscountType DiscountType,
    decimal? DiscountValue,
    string? MainImageUrl,
    int Stock,
    string? Brand,
    bool IsFeatured,
    bool IsBestSeller,
    Guid CategoryId,
    string CategoryName,
    List<string> Tags,
    double AverageRating,
    int ReviewCount);

public record ProductDetailDto(
    Guid Id,
    string Sku,
    string Name,
    string Slug,
    string? ShortDescription,
    string? Description,
    string? Ingredients,
    string? UsageInstructions,
    string? Benefits,
    decimal Price,
    decimal FinalPrice,
    DiscountType DiscountType,
    decimal? DiscountValue,
    string? MainImageUrl,
    int Stock,
    string? Brand,
    Guid CategoryId,
    string CategoryName,
    List<string> Tags,
    List<ProductImageDto> Images,
    string? MetaTitle,
    string? MetaDescription,
    double AverageRating,
    int ReviewCount);

public record ProductTranslationInput(
    string LanguageCode,
    string Name,
    string Slug,
    string? ShortDescription,
    string? Description,
    string? Ingredients,
    string? UsageInstructions,
    string? Benefits,
    string? MetaTitle,
    string? MetaDescription);

public record ProductTranslationDto(
    Guid Id,
    string LanguageCode,
    string Name,
    string Slug,
    string? ShortDescription,
    string? Description,
    string? Ingredients,
    string? UsageInstructions,
    string? Benefits,
    string? MetaTitle,
    string? MetaDescription);

public record ProductAdminDto(
    Guid Id,
    string Sku,
    Guid CategoryId,
    decimal Price,
    DiscountType DiscountType,
    decimal? DiscountValue,
    int Stock,
    string? MainImageUrl,
    string? Brand,
    bool IsFeatured,
    bool IsBestSeller,
    bool IsActive,
    List<string> Tags,
    List<ProductImageDto> Images,
    List<ProductTranslationDto> Translations);

public record ProductUpsertRequest(
    string Sku,
    Guid CategoryId,
    decimal Price,
    DiscountType DiscountType,
    decimal? DiscountValue,
    int Stock,
    string? MainImageUrl,
    string? Brand,
    bool IsFeatured,
    bool IsBestSeller,
    bool IsActive,
    List<string> Tags,
    List<ProductTranslationInput> Translations);

public enum ProductSort
{
    Newest,
    PriceAsc,
    PriceDesc,
    BestSelling
}

public class ProductQueryParameters
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? CategorySlug { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Brand { get; set; }
    public string? Tag { get; set; }
    public string? Search { get; set; }
    public bool? FeaturedOnly { get; set; }
    public bool? BestSellerOnly { get; set; }
    public ProductSort Sort { get; set; } = ProductSort.Newest;
}
