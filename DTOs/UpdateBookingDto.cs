using System.ComponentModel.DataAnnotations;

namespace RestaurangMVCLab2.DTOs
{
    public class UpdateBookingDto
    {
        [Display(Name = "Datum och tid")]
        public DateTime? BookingDateTime { get; set; }

        [Range(1, 20, ErrorMessage = "Antal gäster måste vara mellan 1 och 20")]
        [Display(Name = "Antal gäster")]
        public int? NumberOfGuests { get; set; }

        [StringLength(500)]
        [Display(Name = "Specialönskemål")]
        public string? SpecialRequests { get; set; }
    }
}
