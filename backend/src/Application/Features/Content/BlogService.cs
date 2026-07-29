using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MoleculeByMakeover.Application.Common.Interfaces;
using MoleculeByMakeover.Domain.Entities;
using MoleculeByMakeover.Domain.Interfaces;
using MoleculeByMakeover.Shared.Common;
using MoleculeByMakeover.Shared.Exceptions;

namespace MoleculeByMakeover.Application.Features.Content;

public class BlogService(
    IUnitOfWork unitOfWork,
    ICurrentLanguageService currentLanguage,
    IValidator<BlogPostUpsertRequest> validator) : IBlogService
{
    public async Task<PaginatedList<BlogPostListItemDto>> GetPublishedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var query = unitOfWork.BlogPosts.Query()
            .Include(b => b.Translations)
            .Where(b => b.IsPublished && !b.IsDeleted)
            .OrderByDescending(b => b.PublishedAt);

        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return PaginatedList<BlogPostListItemDto>.Create(items.Select(ToListItem).ToList(), total, page, pageSize);
    }

    public async Task<BlogPostDetailDto> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        var post = await unitOfWork.BlogPosts.GetBySlugAsync(slug, currentLanguage.LanguageId, ct)
            ?? throw new NotFoundException(nameof(BlogPost), slug);

        var translation = ResolveTranslation(post.Translations);
        return new BlogPostDetailDto(post.Id, translation?.Title ?? string.Empty, translation?.Slug ?? string.Empty,
            translation?.Excerpt, translation?.Content ?? string.Empty, post.CoverImageUrl, translation?.MetaTitle, translation?.MetaDescription, post.PublishedAt);
    }

    public async Task<List<BlogPostAdminDto>> GetAllAdminAsync(CancellationToken ct = default)
    {
        var posts = await unitOfWork.BlogPosts.Query().Include(b => b.Translations).ThenInclude(t => t.Language)
            .Where(b => !b.IsDeleted).OrderByDescending(b => b.CreatedAt).ToListAsync(ct);
        return posts.Select(ToAdminDto).ToList();
    }

    public async Task<BlogPostAdminDto> GetByIdAdminAsync(Guid id, CancellationToken ct = default)
    {
        var post = await unitOfWork.BlogPosts.Query().Include(b => b.Translations).ThenInclude(t => t.Language)
            .FirstOrDefaultAsync(b => b.Id == id, ct) ?? throw new NotFoundException(nameof(BlogPost), id);
        return ToAdminDto(post);
    }

    public async Task<Guid> CreateAsync(Guid authorId, BlogPostUpsertRequest request, CancellationToken ct = default)
    {
        await validator.ValidateAndThrowAsync(request, ct);

        var post = new BlogPost
        {
            AuthorId = authorId,
            CoverImageUrl = request.CoverImageUrl,
            IsPublished = request.IsPublished,
            PublishedAt = request.IsPublished ? DateTimeOffset.UtcNow : null,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await ApplyTranslationsAsync(post, request.Translations, ct);

        await unitOfWork.BlogPosts.AddAsync(post, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return post.Id;
    }

    public async Task UpdateAsync(Guid id, BlogPostUpsertRequest request, CancellationToken ct = default)
    {
        await validator.ValidateAndThrowAsync(request, ct);

        var post = await unitOfWork.BlogPosts.Query().Include(b => b.Translations)
            .FirstOrDefaultAsync(b => b.Id == id, ct) ?? throw new NotFoundException(nameof(BlogPost), id);

        post.CoverImageUrl = request.CoverImageUrl;
        if (request.IsPublished && !post.IsPublished) post.PublishedAt = DateTimeOffset.UtcNow;
        post.IsPublished = request.IsPublished;
        post.UpdatedAt = DateTimeOffset.UtcNow;

        post.Translations.Clear();
        await ApplyTranslationsAsync(post, request.Translations, ct);

        unitOfWork.BlogPosts.Update(post);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var post = await unitOfWork.BlogPosts.GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(BlogPost), id);
        post.IsDeleted = true;
        post.DeletedAt = DateTimeOffset.UtcNow;
        unitOfWork.BlogPosts.Update(post);
        await unitOfWork.SaveChangesAsync(ct);
    }

    private async Task ApplyTranslationsAsync(BlogPost post, List<BlogTranslationInput> translations, CancellationToken ct)
    {
        foreach (var t in translations)
        {
            var language = await unitOfWork.Languages.GetByCodeAsync(t.LanguageCode, ct)
                ?? throw new BadRequestException($"Unknown language code '{t.LanguageCode}'.");

            var translation = new BlogPostTranslation
            {
                LanguageId = language.Id,
                Title = t.Title,
                Slug = t.Slug,
                Excerpt = t.Excerpt,
                Content = t.Content,
                MetaTitle = t.MetaTitle,
                MetaDescription = t.MetaDescription,
                CreatedAt = DateTimeOffset.UtcNow
            };
            post.Translations.Add(translation);
            unitOfWork.TrackNew(translation);
        }
    }

    private BlogPostListItemDto ToListItem(BlogPost post)
    {
        var translation = ResolveTranslation(post.Translations);
        return new BlogPostListItemDto(post.Id, translation?.Title ?? string.Empty, translation?.Slug ?? string.Empty,
            translation?.Excerpt, post.CoverImageUrl, post.PublishedAt);
    }

    private static BlogPostAdminDto ToAdminDto(BlogPost post) => new(
        post.Id, post.CoverImageUrl, post.IsPublished, post.PublishedAt,
        post.Translations.Select(t => new BlogTranslationDto(t.Id, t.Language.Code, t.Title, t.Slug, t.Excerpt, t.Content, t.MetaTitle, t.MetaDescription)).ToList());

    private BlogPostTranslation? ResolveTranslation(IEnumerable<BlogPostTranslation> translations) =>
        translations.FirstOrDefault(t => t.LanguageId == currentLanguage.LanguageId) ?? translations.FirstOrDefault();
}
