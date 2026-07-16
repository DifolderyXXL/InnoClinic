using DocumentsAPI.Models;
using DocumentsAPI.Models.Errors;
using MicroserviceApiKernel.Results;
using MongoDB.Driver;

namespace DocumentsAPI.Data;

public class MedicalResultsDbContext(IMongoDatabase database)
{
    public const string MedicalResultTableName = "medicalResults";

    private readonly IMongoCollection<MedicalResult> _collection = database.GetCollection<MedicalResult>(MedicalResultTableName);
    
    public async Task InitializeAsync(CancellationToken ct)
    {
        var collectionNames = await (await database.ListCollectionNamesAsync(null, ct)).ToListAsync(ct);

        if (!collectionNames.Contains(MedicalResultTableName))
        {
            await database.CreateCollectionAsync(MedicalResultTableName, cancellationToken: ct);
        }

        var userIndex = new CreateIndexModel<MedicalResult>(
            Builders<MedicalResult>.IndexKeys.Ascending(m => m.UserId));

        await _collection.Indexes.CreateOneAsync(userIndex, cancellationToken: ct);
    }
    
    
    public async Task<Result> InsertAsync(MedicalResult result, CancellationToken ct)
    {

        try
        {
            await _collection.InsertOneAsync(result, null, ct);
            return Result.Success();
        }
        catch (Exception ex) when (ex is MongoWriteException { WriteError.Code: 11000 } || ex is MongoDuplicateKeyException)
        {
            return MedicalResultErrors.AlreadyExists();
        }
    }
    
    public async Task<Result> UpdateAsync(MedicalResult result, CancellationToken ct)
    {
        try
        {
            var filter = Builders<MedicalResult>.Filter.Eq(r => r.AppointmentId, result.AppointmentId);

            var replacementResult = await _collection.ReplaceOneAsync(filter, result, new ReplaceOptions(), ct);

            if (replacementResult.MatchedCount == 0)
            {
                return MedicalResultErrors.NotFound();
            }
            
            return Result.Success();
        }
        catch (Exception ex) when (ex is MongoWriteException { WriteError.Code: 11000 } || ex is MongoDuplicateKeyException)
        {
            return MedicalResultErrors.AlreadyExists();
        }
    }

    public async Task<Result<MedicalResult>> GetByAppointmentIdAsync(Guid appointmentId, CancellationToken ct)
    {
        var result = await _collection.Find(x => x.AppointmentId == appointmentId).FirstOrDefaultAsync(ct);
        
        return result != null 
            ? Result.Success(result) 
            : MedicalResultErrors.NotFound();
    }
}