namespace final_project_Core.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id, CancellationToken ct);
        Task<IEnumerable<T>> GetAllAsync(CancellationToken ct);
        Task AddAsync(T entity, CancellationToken ct);
        void Update(T entity);
        void Remove(T entity);
        Task<int> SaveChangesAsync(CancellationToken ct);
    }
}
