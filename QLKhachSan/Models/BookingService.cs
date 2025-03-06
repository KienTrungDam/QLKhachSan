using System.ComponentModel.DataAnnotations.Schema;

namespace QLKhachSan.Models
{
    public class BookingService
    {
        public int Id { get; set; }
        [ForeignKey("ServiceId")]
        public int ServiceId { get; set; }
        public Service Service { get; set; }
        public int Quantity { get; set; }
        public double ToTalPrice { get; set; }
    }
}
