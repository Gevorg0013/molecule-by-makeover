using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MoleculeByMakeover.Application.Features.Admin;
using MoleculeByMakeover.Application.Features.Catalog;
using MoleculeByMakeover.Application.Features.Content;
using MoleculeByMakeover.Application.Features.Identity;
using MoleculeByMakeover.Application.Features.Marketing;
using MoleculeByMakeover.Application.Features.Media;
using MoleculeByMakeover.Application.Features.Ordering;
using MoleculeByMakeover.Application.Features.Reviews;
using MoleculeByMakeover.Application.Features.Wishlists;

namespace MoleculeByMakeover.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => { }, typeof(DependencyInjection).Assembly);
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<IAuthService, AuthService>();

        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductService, ProductService>();

        services.AddScoped<ICartService, CartService>();
        services.AddScoped<ICheckoutService, CheckoutService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ICouponService, CouponService>();

        services.AddScoped<IWishlistService, WishlistService>();
        services.AddScoped<IReviewService, ReviewService>();

        services.AddScoped<IBlogService, BlogService>();
        services.AddScoped<IPageService, PageService>();

        services.AddScoped<IBannerService, BannerService>();
        services.AddScoped<INewsletterService, NewsletterService>();

        services.AddScoped<IMediaService, MediaService>();

        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<ILanguageService, LanguageService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<ICustomerService, CustomerService>();

        return services;
    }
}
