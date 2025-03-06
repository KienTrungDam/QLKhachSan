using System.ComponentModel.DataAnnotations.Schema;

namespace QLKhachSan.Models
{
    public class Booking
    {
        public int Id { get; set; }
        [ForeignKey("RoomId")]
        public int RoomId { get; set; }
        public Room Room { get; set; }
        [ForeignKey("PersonId")]
        public string PersonId { get; set; }
        public Person Person { get; set; }
        [ForeignKey("BookingServiceId")]
        public int BookingServiceId { get; set; }
        public BookingService BookingService { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public double TotalPrice { get; set; }
    }
}
