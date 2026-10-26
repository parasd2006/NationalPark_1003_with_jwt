using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NationalPark_API_1003.Models
{
    public class Appointment
    {
        
        public int Id { get; set; }

        [Required]
        public int NationalParkId { get; set; }

        [ForeignKey("NationalParkId")]
        public NationalPark? NationalPark { get; set; }
      
        public int? TrailId { get; set; }

        [ForeignKey("TrailId")]
        public Trail? Trail { get; set; }

        [Required]
        public string CustomerName { get; set; }

        [Required]
        [EmailAddress]
        public string CustomerEmail { get; set; }

        [Required]
        public string CustomerPhone { get; set; }

        [Required]
        public DateTime VisitDate { get; set; }

        public DateTime BookingDate { get; set; } = DateTime.UtcNow;

        [Range(1, 5)]
        public int NumberOfAdults { get; set; } = 1;

        [Range(0, 4)]
        public int NumberOfChildren { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public string BookingStatus { get; set; } = "Pending";
        public string PaymentStatus { get; set; } = "Pending";

        public string? StripeSessionId { get; set; }
        public string? StripePaymentIntentId { get; set; }
        public DateTime? PaymentDate { get; set; }
    }
}
