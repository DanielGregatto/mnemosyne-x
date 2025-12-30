using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Identity.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Moq;
using Services.Features.Account.Queries.GetUserProfile;
using Unit.Tests.Fixtures;
using Unit.Tests.Helpers;
using Xunit;

namespace Unit.Tests.Services.Queries;

public class GetUserProfileQueryHandlerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<IStringLocalizer<Domain.Resources.Messages>> _mockLocalizer;
    private readonly ApplicationUserFixture _userFixture;

    public GetUserProfileQueryHandlerTests()
    {
        _mockUserManager = MockUserManagerHelper.CreateMockUserManager();
        _mockLocalizer = new Mock<IStringLocalizer<Domain.Resources.Messages>>();

        // Setup localizer to return non-null strings
        _mockLocalizer.Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));

        _userFixture = new ApplicationUserFixture();
    }

    [Fact]
    public async Task Handle_ExistingUser_ShouldReturnProfile()
    {
        // Arrange
        var existingUser = _userFixture.GenerateUser();
        _mockUserManager.SetupFindByIdAsync(existingUser);

        var query = new GetUserProfileQuery { UserId = Guid.Parse(existingUser.Id) };

        var handler = new GetUserProfileQueryHandler(
            _mockUserManager.Object,
            _mockLocalizer.Object
        );

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Id.Should().Be(Guid.Parse(existingUser.Id));
        result.Data.Email.Should().Be(existingUser.Email);
        result.Data.FullName.Should().Be(existingUser.FullName);
        result.Data.CPF_CNPJ.Should().Be(existingUser.CPF_CNPJ);
        result.Data.PhoneNumber.Should().Be(existingUser.PhoneNumber);
    }

    [Fact]
    public async Task Handle_NonExistentUser_ShouldReturnNotFound()
    {
        // Arrange
        _mockUserManager.SetupFindByIdAsync(null);

        var query = new GetUserProfileQuery { UserId = Guid.NewGuid() };

        var handler = new GetUserProfileQueryHandler(
            _mockUserManager.Object,
            _mockLocalizer.Object
        );

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
    }
}
