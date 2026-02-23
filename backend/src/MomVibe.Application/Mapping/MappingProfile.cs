namespace MomVibe.Application.Mapping;

using AutoMapper;

using DTOs.Admin;
using DTOs.Items;
using DTOs.Users;
using DTOs.Messages;
using DTOs.Payments;
using DTOs.Feedbacks;
using DTOs.Shipping;
using DTOs.PurchaseRequests;
using Domain.Entities;

/// <summary>
/// AutoMapper profile configuring mappings between domain entities and DTOs:
/// - ApplicationUser -> UserDto, AdminUserDto (includes ItemCount).
/// - Item -> ItemDto (maps CategoryName and nested User), ItemPhoto -> ItemPhotoDto.
/// - Message -> MessageDto (Timestamp, SenderDisplayName, SenderAvatarUrl).
/// - Payment -> PaymentDto (ItemTitle).
/// - Feedback -> FeedbackDto (UserDisplayName, UserAvatarUrl).
/// - Shipment -> ShipmentDto (ItemTitle via Payment.Item).
/// Ensure required navigations are loaded or use ProjectTo to compute counts and nested properties efficiently.
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<ApplicationUser, UserDto>();

        CreateMap<ApplicationUser, AdminUserDto>()
            .ForMember(d => d.ItemCount, opt => opt.MapFrom(s => s.Items.Count));

        CreateMap<Item, ItemDto>()
            .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.Category != null ? s.Category.Name : null))
            .ForMember(d => d.UserDisplayName, opt => opt.MapFrom(s => s.User != null ? s.User.DisplayName : null))
            .ForMember(d => d.UserAvatarUrl, opt => opt.MapFrom(s => s.User != null ? s.User.AvatarUrl : null))
            .ForMember(d => d.User, opt => opt.MapFrom(s => s.User));

        CreateMap<ItemPhoto, ItemPhotoDto>();

        CreateMap<Message, MessageDto>()
            .ForMember(d => d.Timestamp, opt => opt.MapFrom(s => s.CreatedAt))
            .ForMember(d => d.SenderDisplayName, opt => opt.MapFrom(s => s.Sender != null ? s.Sender.DisplayName : null))
            .ForMember(d => d.SenderAvatarUrl, opt => opt.MapFrom(s => s.Sender != null ? s.Sender.AvatarUrl : null));

        CreateMap<Payment, PaymentDto>()
            .ForMember(d => d.ItemTitle, opt => opt.MapFrom(s => s.Item != null ? s.Item.Title : null));

        CreateMap<Feedback, FeedbackDto>()
            .ForMember(d => d.UserDisplayName, opt => opt.MapFrom(s => s.User != null ? s.User.DisplayName : null))
            .ForMember(d => d.UserAvatarUrl, opt => opt.MapFrom(s => s.User != null ? s.User.AvatarUrl : null));

        CreateMap<Shipment, ShipmentDto>()
            .ForMember(d => d.ItemTitle, opt => opt.MapFrom(s => s.Payment != null && s.Payment.Item != null ? s.Payment.Item.Title : null));

        CreateMap<PurchaseRequest, PurchaseRequestDto>()
            .ForMember(d => d.ItemTitle, opt => opt.MapFrom(s => s.Item != null ? s.Item.Title : null))
            .ForMember(d => d.ItemPhotoUrl, opt => opt.MapFrom(s =>
                s.Item != null && s.Item.Photos != null && s.Item.Photos.Count > 0
                    ? s.Item.Photos.OrderBy(p => p.DisplayOrder).First().Url
                    : null))
            .ForMember(d => d.ListingType, opt => opt.MapFrom(s => s.Item != null ? s.Item.ListingType : default))
            .ForMember(d => d.Price, opt => opt.MapFrom(s => s.Item != null ? s.Item.Price : null))
            .ForMember(d => d.BuyerDisplayName, opt => opt.MapFrom(s => s.Buyer != null ? s.Buyer.DisplayName : null))
            .ForMember(d => d.BuyerAvatarUrl, opt => opt.MapFrom(s => s.Buyer != null ? s.Buyer.AvatarUrl : null));
    }
}
