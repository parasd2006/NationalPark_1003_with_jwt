using System.ComponentModel.DataAnnotations;

namespace NationalPark_API_1003.Models.DTOs
{
    public class AppointmentDto
    {
        public int Id { get; set; }

        [Required]
        public int NationalParkId { get; set; }
        public NationalParkDto? NationalPark { get; set; }

        [Required(ErrorMessage = "Customer name is required")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string CustomerEmail { get; set; }

        [Required(ErrorMessage = "Phone is required")]
        public string CustomerPhone { get; set; }

        [Required]
        public DateTime VisitDate { get; set; }

        public DateTime BookingDate { get; set; } = DateTime.UtcNow;

        [Range(1, 5, ErrorMessage = "Adults must be between 1 and 5")]
        public int NumberOfAdults { get; set; } = 1;

        [Range(0, 4)]
        public int NumberOfChildren { get; set; } = 0;

        public decimal TotalAmount { get; set; }

        public string? BookingStatus { get; set; }
        public string? PaymentStatus { get; set; }

        public string? StripeSessionId { get; set; }
        public string? StripePaymentIntentId { get; set; }
        public DateTime? PaymentDate { get; set; }
        public int? TrailId { get; set; }
        public TrailDto? Trail { get; set; }
    }
}