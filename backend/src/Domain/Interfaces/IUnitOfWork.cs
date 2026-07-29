namespace MoleculeByMakeover.Domain.Interfaces;

public interface IUnitOfWork
{
    IUserRepository Users { get; }
    IRoleRepository Roles { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    ILanguageRepository Languages { get; }

    ICategoryRepository Categories { get; }
    IProductRepository Products { get; }
    ITagRepository Tags { get; }

    ICartRepository Carts { get; }
    ICouponRepository Coupons { get; }
    IOrderRepository Orders { get; }
    IWishlistRepository Wishlists { get; }
    IReviewRepository Reviews { get; }

    IBlogPostRepository BlogPosts { get; }
    IPageRepository Pages { get; }
    IBannerRepository Banners { get; }
    IGalleryImageRepository GalleryImages { get; }
    ISettingRepository Settings { get; }
    INewsletterSubscriberRepository NewsletterSubscribers { get; }
    IProcessedPaymentEventRepository ProcessedPaymentEvents { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);

    /// <summary>
    /// Explicitly tracks a newly-constructed entity as Added. Required whenever a new child is
    /// attached only via a parent's collection navigation (e.g. `parent.Children.Add(new Child())`)
    /// and the parent may already be tracked/persisted in this DbContext: because BaseEntity.Id is
    /// assigned a non-default Guid at construction time, EF Core's automatic fixup can otherwise
    /// mistake the new row for an existing one and issue an UPDATE that matches nothing, throwing
    /// DbUpdateConcurrencyException. Safe to call regardless of whether the parent itself is new.
    /// </summary>
    void TrackNew<TEntity>(TEntity entity) where TEntity : class;
}
