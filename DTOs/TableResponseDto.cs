namespace RestaurangMVCLab2.DTOs
{
    public class TableResponseDto
    {
        public int Id { get; set; }
        public int TableNumber { get; set; }
        public int Capacity { get; set; }
        public bool IsActive { get; set; }
        public int CurrentBookingsCount { get; set; }
    }
}
