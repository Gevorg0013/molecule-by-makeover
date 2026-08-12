using MoleculeByMakeover.Domain.Interfaces;

namespace MoleculeByMakeover.Infrastructure.Persistence.Repositories;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    private IUserRepository? _users;
    private IRoleRepository? _roles;
    private IRefreshTokenRepository? _refreshTokens;
    private IPasswordResetTokenRepository? _passwordResetTokens;
    private ILanguageRepository? _languages;
    private ICategoryRepository? _categories;
    private IProductRepository? _products;
    private ITagRepository? _tags;
    private ICartRepository? _carts;
    private ICouponRepository? _coupons;
    private IOrderRepository? _orders;
    private IWishlistRepository? _wishlists;
    private IReviewRepository? _reviews;
    private IBlogPostRepository? _blogPosts;
    private IPageRepository? _pages;
    private IBannerRepository? _banners;
    private IGalleryImageRepository? _galleryImages;
    private ISettingRepository? _settings;
    private INewsletterSubscriberRepository? _newsletterSubscribers;
    private IProcessedPaymentEventRepository? _processedPaymentEvents;

    public IUserRepository Users => _users ??= new UserRepository(context);
    public IRoleRepository Roles => _roles ??= new RoleRepository(context);
    public IRefreshTokenRepository RefreshTokens => _refreshTokens ??= new RefreshTokenRepository(context);
    public IPasswordResetTokenRepository PasswordResetTokens => _passwordResetTokens ??= new PasswordResetTokenRepository(context);
    public ILanguageRepository Languages => _languages ??= new LanguageRepository(context);
    public ICategoryRepository Categories => _categories ??= new CategoryRepository(context);
    public IProductRepository Products => _products ??= new ProductRepository(context);
    public ITagRepository Tags => _tags ??= new TagRepository(context);
    public ICartRepository Carts => _carts ??= new CartRepository(context);
    public ICouponRepository Coupons => _coupons ??= new CouponRepository(context);
    public IOrderRepository Orders => _orders ??= new OrderRepository(context);
    public IWishlistRepository Wishlists => _wishlists ??= new WishlistRepository(context);
    public IReviewRepository Reviews => _reviews ??= new ReviewRepository(context);
    public IBlogPostRepository BlogPosts => _blogPosts ??= new BlogPostRepository(context);
    public IPageRepository Pages => _pages ??= new PageRepository(context);
    public IBannerRepository Banners => _banners ??= new BannerRepository(context);
    public IGalleryImageRepository GalleryImages => _galleryImages ??= new GalleryImageRepository(context);
    public ISettingRepository Settings => _settings ??= new SettingRepository(context);
    public INewsletterSubscriberRepository NewsletterSubscribers => _newsletterSubscribers ??= new NewsletterSubscriberRepository(context);
    public IProcessedPaymentEventRepository ProcessedPaymentEvents => _processedPaymentEvents ??= new ProcessedPaymentEventRepository(context);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);

    public void TrackNew<TEntity>(TEntity entity) where TEntity : class => context.Set<TEntity>().Add(entity);
}
