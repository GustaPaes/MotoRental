using System.Linq.Expressions;

namespace MotoRental.Application.Interfaces
{
    public interface IMongoRepository<T>
    {
        Task<IEnumerable<T>> FindAllAsync();
        Task<T> FindByIdAsync(string id);
        Task InsertOneAsync(T document);
        Task ReplaceOneAsync(string id, T document);
        Task DeleteByIdAsync(string id);
        Task<IEnumerable<T>> FilterByAsync(Expression<Func<T, bool>> filterExpression);
    }
}
