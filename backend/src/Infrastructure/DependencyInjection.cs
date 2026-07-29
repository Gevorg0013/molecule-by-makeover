using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MoleculeByMakeover.Application.Common.Interfaces;
using MoleculeByMakeover.Application.Common.Options;
using MoleculeByMakeover.Domain.Interfaces;
using MoleculeByMakeover.Infrastructure.BackgroundJobs;
using MoleculeByMakeover.Infrastructure.Common;
using MoleculeByMakeover.Infrastructure.Email;
using MoleculeByMakeover.Infrastructure.Files;
using MoleculeByMakeover.Infrastructure.Identity;
using MoleculeByMakeover.Infrastructure.Payments;
using MoleculeByMakeover.Infrastructure.Persistence;
using MoleculeByMakeover.Infrastructure.Persistence.Repositories;

namespace MoleculeByMakeover.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<FileStorageOptions>(configuration.GetSection(FileStorageOptions.SectionName));
        services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.SectionName));
        services.Configure<StripeOptions>(configuration.GetSection(StripeOptions.SectionName));

        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<IPasswordHasherService, PasswordHasherService>();

        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();

        // Payment providers: register every IPaymentProvider adapter here. Adding Idram/ArCa/Telcell
        // later is one more AddScoped<IPaymentProvider, ...> line - nothing else in the app changes.
        services.AddScoped<IPaymentProvider, StripePaymentProvider>();
        services.AddScoped<IPaymentProviderResolver, PaymentProviderResolver>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ICurrentLanguageService, CurrentLanguageService>();
        services.AddSingleton<LanguageCache>();

        services.AddHostedService<StockReservationCleanupService>();

        return services;
    }
}
