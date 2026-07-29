using MoleculeByMakeover.Domain.Common;

namespace MoleculeByMakeover.Domain.Entities;

public class BlogPost : BaseEntity, ISoftDelete
{
    public Guid? AuthorId { get; set; }
    public User? Author { get; set; }

    public string? CoverImageUrl { get; set; }
    public bool IsPublished { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public ICollection<BlogPostTranslation> Translations { get; set; } = [];
}

public class BlogPostTranslation : BaseEntity
{
    public Guid BlogPostId { get; set; }
    public BlogPost BlogPost { get; set; } = default!;

    public int LanguageId { get; set; }
    public Language Language { get; set; } = default!;

    public string Title { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string? Excerpt { get; set; }
    public string Content { get; set; } = default!;
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
}
