using System;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;
using OfficesApi.Models;

namespace OfficesApi.Infrastructure;

public class OfficesDbContext(IMongoDatabase database)
{
    public const string OfficesTableName = "offices";
    public async Task InitializeAsync(CancellationToken ct)
    {
        var collectionNames = await database.ListCollectionNames().ToListAsync(ct);

        if (!collectionNames.Contains(OfficesTableName))
        {
            await database.CreateCollectionAsync(OfficesTableName, cancellationToken: ct);
        }

        var collection = database.GetCollection<Office>(OfficesTableName);
        var indexModel = new CreateIndexModel<Office>(Builders<Office>.IndexKeys
            .Ascending(m => m.City));

        var indexKeysDefinition = Builders<Office>.IndexKeys
            .Ascending(r => r.City)
            .Ascending(r => r.Street)
            .Ascending(r => r.HouseNumber);

        var indexOptions = new CreateIndexOptions
        {
            Unique = true,
            Name = "UX_Office_City_Street_HouseNumber"
        };

        await collection.Indexes.CreateManyAsync(
            [indexModel,
            new CreateIndexModel<Office>(indexKeysDefinition, indexOptions)]
            , cancellationToken: ct);
    }

    public async Task Insert(Office office, CancellationToken ct)
    {
        var collection = database.GetCollection<Office>(OfficesTableName);

        try
        {
            await collection.InsertOneAsync(office, null, ct);
        }
        catch (MongoDuplicateKeyException)
        {
            throw new InvalidOperationException($"Office with ID {office.Id} already exists.");
        }
    }
}
