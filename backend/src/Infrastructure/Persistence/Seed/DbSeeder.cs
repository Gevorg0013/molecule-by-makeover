using Microsoft.EntityFrameworkCore;
using MoleculeByMakeover.Application.Common.Interfaces;
using MoleculeByMakeover.Domain.Entities;
using MoleculeByMakeover.Domain.Enums;
using MoleculeByMakeover.Shared.Constants;

namespace MoleculeByMakeover.Infrastructure.Persistence.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context, IPasswordHasherService passwordHasher)
    {
        await context.Database.MigrateAsync();

        var languages = await SeedLanguagesAsync(context);
        await SeedRolesAsync(context);
        await context.SaveChangesAsync();

        await SeedAdminUserAsync(context, passwordHasher);
        var categories = await SeedCategoriesAsync(context, languages);
        await SeedProductsAsync(context, languages, categories);
        await SeedCouponAsync(context);
        await SeedPagesAsync(context, languages);
        await SeedSettingsAsync(context);

        await context.SaveChangesAsync();
    }

    private static async Task<Dictionary<string, Language>> SeedLanguagesAsync(AppDbContext context)
    {
        if (!await context.Languages.AnyAsync())
        {
            context.Languages.AddRange(
                new Language { Code = LanguageCodes.English, Name = "English", IsDefault = true, IsActive = true },
                new Language { Code = LanguageCodes.Armenian, Name = "Հայերեն", IsDefault = false, IsActive = true },
                new Language { Code = LanguageCodes.Russian, Name = "Русский", IsDefault = false, IsActive = true });
            await context.SaveChangesAsync();
        }

        return await context.Languages.ToDictionaryAsync(l => l.Code);
    }

    private static async Task SeedRolesAsync(AppDbContext context)
    {
        if (!await context.Roles.AnyAsync())
        {
            context.Roles.AddRange(
                new Role { Name = RoleNames.Admin },
                new Role { Name = RoleNames.Customer });
        }
    }

    private static async Task SeedAdminUserAsync(AppDbContext context, IPasswordHasherService passwordHasher)
    {
        const string adminEmail = "admin@moleculebymakeover.com";
        if (await context.Users.AnyAsync(u => u.Email == adminEmail)) return;

        var adminRole = await context.Roles.FirstAsync(r => r.Name == RoleNames.Admin);

        var admin = new User
        {
            Email = adminEmail,
            PasswordHash = passwordHasher.Hash("Admin@12345"),
            FirstName = "Molecule",
            LastName = "Admin",
            IsEmailConfirmed = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
        admin.UserRoles.Add(new UserRole { User = admin, Role = adminRole });

        context.Users.Add(admin);
    }

    private static async Task<Dictionary<string, Category>> SeedCategoriesAsync(AppDbContext context, Dictionary<string, Language> languages)
    {
        if (await context.Categories.AnyAsync())
            return await context.Categories.Include(c => c.Translations)
                .ToDictionaryAsync(c => c.Translations.First(t => t.LanguageId == languages[LanguageCodes.English].Id).Slug);

        var hairCare = new Category { SortOrder = 1, IsActive = true, CreatedAt = DateTimeOffset.UtcNow };
        hairCare.Translations.Add(Translate(languages, LanguageCodes.English, l => new CategoryTranslation { LanguageId = l.Id, Name = "Hair Care", Slug = "hair-care", Description = "Shampoos, conditioners, and treatments for healthy hair." }));
        hairCare.Translations.Add(Translate(languages, LanguageCodes.Armenian, l => new CategoryTranslation { LanguageId = l.Id, Name = "Մազերի խնամք", Slug = "mazeri-khnamq", Description = "Շամպուններ և խնամքի միջոցներ առողջ մազերի համար։" }));
        hairCare.Translations.Add(Translate(languages, LanguageCodes.Russian, l => new CategoryTranslation { LanguageId = l.Id, Name = "Уход за волосами", Slug = "uhod-za-volosami", Description = "Шампуни и средства для здоровых волос." }));

        var beardCare = new Category { SortOrder = 2, IsActive = true, CreatedAt = DateTimeOffset.UtcNow };
        beardCare.Translations.Add(Translate(languages, LanguageCodes.English, l => new CategoryTranslation { LanguageId = l.Id, Name = "Beard Care", Slug = "beard-care", Description = "Oils, balms, and grooming tools for a premium beard." }));
        beardCare.Translations.Add(Translate(languages, LanguageCodes.Armenian, l => new CategoryTranslation { LanguageId = l.Id, Name = "Մորուքի խնամք", Slug = "morowqi-khnamq", Description = "Յուղեր և բալզամներ մորուքի խնամքի համար։" }));
        beardCare.Translations.Add(Translate(languages, LanguageCodes.Russian, l => new CategoryTranslation { LanguageId = l.Id, Name = "Уход за бородой", Slug = "uhod-za-borodoy", Description = "Масла и бальзамы для ухода за бородой." }));

        context.Categories.AddRange(hairCare, beardCare);
        await context.SaveChangesAsync();

        return new Dictionary<string, Category> { ["hair-care"] = hairCare, ["beard-care"] = beardCare };
    }

    private static async Task SeedProductsAsync(AppDbContext context, Dictionary<string, Language> languages, Dictionary<string, Category> categories)
    {
        if (await context.Products.AnyAsync()) return;

        var vegan = new Tag { Name = "vegan" };
        var crueltyFree = new Tag { Name = "cruelty-free" };
        context.Tags.AddRange(vegan, crueltyFree);

        var shampoo = new Product
        {
            Sku = "MBM-SHM-001",
            CategoryId = categories["hair-care"].Id,
            Price = 12500m,
            DiscountType = DiscountType.Percent,
            DiscountValue = 10,
            Stock = 120,
            Brand = "Molecule by Makeover",
            IsFeatured = true,
            IsBestSeller = true,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
        shampoo.Translations.Add(Translate(languages, LanguageCodes.English, l => new ProductTranslation
        {
            LanguageId = l.Id, Name = "Volumizing Shampoo", Slug = "volumizing-shampoo",
            ShortDescription = "Lightweight shampoo that adds body and shine.",
            Description = "A sulfate-free formula that cleanses without stripping natural oils, leaving hair fuller and shinier.",
            Ingredients = "Aqua, Sodium Cocoyl Isethionate, Panthenol, Biotin, Argan Oil.",
            UsageInstructions = "Apply to wet hair, massage into a lather, rinse thoroughly. Use 3-4 times a week.",
            Benefits = "Adds volume; strengthens strands; sulfate-free."
        }));
        shampoo.Translations.Add(Translate(languages, LanguageCodes.Armenian, l => new ProductTranslation
        {
            LanguageId = l.Id, Name = "Ծավալ տվող շամպուն", Slug = "cavaltvox-shampoun",
            ShortDescription = "Թեթև շամպուն, որը հավելյալ ծավալ և փայլ է հաղորդում։",
            Description = "Սուլֆատ չպարունակող բաղադրություն, որը մաքրում է՝ առանց հեռացնելու բնական յուղերը։",
            Ingredients = "Ջուր, Սոդիում Կոկոիլ Իսետիոնատ, Պանթենոլ, Բիոտին, Արգանի յուղ։",
            UsageInstructions = "Կիրառել թաց մազերի վրա, մերսել, ողողել։ Օգտագործել շաբաթը 3-4 անգամ։",
            Benefits = "Ավելացնում է ծավալ, ամրացնում է մազերը, առանց սուլֆատի։"
        }));
        shampoo.Translations.Add(Translate(languages, LanguageCodes.Russian, l => new ProductTranslation
        {
            LanguageId = l.Id, Name = "Шампунь для объёма", Slug = "shampun-dlya-obyoma",
            ShortDescription = "Лёгкий шампунь, придающий объём и блеск.",
            Description = "Формула без сульфатов бережно очищает, не лишая волосы natural масел, делая их гуще и блестящее.",
            Ingredients = "Вода, Кокоил изетионат натрия, Пантенол, Биотин, Аргановое масло.",
            UsageInstructions = "Нанести на влажные волосы, вспенить, тщательно смыть. Использовать 3-4 раза в неделю.",
            Benefits = "Добавляет объём; укрепляет волосы; без сульфатов."
        }));
        shampoo.ProductTags.Add(new ProductTag { Tag = vegan });
        shampoo.ProductTags.Add(new ProductTag { Tag = crueltyFree });

        var beardOil = new Product
        {
            Sku = "MBM-BRD-001",
            CategoryId = categories["beard-care"].Id,
            Price = 9800m,
            Stock = 80,
            Brand = "Molecule by Makeover",
            IsFeatured = true,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
        beardOil.Translations.Add(Translate(languages, LanguageCodes.English, l => new ProductTranslation
        {
            LanguageId = l.Id, Name = "Beard Oil", Slug = "beard-oil",
            ShortDescription = "Nourishing oil blend for a softer, healthier beard.",
            Description = "A fast-absorbing blend of argan and jojoba oils that conditions skin and beard hair alike.",
            Ingredients = "Argan Oil, Jojoba Oil, Vitamin E, Sandalwood Fragrance.",
            UsageInstructions = "Apply 3-4 drops to palms, work through beard and skin daily.",
            Benefits = "Softens beard hair; reduces itchiness; adds natural shine."
        }));
        beardOil.Translations.Add(Translate(languages, LanguageCodes.Armenian, l => new ProductTranslation
        {
            LanguageId = l.Id, Name = "Մորուքի յուղ", Slug = "morowqi-yugh",
            ShortDescription = "Սնուցող յուղերի խառնուրդ փափուկ և առողջ մորուքի համար։",
            Description = "Արգանի և ժոժոբայի յուղերի արագ ներծծվող խառնուրդ՝ մաշկի և մորուքի համար։",
            Ingredients = "Արգանի յուղ, Ժոժոբայի յուղ, Վիտամին E, Սանդալի բույր։",
            UsageInstructions = "Կիրառել 3-4 կաթիլ ափերին և տարածել մորուքի ու մաշկի վրա ամեն օր։",
            Benefits = "Փափկացնում է մորուքը, նվազեցնում է քորը, հաղորդում է բնական փայլ։"
        }));
        beardOil.Translations.Add(Translate(languages, LanguageCodes.Russian, l => new ProductTranslation
        {
            LanguageId = l.Id, Name = "Масло для бороды", Slug = "maslo-dlya-borody",
            ShortDescription = "Питательная смесь масел для мягкой и здоровой бороды.",
            Description = "Быстро впитывающаяся смесь масел арганы и жожоба, ухаживающая за кожей и волосами бороды.",
            Ingredients = "Масло арганы, масло жожоба, витамин E, аромат сандала.",
            UsageInstructions = "Нанести 3-4 капли на ладони и распределить по бороде и коже ежедневно.",
            Benefits = "Смягчает бороду; уменьшает зуд; придаёт естественный блеск."
        }));
        beardOil.ProductTags.Add(new ProductTag { Tag = vegan });

        context.Products.AddRange(shampoo, beardOil);
    }

    private static async Task SeedCouponAsync(AppDbContext context)
    {
        if (await context.Coupons.AnyAsync()) return;

        context.Coupons.Add(new Coupon
        {
            Code = "WELCOME10",
            DiscountType = DiscountType.Percent,
            DiscountValue = 10,
            MinOrderAmount = 10000m,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        });
    }

    private static async Task SeedPagesAsync(AppDbContext context, Dictionary<string, Language> languages)
    {
        if (await context.Pages.AnyAsync()) return;

        (string Key, string EnTitle, string EnContent)[] pages =
        [
            ("about", "About Us", "Molecule by Makeover crafts premium hair and beard care rooted in science and nature."),
            ("privacy-policy", "Privacy Policy", "We respect your privacy and only collect the data required to fulfil your orders."),
            ("terms", "Terms & Conditions", "By using this site you agree to our terms of sale and use."),
            ("faq", "Frequently Asked Questions", "Find answers to common questions about shipping, returns, and our products.")
        ];

        foreach (var p in pages)
        {
            var page = new Page { Key = p.Key, IsPublished = true, CreatedAt = DateTimeOffset.UtcNow };
            page.Translations.Add(Translate(languages, LanguageCodes.English, l => new PageTranslation { LanguageId = l.Id, Title = p.EnTitle, Slug = p.Key, Content = p.EnContent }));
            page.Translations.Add(Translate(languages, LanguageCodes.Armenian, l => new PageTranslation { LanguageId = l.Id, Title = p.EnTitle, Slug = p.Key, Content = p.EnContent }));
            page.Translations.Add(Translate(languages, LanguageCodes.Russian, l => new PageTranslation { LanguageId = l.Id, Title = p.EnTitle, Slug = p.Key, Content = p.EnContent }));
            context.Pages.Add(page);
        }
    }

    private static async Task SeedSettingsAsync(AppDbContext context)
    {
        if (await context.Settings.AnyAsync()) return;

        context.Settings.AddRange(
            new Setting { Key = "site_name", Value = "Molecule by Makeover", Group = "general" },
            new Setting { Key = "contact_email", Value = "hello@moleculebymakeover.com", Group = "general" },
            new Setting { Key = "instagram_url", Value = "https://instagram.com/moleculebymakeover", Group = "social" });
    }

    private static T Translate<T>(Dictionary<string, Language> languages, string code, Func<Language, T> factory) =>
        factory(languages[code]);
}
