using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLKhachSan.Models
{
    public class Room
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [ForeignKey("ResortId")]
        public int ResortId { get; set; }
        public Resort Resort { get; set; }
        [ForeignKey("CategoryRoomId")]
        public int CategoryRoomId { get; set; }
        public CategoryRoom CategoryRoom { get; set; }
        public string Status { get; set; }

        public double PriceDay { get; set; }
        public double PriceHour { get; set; }
        public double PriceWeek { get; set; }
        public double PriceMonth { get; set; }
        

    }
}
