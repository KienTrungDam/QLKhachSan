namespace QLKhachSan.Models
{
    public class Staff : Person
    {
        public string Position { get; set; }
        public decimal Salary { get; set; }
        public DateTime HireDate { get; set; }
    }
}
