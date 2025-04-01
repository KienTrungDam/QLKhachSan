using QLKhachSan.IRepository;
using QLKhachSan.Repository.IRepository;

namespace QLKhachSan.Repository.IRepository
{
    public interface IUnitOfWork
    {
        IUserRepository User { get; }
        ICategoryRoomRepository CategoryRoom { get; }
        IRoomRepository Room { get; }
        IUserManagementRepository UserManagement { get; }
        IResortRepository Resort { get; }
        IServiceRepository Service { get; }
        Task SaveAsync();
    }
}
