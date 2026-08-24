using Contracts.DocumentsContracts;
using DocumentsAPI.Consumers;
using DocumentsAPI.Infrastructure.Photos;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace DocumentsApiTests;

public class ConfirmProfilePhotoConsumerTests : IAsyncLifetime
{
    private readonly ServiceProvider _provider;
    private readonly ITestHarness _harness;
    private readonly Mock<IUserPhotoStorage> _photoStorageMock = new();

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _photoId = Guid.NewGuid();
    private readonly Guid _oldPhotoId = Guid.NewGuid();

    public ConfirmProfilePhotoConsumerTests()
    {
        _provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<ConfirmProfilePhotoConsumer>();
            })
            .AddSingleton(_photoStorageMock.Object)
            .BuildServiceProvider(true);

        _harness = _provider.GetRequiredService<ITestHarness>();
    }

    public async Task InitializeAsync()
    {
        await _harness.Start();
    }

    public async Task DisposeAsync()
    {
        await _provider.DisposeAsync();
    }

    [Fact]
    public async Task Consume_PublicWithOldPhoto_DeletesOldPhotoAndConfirmsNew()
    {
        // Arrange
        var message = new ConfirmProfilePhoto(_userId, _photoId, _oldPhotoId, IsPublicUser: true);

        // Act
        await _harness.Bus.Publish(message);

        // Assert
        Assert.True(await _harness.Consumed.Any<ConfirmProfilePhoto>(x => x.Context.Message.UserId == _userId));

        _photoStorageMock.Verify(x => x.DeletePhotoAsync(_userId.ToString(), _oldPhotoId, It.IsAny<CancellationToken>()), Times.Once);
        _photoStorageMock.Verify(x => x.ConfirmPhotoAsync(_userId.ToString(), _photoId, It.IsAny<CancellationToken>()), Times.Once);
        _photoStorageMock.Verify(x => x.SetPublicity(_userId.ToString(), _photoId, true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Consume_PrivateWithoutOldPhoto_DoesNotDeleteAndConfirmsNew()
    {
        // Arrange
        var message = new ConfirmProfilePhoto(_userId, _photoId, OldPhoto: null, IsPublicUser: false);

        // Act
        await _harness.Bus.Publish(message);

        // Assert
        Assert.True(await _harness.Consumed.Any<ConfirmProfilePhoto>(x => x.Context.Message.UserId == _userId));

        _photoStorageMock.Verify(x => x.DeletePhotoAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        
        _photoStorageMock.Verify(x => x.ConfirmPhotoAsync(_userId.ToString(), _photoId, It.IsAny<CancellationToken>()), Times.Once);
        _photoStorageMock.Verify(x => x.SetPublicity(_userId.ToString(), _photoId, false, It.IsAny<CancellationToken>()), Times.Once);
    }
}