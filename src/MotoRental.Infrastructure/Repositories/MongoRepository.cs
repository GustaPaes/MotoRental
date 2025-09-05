using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MotoRental.Application.Interfaces;
using System.Linq.Expressions;

namespace MotoRental.Infrastructure.Repositories
{
    public class MongoRepository<T>(IMongoDatabase database, string collectionName, ILogger<MongoRepository<T>> logger) : IMongoRepository<T>
    {
        private readonly IMongoCollection<T> _collection = database.GetCollection<T>(collectionName);

        public async Task<IEnumerable<T>> FindAllAsync()
        {
            try
            {
                return await _collection.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error finding all documents");
                throw;
            }
        }

        public async Task<T> FindByIdAsync(string id)
        {
            try
            {
                var filter = Builders<T>.Filter.Eq("_id", id);
                return await _collection.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error finding document by ID: {Id}", id);
                throw;
            }
        }

        public async Task InsertOneAsync(T document)
        {
            try
            {
                await _collection.InsertOneAsync(document);
                logger.LogInformation("Document inserted successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error inserting document");
                throw;
            }
        }

        public async Task ReplaceOneAsync(string id, T document)
        {
            try
            {
                var filter = Builders<T>.Filter.Eq("_id", id);
                await _collection.ReplaceOneAsync(filter, document);
                logger.LogInformation("Document with ID {Id} replaced successfully", id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error replacing document with ID: {Id}", id);
                throw;
            }
        }

        public async Task DeleteByIdAsync(string id)
        {
            try
            {
                var filter = Builders<T>.Filter.Eq("_id", id);
                await _collection.DeleteOneAsync(filter);
                logger.LogInformation("Document with ID {Id} deleted successfully", id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting document with ID: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<T>> FilterByAsync(Expression<Func<T, bool>> filterExpression)
        {
            try
            {
                return await _collection.Find(filterExpression).ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error filtering documents");
                throw;
            }
        }
    }
}
