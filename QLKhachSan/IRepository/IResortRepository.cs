

using QLKhachSan.Models;

namespace QLKhachSan.Repository.IRepository
{
    public interface IResortRepository : IRepository<Resort>
    {
        Task<Resort> UpdateAsync(Resort entity);
    }
}
