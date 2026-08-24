using DocumentsAPI.Application;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.Redis;
using DocumentsAPI.Infrastructure.Locking;
using DocumentsAPI.Options;
using Microsoft.Extensions.Options;
using NSubstitute;
using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;


namespace DocumentsApiTests;


public class PdfGenerationIntegrationTest :IAsyncLifetime
{
    private readonly RedisContainer _redisContainer = new RedisBuilder()
        .WithImage("redis:7-alpine")
        .Build();

    public async Task InitializeAsync() => await _redisContainer.StartAsync();
    public async Task DisposeAsync() => await _redisContainer.DisposeAsync();

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetOrCreate_ConcurrentRequests_GeneratesPdfOnlyOnce()
    {
        var redisConnString = _redisContainer.GetConnectionString();

        var parts = redisConnString.Split(':');
        var host = parts[0];
        var port = int.Parse(parts[1]);

        var dnsEndPoint = new System.Net.DnsEndPoint(host, port);
        var redLockEndPoint = new RedLockEndPoint(dnsEndPoint);

        IDistributedLockFactory redLockFactory = RedLockFactory.Create(new List<RedLockEndPoint> { redLockEndPoint });

        var lockService = new DistributedLockService(redLockFactory);

        var options = Options.Create(new PdfGenerationLockOptions
        {
            ExpireTime = TimeSpan.FromSeconds(5),
            WaitTime = TimeSpan.FromMilliseconds(50),
            AcquireRetryTime = TimeSpan.FromMilliseconds(10)
        });
        
        var storage = Substitute.For<IMedicalResultStorage>();
        var pdfGenerator = Substitute.For<IPdfMedicalResultGenerator>();
        
        var isCacheValid = false;
        storage.GetMedicalResultInfoAsync(Arg.Any<AppointmentKey>(), Arg.Any<CancellationToken>())
            .Returns(_ => (isCacheValid, isCacheValid ? DateTimeOffset.UtcNow : DateTimeOffset.MinValue));
        
        storage.UploadPdfAsync(Arg.Any<AppointmentKey>(), Arg.Any<byte[]>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                isCacheValid = true;
                return Task.CompletedTask;
            });
        storage.GetMedicalResultInfoAsync(Arg.Any<AppointmentKey>(), Arg.Any<CancellationToken>())
            .Returns(_ => (isCacheValid, isCacheValid ? DateTimeOffset.UtcNow : DateTimeOffset.MinValue));
        
        var service = new MedicalResultService(pdfGenerator, storage, lockService, options);
        
        var patientId = Guid.NewGuid();
        var data = new MedicalResultPdfData(Guid.NewGuid(), default, default, default, default, default, default,
            default, default, default, default);

        // Act
        var task1 = service.GetOrCreateMedicalResultPdfAsync(patientId, DateTimeOffset.UtcNow, data, CancellationToken.None);
        var task2 = service.GetOrCreateMedicalResultPdfAsync(patientId, DateTimeOffset.UtcNow, data, CancellationToken.None);

        var results = await Task.WhenAll(task1, task2);

        // Assert
        pdfGenerator.Received(1).Generate(Arg.Any<MedicalResultPdfData>());

        Assert.All(results, r => Assert.False(r.IsError));
    }
}