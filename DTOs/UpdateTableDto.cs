using System.ComponentModel.DataAnnotations;

namespace RestaurangMVCLab2.DTOs
{
    public class UpdateTableDto
    {
        [Range(1, 999, ErrorMessage = "Bordsnummer måste vara mellan 1 och 999")]
        [Display(Name = "Bordsnummer")]
        public int? TableNumber { get; set; }

        [Range(1, 20, ErrorMessage = "Kapacitet måste vara mellan 1 och 20")]
        [Display(Name = "Antal platser")]
        public int? Capacity { get; set; }

        [Display(Name = "Aktivt")]
        public bool? IsActive { get; set; }
    }
}
