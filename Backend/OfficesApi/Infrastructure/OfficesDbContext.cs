using System.Reflection;
using MicroserviceApiKernel.Extensions.Queryable;
using MicroserviceApiKernel.Results;
using MongoDB.Bson;
using MongoDB.Driver;
using OfficesApi.Endpoints.GetOffices;
using OfficesApi.Models;

namespace OfficesApi.Infrastructure;

public class OfficesDbContext(IMongoDatabase database)
{
    public const string OfficesTableName = "offices";
    
    private readonly IMongoCollection<Office> _collection = database.GetCollection<Office>(OfficesTableName);
    public async Task InitializeAsync(CancellationToken ct)
    {
        var collectionNames = await (await database.ListCollectionNamesAsync(null, ct)).ToListAsync(ct);

        if (!collectionNames.Contains(OfficesTableName))
        {
            await database.CreateCollectionAsync(OfficesTableName, cancellationToken: ct);
        }

        var cityIndex = new CreateIndexModel<Office>(
            Builders<Office>.IndexKeys.Ascending(m => m.City));

        var uniqueAddressIndex = new CreateIndexModel<Office>(
            Builders<Office>.IndexKeys
                .Ascending(r => r.City)
                .Ascending(r => r.Street)
                .Ascending(r => r.HouseNumber),
            new CreateIndexOptions
            {
                Unique = true,
                Name = "UX_Office_City_Street_HouseNumber"
            });

        await _collection.Indexes.CreateManyAsync(
            [cityIndex, uniqueAddressIndex]
            , cancellationToken: ct);
    }

    public async Task<Result<string>> Insert(Office office, CancellationToken ct)
    {
        var collection = database.GetCollection<Office>(OfficesTableName);

        try
        {
            await collection.InsertOneAsync(office, null, ct);
            
            return Result.Success(office.Id.ToString());
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Code == 11000)
        {
            return OfficeErrors.AlreadyExists();
        }
        catch (MongoDuplicateKeyException)
        {
            return OfficeErrors.AlreadyExists();
        }
        catch (Exception ex)
        { 
            return new Error(ex.Message, ErrorType.Internal);
        }
    }


    public async Task<Result> UpdateOffice(Office office, CancellationToken ct)
    {
        try
        {
            var result = await _collection.ReplaceOneAsync(x => x.Id == office.Id, office, cancellationToken: ct);

            if (result.MatchedCount == 0)
            {
                return Result.Failure(null);
            }
        }
        catch (Exception)
        {
            throw;
        }

        return Result.Success();
    }

    public async Task<Result> UpdateOfficeActive(Office office, bool active, CancellationToken ct)
    {
        var collection = database.GetCollection<Office>(OfficesTableName);

        try
        {
            var filter = Builders<Office>.Filter.Eq(x => x.Id, office.Id);
            var update = Builders<Office>.Update.Set(x => x.IsActive, active);

            var result = await collection.UpdateOneAsync(filter, update, cancellationToken: ct);

            if (result.MatchedCount == 0)
            {
                return Result.Failure(null);
            }
        }
        catch (Exception)
        {
            throw;
        }

        return Result.Success();
    }

    public async Task<Result<Office>> GetOffice(string officeId, CancellationToken ct)
    {
        if (!ObjectId.TryParse(officeId, out var id))
        {
            return OfficeErrors.NotFound();
        }

        var office = await _collection.Find(x => x.Id == id).FirstOrDefaultAsync(ct);
        
        return office != null 
            ? Result.Success(office) 
            : OfficeErrors.NotFound();
    }
    public async Task<List<Office>> GetAll(PaginationParameters pagination, CancellationToken ct)
    {
        return await _collection.Find(Builders<Office>.Filter.Empty)
            .Skip(pagination.Skip())
            .Limit(pagination.PageSize)
            .ToListAsync(ct);
    }
}