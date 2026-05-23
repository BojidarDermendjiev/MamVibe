using AutoMapper;
using FluentAssertions;

using MomVibe.Domain.Entities;
using MomVibe.Domain.Enums;
using MomVibe.Application.Mapping;
using MomVibe.Application.DTOs.Items;
using MomVibe.Application.DTOs.Users;
using MomVibe.Application.DTOs.Messages;
using MomVibe.Application.DTOs.Payments;
using MomVibe.Application.DTOs.Feedbacks;
using MomVibe.Application.DTOs.Shipping;
using MomVibe.Application.DTOs.PurchaseRequests;
using MomVibe.Application.DTOs.DoctorReviews;
using MomVibe.Application.DTOs.ChildFriendlyPlaces;
using MomVibe.Application.DTOs.UserRatings;
using MomVibe.Application.DTOs.Admin;

namespace MomVibe.UnitTests.Mapping;

public class MappingProfileTests
{
    private readonly IMapper _mapper;

    public MappingProfileTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
    }

    // ── Configuration validity ────────────────────────────────────────────────

    [Fact]
    public void MappingProfile_ConfigurationIsValid()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        // Throws if any destination member is unmapped or any resolver is missing.
        config.AssertConfigurationIsValid();
    }

    // ── ApplicationUser → UserDto ─────────────────────────────────────────────

    [Fact]
    public void ApplicationUser_MapsTo_UserDto_CopiesIdentityFields()
    {
        var user = new ApplicationUser
        {
            Id = "user-123",
            Email = "test@example.com",
            DisplayName = "Test User",
            ProfileType = ProfileType.Female,
            Bio = "Hello world",
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString(),
        };

        var dto = _mapper.Map<UserDto>(user);

        dto.Id.Should().Be("user-123");
        dto.Email.Should().Be("test@example.com");
        dto.DisplayName.Should().Be("Test User");
        dto.ProfileType.Should().Be(ProfileType.Female);
        dto.Bio.Should().Be("Hello world");
    }

    // ── ApplicationUser → AdminUserDto ───────────────────────────────────────

    [Fact]
    public void ApplicationUser_MapsTo_AdminUserDto_ItemCountFromCollection()
    {
        var user = new ApplicationUser
        {
            Id = "admin-456",
            Email = "admin@example.com",
            DisplayName = "Admin",
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            Items = [
                new Item { Title = "A", Description = "d", UserId = "admin-456" },
                new Item { Title = "B", Description = "d", UserId = "admin-456" },
            ]
        };

        var dto = _mapper.Map<AdminUserDto>(user);

        dto.ItemCount.Should().Be(2);
    }

    // ── Item → ItemDto ────────────────────────────────────────────────────────

    [Fact]
    public void Item_MapsTo_ItemDto_WithNullNavigations()
    {
        var item = new Item
        {
            Id = Guid.NewGuid(),
            Title = "Baby Jacket",
            Description = "Almost new, worn twice",
            UserId = "seller-1",
            ListingType = ListingType.Sell,
            Price = 15m,
        };

        var dto = _mapper.Map<ItemDto>(item);

        dto.Id.Should().Be(item.Id);
        dto.Title.Should().Be("Baby Jacket");
        dto.Price.Should().Be(15m);
        dto.CategoryName.Should().BeNull();  // Category navigation not loaded
        dto.UserDisplayName.Should().BeNull();
    }

    [Fact]
    public void Item_MapsTo_ItemDto_WithLoadedCategory()
    {
        var item = new Item
        {
            Id = Guid.NewGuid(),
            Title = "Stroller",
            Description = "Good condition",
            UserId = "seller-2",
            ListingType = ListingType.Donate,
            Category = new Category { Name = "Strollers", Slug = "strollers" },
        };

        var dto = _mapper.Map<ItemDto>(item);

        dto.CategoryName.Should().Be("Strollers");
    }

    // ── Message → MessageDto ─────────────────────────────────────────────────

    [Fact]
    public void Message_MapsTo_MessageDto_TimestampFromCreatedAt()
    {
        var now = DateTime.UtcNow;
        var msg = new Message
        {
            Id = Guid.NewGuid(),
            SenderId = "s1",
            ReceiverId = "r1",
            Content = "Hello",
            CreatedAt = now,
        };

        var dto = _mapper.Map<MessageDto>(msg);

        dto.Timestamp.Should().Be(now);
        dto.Content.Should().Be("Hello");
        dto.SenderDisplayName.Should().BeNull();  // Sender not loaded
    }

    // ── Feedback → FeedbackDto ───────────────────────────────────────────────

    [Fact]
    public void Feedback_MapsTo_FeedbackDto_WithNullUser()
    {
        var fb = new Feedback
        {
            Id = Guid.NewGuid(),
            UserId = "u1",
            Rating = 4,
            Category = FeedbackCategory.Praise,
            Content = "Great!",
        };

        var dto = _mapper.Map<FeedbackDto>(fb);

        dto.Id.Should().Be(fb.Id);
        dto.Rating.Should().Be(4);
        dto.Content.Should().Be("Great!");
        dto.UserDisplayName.Should().BeNull();
    }

    // ── Payment → PaymentDto ─────────────────────────────────────────────────

    [Fact]
    public void Payment_MapsTo_PaymentDto_ItemTitleFromNavigation()
    {
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            BuyerId = "b1",
            SellerId = "s1",
            Amount = 29.99m,
            Status = PaymentStatus.Completed,
            Item = new Item { Title = "Winter Boots", Description = "d", UserId = "s1" },
        };

        var dto = _mapper.Map<PaymentDto>(payment);

        dto.Amount.Should().Be(29.99m);
        dto.ItemTitle.Should().Be("Winter Boots");
    }

    // ── PurchaseRequest → PurchaseRequestDto ─────────────────────────────────

    [Fact]
    public void PurchaseRequest_MapsTo_PurchaseRequestDto_WithNullNavigations()
    {
        var pr = new PurchaseRequest
        {
            Id = Guid.NewGuid(),
            ItemId = Guid.NewGuid(),
            BuyerId = "buyer-1",
            SellerId = "seller-1",
            Status = PurchaseRequestStatus.Pending,
        };

        var dto = _mapper.Map<PurchaseRequestDto>(pr);

        dto.Id.Should().Be(pr.Id);
        dto.Status.Should().Be(PurchaseRequestStatus.Pending);
        dto.ItemTitle.Should().BeNull();
        dto.BuyerDisplayName.Should().BeNull();
    }

    // ── DoctorReview → DoctorReviewDto ───────────────────────────────────────

    [Fact]
    public void DoctorReview_MapsTo_DoctorReviewDto()
    {
        var review = new DoctorReview
        {
            Id = Guid.NewGuid(),
            UserId = "u1",
            DoctorName = "Dr. Smith",
            Specialization = "Pediatrics",
            City = "Sofia",
            Rating = 5,
            Content = "Excellent.",
        };

        var dto = _mapper.Map<DoctorReviewDto>(review);

        dto.DoctorName.Should().Be("Dr. Smith");
        dto.Rating.Should().Be(5);
    }

    // ── ChildFriendlyPlace → ChildFriendlyPlaceDto ───────────────────────────

    [Fact]
    public void ChildFriendlyPlace_MapsTo_ChildFriendlyPlaceDto()
    {
        var place = new ChildFriendlyPlace
        {
            Id = Guid.NewGuid(),
            UserId = "u1",
            Name = "Sunny Park",
            Description = "Great for toddlers",
            City = "Plovdiv",
            PlaceType = PlaceType.Park,
        };

        var dto = _mapper.Map<ChildFriendlyPlaceDto>(place);

        dto.Name.Should().Be("Sunny Park");
        dto.City.Should().Be("Plovdiv");
        dto.PlaceType.Should().Be(PlaceType.Park);
    }

    // ── UserRating → UserRatingDto ────────────────────────────────────────────

    [Fact]
    public void UserRating_MapsTo_UserRatingDto_RaterDisplayNameFromNavigation()
    {
        var rating = new UserRating
        {
            Id = Guid.NewGuid(),
            RaterId = "rater-1",
            RatedUserId = "seller-1",
            Rating = 4,
            Comment = "Good seller",
            Rater = new ApplicationUser
            {
                Id = "rater-1",
                DisplayName = "John",
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
            }
        };

        var dto = _mapper.Map<UserRatingDto>(rating);

        dto.Rating.Should().Be(4);
        dto.RaterDisplayName.Should().Be("John");
    }
}
