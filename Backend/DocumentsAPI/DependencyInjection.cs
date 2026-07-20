using DocumentsAPI.Application;
using DocumentsAPI.Infrastructure;
using DocumentsAPI.Infrastructure.Locking;
using DocumentsAPI.Infrastructure.Pdf;
using DocumentsAPI.Infrastructure.Photos;
using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;

namespace DocumentsAPI;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddServices()
        {
            services.AddScoped<ProfilePhotoRepository>();
            services.AddScoped<PublicPhotoRepository>();

            services.AddScoped<IUserPhotoStorage, UserPhotoStorage>();
            services.AddScoped<IPublicPhotoStorage, PublicPhotoStorage>();
            
            services.AddSingleton<IDistributedLockService, DistributedLockService>();
            services.AddScoped<IMedicalResultStorage, MedicalResultBlobStorage>();
            services.AddScoped<MedicalResultService>();
            services.AddScoped<IPdfMedicalResultGenerator, QuestPdfMedicalResultGenerator>();

            services.AddScoped<BlobDbContext>();
            services.AddScoped<MedicalResultsDbContext>();
            
            services.AddSingleton<IDistributedLockFactory>(sp => 
            {
                var connection = sp.GetRequiredService<IConnectionMultiplexer>();
                return RedLockFactory.Create([new RedLockMultiplexer(connection)]);
            });
            
            return services;
        }    
    }
}