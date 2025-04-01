using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QLKhachSan.Models;

namespace QLKhachSan.Data
{
    public class ApplicationDbContext : IdentityDbContext<Person>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)

        {
        }
        public DbSet<Person> Persons { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingService> BookingServices { get; set; }
        public DbSet<CategoryRoom> CategoryRooms { get; set; }
        public DbSet<Payment> Payments { get; set; }

        public DbSet<Resort> Resorts { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<RoomImage> RoomImages { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<BookingServiceDetail> BookingServiceDetails { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<CategoryRoom>().HasData(
                
                new CategoryRoom { Id = 1, Name = "Phòng đơn", Description = "Phòng đơn", Capacity = 50},
                new CategoryRoom { Id = 2, Name = "Phòng đôi", Description = "Phòng đơn", Capacity = 70},
                new CategoryRoom { Id = 3, Name = "Phòng Vip", Description = "Phòng đơn", Capacity = 80},
                new CategoryRoom { Id = 4, Name = "Phòng SUpperVip", Description = "Phòng đơn", Capacity = 100}
                );
            modelBuilder.Entity<Resort>().HasData(
                new Resort { Id = 1, Name = "Resort 1", Address = "123", PhoneNumber = "123", Rate = 5 },
                new Resort { Id = 2, Name = "Resort 2", Address = "123", PhoneNumber = "123", Rate = 4 },
                new Resort { Id = 3, Name = "Resort 3", Address = "123", PhoneNumber = "123", Rate = 2 },
                new Resort { Id = 4, Name = "Resort 4", Address = "123", PhoneNumber = "123", Rate = 1 }
                );
            modelBuilder.Entity<Room>().HasData(
                new Room { Id = 1, CategoryRoomId = 1, ResortId = 1, Status = "Trống", PriceDay = 100, PriceHour = 10, PriceWeek = 500, PriceMonth = 2000 },
                new Room { Id = 2, CategoryRoomId = 2, ResortId = 2, Status = "Đã thuê", PriceDay = 100, PriceHour = 10, PriceWeek = 500, PriceMonth = 2000 },
                new Room { Id = 3, CategoryRoomId = 2, ResortId = 2, Status = "Đã thuê", PriceDay = 100, PriceHour = 10, PriceWeek = 500, PriceMonth = 2000 },
                new Room { Id = 4, CategoryRoomId = 3, ResortId = 1, Status = "Trống", PriceDay = 100, PriceHour = 10, PriceWeek = 500, PriceMonth = 2000 }
                );
                
                
        }
    }
}
