using AutoMapper;
using MoleculeByMakeover.Application.Features.Admin;
using MoleculeByMakeover.Application.Features.Media;
using MoleculeByMakeover.Application.Features.Ordering;
using MoleculeByMakeover.Domain.Entities;

namespace MoleculeByMakeover.Application.Common.Mappings;

// AutoMapper handles the straightforward, non-translated entity -> DTO mappings.
// Translated entities (Product, Category, BlogPost, Page, Banner) are mapped by hand in their
// services instead, since picking the right translation for the current language isn't a
// static 1:1 member map - see CategoryService/ProductService for that resolution logic.
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Coupon, CouponDto>();
        CreateMap<Language, LanguageDto>();
        CreateMap<Setting, SettingDto>();
        CreateMap<GalleryImage, GalleryImageDto>();
    }
}
