using final_project_Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace final_project_Data.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext Context;
        protected readonly DbSet<T> DbSet;

        public Repository(AppDbContext context)
        {
            Context = context;
            DbSet = context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(int id, CancellationToken ct) => await DbSet.FindAsync(new object?[] { id }, ct);

        public async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct) => await DbSet.ToListAsync(ct);

        public async Task AddAsync(T entity, CancellationToken ct) => await DbSet.AddAsync(entity, ct);

        public void Update(T entity) => DbSet.Update(entity);

        public void Remove(T entity) => DbSet.Remove(entity);

        public async Task<int> SaveChangesAsync(CancellationToken ct) => await Context.SaveChangesAsync(ct);
    }
}
