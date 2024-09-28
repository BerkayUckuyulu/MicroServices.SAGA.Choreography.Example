using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;

namespace Stock.API.Services;

public class StockAPIDbService
{
    readonly IMongoDatabase _database;
    public StockAPIDbService(IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("MongoDB")!;
        MongoClient client = new(connectionString);
        _database = client.GetDatabase(new MongoUrl(connectionString).DatabaseName);
    }

    public IMongoCollection<T> GetCollection<T>()
    {
        return _database.GetCollection<T>(typeof(T).Name.ToLowerInvariant());
    }
}

