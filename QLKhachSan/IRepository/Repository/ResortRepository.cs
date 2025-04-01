using QLKhachSan.Data;
using QLKhachSan.Models;
using QLKhachSan.Repository.IRepository;

namespace QLKhachSan.Repository
{
    public class ResortRepository : Repository<Resort>, IResortRepository
    {
        private readonly ApplicationDbContext _db;
        public ResortRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public async Task<Resort> UpdateAsync(Resort entity)
        {
            _db.Resorts.Update(entity);
            await _db.SaveChangesAsync();
            return entity;
        }
    }
}
