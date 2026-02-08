using FluentAssertions;
using MomVibe.Domain.Entities;
using MomVibe.Domain.Enums;

namespace MomVibe.UnitTests.Entities;

public class EntityTests
{
    [Fact]
    public void Item_Should_Initialize_With_Default_Values()
    {
        var item = new Item
        {
            Title = "Test Item",
            Description = "Test Description",
            UserId = "user-1"
        };

        item.Id.Should().NotBe(Guid.Empty);
        item.IsActive.Should().BeTrue();
        item.ViewCount.Should().Be(0);
        item.LikeCount.Should().Be(0);
    }

    [Fact]
    public void ApplicationUser_Should_Set_Properties()
    {
        var user = new ApplicationUser
        {
            DisplayName = "Jane Doe",
            ProfileType = ProfileType.Female,
            Bio = "Hello!",
            LanguagePreference = "en"
        };

        user.DisplayName.Should().Be("Jane Doe");
        user.ProfileType.Should().Be(ProfileType.Female);
        user.Bio.Should().Be("Hello!");
        user.IsBlocked.Should().BeFalse();
    }

    [Fact]
    public void RefreshToken_IsActive_Should_Return_True_When_Valid()
    {
        var token = new RefreshToken
        {
            Token = "test-token",
            UserId = "user-1",
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        token.IsActive.Should().BeTrue();
        token.IsExpired.Should().BeFalse();
        token.IsRevoked.Should().BeFalse();
    }

    [Fact]
    public void RefreshToken_IsExpired_Should_Return_True_When_Expired()
    {
        var token = new RefreshToken
        {
            Token = "test-token",
            UserId = "user-1",
            ExpiresAt = DateTime.UtcNow.AddDays(-1)
        };

        token.IsExpired.Should().BeTrue();
        token.IsActive.Should().BeFalse();
    }

    [Fact]
    public void RefreshToken_IsRevoked_Should_Return_True_When_Revoked()
    {
        var token = new RefreshToken
        {
            Token = "test-token",
            UserId = "user-1",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            RevokedAt = DateTime.UtcNow
        };

        token.IsRevoked.Should().BeTrue();
        token.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Category_Should_Set_Properties()
    {
        var category = new Category
        {
            Name = "Clothes",
            Description = "Baby clothes",
            Slug = "clothes"
        };

        category.Name.Should().Be("Clothes");
        category.Slug.Should().Be("clothes");
    }

    [Fact]
    public void Like_Should_Set_Composite_Key()
    {
        var like = new Like
        {
            UserId = "user-1",
            ItemId = Guid.NewGuid()
        };

        like.UserId.Should().Be("user-1");
        like.ItemId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Payment_Should_Set_Properties()
    {
        var payment = new Payment
        {
            ItemId = Guid.NewGuid(),
            BuyerId = "buyer-1",
            SellerId = "seller-1",
            Amount = 29.99m,
            PaymentMethod = PaymentMethod.Card,
            Status = PaymentStatus.Pending
        };

        payment.Amount.Should().Be(29.99m);
        payment.Status.Should().Be(PaymentStatus.Pending);
    }
}
