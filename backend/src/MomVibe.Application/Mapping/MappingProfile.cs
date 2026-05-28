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
using DTOs.DoctorReviews;
using DTOs.ChildFriendlyPlaces;
using DTOs.UserRatings;
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
    /// <summary>
    /// Initializes a new instance of <see cref="MappingProfile"/> and registers all entity-to-DTO mappings.
    /// </summary>
    public MappingProfile()
    {
        CreateMap<ApplicationUser, UserDto>()
            .ForMember(d => d.Roles, opt => opt.Ignore())
            .ForMember(d => d.AverageRating, opt => opt.Ignore())
            .ForMember(d => d.RatingCount, opt => opt.Ignore());

        CreateMap<ApplicationUser, AdminUserDto>()
            .ForMember(d => d.ItemCount, opt => opt.MapFrom(s => s.Items.Count))
            .ForMember(d => d.Roles, opt => opt.Ignore());

        CreateMap<Item, ItemDto>()
            .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.Category != null ? s.Category.Name : null))
            .ForMember(d => d.UserDisplayName, opt => opt.MapFrom(s => s.User != null ? s.User.DisplayName : null))
            .ForMember(d => d.UserAvatarUrl, opt => opt.MapFrom(s => s.User != null ? s.User.AvatarUrl : null))
            .ForMember(d => d.UserIsOnHoliday, opt => opt.MapFrom(s => s.User != null && s.User.IsOnHoliday))
            .ForMember(d => d.User, opt => opt.MapFrom(s => s.User))
            .ForMember(d => d.IsLikedByCurrentUser, opt => opt.Ignore());


        CreateMap<ItemPhoto, ItemPhotoDto>();

        CreateMap<Message, MessageDto>()
            .ForMember(d => d.Timestamp, opt => opt.MapFrom(s => s.CreatedAt))
            .ForMember(d => d.SenderDisplayName, opt => opt.MapFrom(s => s.Sender != null ? s.Sender.DisplayName : null))
            .ForMember(d => d.SenderAvatarUrl, opt => opt.MapFrom(s => s.Sender != null ? s.Sender.AvatarUrl : null));

        CreateMap<Payment, PaymentDto>()
            .ForMember(d => d.ItemTitle, opt => opt.MapFrom(s => s.Item != null ? s.Item.Title : null));

        CreateMap<Payment, EBillDto>()
            .ForMember(d => d.ItemTitle, opt => opt.MapFrom(s => s.Item != null ? s.Item.Title : null))
            .ForMember(d => d.SellerDisplayName, opt => opt.MapFrom(s => s.Seller != null ? s.Seller.DisplayName : null))
            .ForMember(d => d.IssuedAt, opt => opt.MapFrom(s => s.CreatedAt))
            .ForMember(d => d.Currency, opt => opt.MapFrom(_ => "BGN"));

        CreateMap<Feedback, FeedbackDto>()
            .ForMember(d => d.UserDisplayName, opt => opt.MapFrom(s => s.User != null ? s.User.DisplayName : null))
            .ForMember(d => d.UserAvatarUrl, opt => opt.MapFrom(s => s.User != null ? s.User.AvatarUrl : null));

        CreateMap<Shipment, ShipmentDto>()
            .ForMember(d => d.ItemTitle, opt => opt.MapFrom(s =>
                s.Payment != null && s.Payment.Item != null ? s.Payment.Item.Title : null))
            .ForMember(d => d.SellerId, opt => opt.MapFrom(s =>
                s.Payment != null ? s.Payment.SellerId : null))
            .ForMember(d => d.IsCurrentUserSeller, opt => opt.Ignore());

        CreateMap<PurchaseRequest, PurchaseRequestDto>()
            .ForMember(d => d.ItemTitle, opt => opt.MapFrom(s => s.Item != null ? s.Item.Title : null))
            .ForMember(d => d.ItemPhotoUrl, opt => opt.MapFrom(s =>
                s.Item != null && s.Item.Photos != null && s.Item.Photos.Count > 0
                    ? s.Item.Photos.OrderBy(p => p.DisplayOrder).Select(p => p.Url).FirstOrDefault()
                    : null))
            .ForMember(d => d.ListingType, opt => opt.MapFrom(s => s.Item != null ? s.Item.ListingType : default))
            .ForMember(d => d.Price, opt => opt.MapFrom(s => s.Item != null ? s.Item.Price : null))
            .ForMember(d => d.BuyerDisplayName, opt => opt.MapFrom(s => s.Buyer != null ? s.Buyer.DisplayName : null))
            .ForMember(d => d.BuyerAvatarUrl, opt => opt.MapFrom(s => s.Buyer != null ? s.Buyer.AvatarUrl : null))
            .ForMember(d => d.BundleId, opt => opt.MapFrom(s => s.BundleId))
            .ForMember(d => d.BundleTitle, opt => opt.MapFrom(s => s.Bundle != null ? s.Bundle.Title : null))
            .ForMember(d => d.BundlePhotoUrl, opt => opt.MapFrom(s =>
                s.Bundle != null && s.Bundle.BundleItems.Any()
                    ? s.Bundle.BundleItems.First().Item.Photos.OrderBy(p => p.DisplayOrder).Select(p => p.Url).FirstOrDefault()
                    : null))
            .ForMember(d => d.ShipmentId, opt => opt.Ignore());

        CreateMap<DoctorReview, DoctorReviewDto>()
            .ForMember(d => d.AuthorDisplayName, opt => opt.MapFrom(s => s.User != null ? s.User.DisplayName : null))
            .ForMember(d => d.AuthorAvatarUrl, opt => opt.MapFrom(s => s.User != null ? s.User.AvatarUrl : null));

        CreateMap<ChildFriendlyPlace, ChildFriendlyPlaceDto>()
            .ForMember(d => d.AuthorDisplayName, opt => opt.MapFrom(s => s.User != null ? s.User.DisplayName : null));

        CreateMap<UserRating, UserRatingDto>()
            .ForMember(d => d.RaterDisplayName, opt => opt.MapFrom(s => s.Rater != null ? s.Rater.DisplayName : null))
            .ForMember(d => d.RaterAvatarUrl, opt => opt.MapFrom(s => s.Rater != null ? s.Rater.AvatarUrl : null));
    }
}
