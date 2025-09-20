// DTOs/BookingResponseDto.cs (kopiera exakt från API)
namespace RestaurangMVCLab2.DTOs
{
    public class BookingResponseDto
    {
        public int Id { get; set; }
        public DateTime BookingDateTime { get; set; }
        public int NumberOfGuests { get; set; }
        public string? SpecialRequests { get; set; }
        public DateTime CreatedAt { get; set; }
        public CustomerSummaryDto Customer { get; set; } = null!;
        public TableSummaryDto Table { get; set; } = null!;
        public DateTime EndTime => BookingDateTime.AddHours(2);
    }

    public class CustomerSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }

    public class TableSummaryDto
    {
        public int Id { get; set; }
        public int TableNumber { get; set; }
        public int Capacity { get; set; }
    }
}