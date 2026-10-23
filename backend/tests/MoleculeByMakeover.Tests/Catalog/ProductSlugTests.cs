using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MoleculeByMakeover.Application.Common.Interfaces;
using MoleculeByMakeover.Application.Features.Catalog;
using MoleculeByMakeover.Domain.Entities;
using MoleculeByMakeover.Domain.Enums;
using MoleculeByMakeover.Infrastructure.Persistence;
using MoleculeByMakeover.Infrastructure.Persistence.Repositories;
using NSubstitute;

namespace MoleculeByMakeover.Tests.Catalog;

// The admin product form no longer has a Slug field: slugs are generated from the name on
// create and kept on edit, so existing product URLs keep working.
public class ProductSlugTests
{
    private readonly string _databaseName = Guid.NewGuid().ToString();
    private readonly Guid _categoryId = Guid.NewGuid();

    public ProductSlugTests()
    {
        using var db = NewContext();
        db.Languages.AddRange(
            new Language { Id = 1, Code = "en", Name = "English", IsDefault = true },
            new Language { Id = 2, Code = "hy", Name = "Armenian" });
        db.Categories.Add(new Category { Id = _categoryId });
        db.SaveChanges();
    }

    [Fact]
    public async Task Create_without_slug_generates_one_from_the_name()
    {
        var id = await CreateAsync("SKU-1", ("en", "NEW Rosemary & Mint Serum 5.0!", null), ("hy", "Մորուքի Յուղ", null));

        var slugs = await SlugsAsync(id);
        Assert.Equal("new-rosemary-mint-serum-5-0", slugs["en"]);
        Assert.Equal("մորուքի-յուղ", slugs["hy"]);
    }

    [Fact]
    public async Task Duplicate_names_get_a_numeric_suffix()
    {
        var first = await CreateAsync("SKU-1", ("en", "Beard Oil", null));
        var second = await CreateAsync("SKU-2", ("en", "Beard Oil", null));
        var third = await CreateAsync("SKU-3", ("en", "beard  oil", null));

        Assert.Equal("beard-oil", (await SlugsAsync(first))["en"]);
        Assert.Equal("beard-oil-2", (await SlugsAsync(second))["en"]);
        Assert.Equal("beard-oil-3", (await SlugsAsync(third))["en"]);
    }

    [Fact]
    public async Task A_deleted_product_still_reserves_its_slug()
    {
        var deleted = await CreateAsync("SKU-1", ("en", "Beard Oil", null));
        await using (var db = NewContext())
            await NewService(db).DeleteAsync(deleted);

        var id = await CreateAsync("SKU-2", ("en", "Beard Oil", null));

        Assert.Equal("beard-oil-2", (await SlugsAsync(id))["en"]);
    }

    [Fact]
    public async Task Editing_the_name_keeps_the_existing_slug()
    {
        var id = await CreateAsync("SKU-1", ("en", "Beard Oil", null));

        await using (var db = NewContext())
            await NewService(db).UpdateAsync(id, Request("SKU-1", ("en", "Premium Beard Oil", null)));

        await using var check = NewContext();
        var translation = await check.ProductTranslations.SingleAsync(t => t.ProductId == id);
        Assert.Equal("Premium Beard Oil", translation.Name);
        Assert.Equal("beard-oil", translation.Slug);
    }

    [Fact]
    public async Task A_translation_added_on_edit_gets_a_generated_slug()
    {
        var id = await CreateAsync("SKU-1", ("en", "Beard Oil", null));

        await using (var db = NewContext())
            await NewService(db).UpdateAsync(id, Request("SKU-1", ("en", "Beard Oil", null), ("hy", "Մորուքի յուղ", null)));

        var slugs = await SlugsAsync(id);
        Assert.Equal("beard-oil", slugs["en"]);
        Assert.Equal("մորուքի-յուղ", slugs["hy"]);
    }

    [Fact]
    public async Task An_explicit_slug_from_an_api_client_is_still_used()
    {
        var id = await CreateAsync("SKU-1", ("en", "Beard Oil", "custom-beard-oil"));

        Assert.Equal("custom-beard-oil", (await SlugsAsync(id))["en"]);
    }

    [Fact]
    public void Validation_allows_a_missing_slug_but_rejects_an_invalid_one()
    {
        var validator = new ProductUpsertRequestValidator();

        Assert.True(validator.Validate(Request("SKU-1", ("en", "Beard Oil", null))).IsValid);
        Assert.True(validator.Validate(Request("SKU-1", ("en", "Beard Oil", ""))).IsValid);
        Assert.False(validator.Validate(Request("SKU-1", ("en", "Beard Oil", "bad slug!"))).IsValid);
    }

    private async Task<Guid> CreateAsync(string sku, params (string Language, string Name, string? Slug)[] translations)
    {
        await using var db = NewContext();
        return await NewService(db).CreateAsync(Request(sku, translations));
    }

    private async Task<Dictionary<string, string>> SlugsAsync(Guid productId)
    {
        await using var db = NewContext();
        return await db.ProductTranslations.IgnoreQueryFilters()
            .Where(t => t.ProductId == productId)
            .Join(db.Languages, t => t.LanguageId, l => l.Id, (t, l) => new { l.Code, t.Slug })
            .ToDictionaryAsync(x => x.Code, x => x.Slug);
    }

    private ProductUpsertRequest Request(string sku, params (string Language, string Name, string? Slug)[] translations) =>
        new(sku, _categoryId, 1000m, DiscountType.None, null, 5, null, null, false, false, true, [],
            translations.Select(t => new ProductTranslationInput(t.Language, t.Name, t.Slug, null, null, null, null, null, null, null)).ToList());

    private AppDbContext NewContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(_databaseName).Options);

    private static ProductService NewService(AppDbContext db) =>
        new(new UnitOfWork(db), Substitute.For<ICurrentLanguageService>(), new ProductUpsertRequestValidator(),
            NullLogger<ProductService>.Instance);
}
