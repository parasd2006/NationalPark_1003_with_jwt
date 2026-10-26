using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace NationalParkWebApp_1003.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        [Required]
        public int NationalParkId { get; set; }
        [ValidateNever]
        public NationalPark? NationalPark { get; set; }

        // --- Trail properties ---
        [Required(ErrorMessage = "Please select a trail")]
        [Display(Name = "Select Trail")]
        public int? TrailId { get; set; }
        [ValidateNever]
        public Trail? Trail { get; set; }
        // -------------------------

        [Required(ErrorMessage = "Please enter your name")]
        [Display(Name = "Full Name")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Please enter email")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [Display(Name = "Email Address")]
        public string CustomerEmail { get; set; }

        [Required(ErrorMessage = "Please enter phone number")]
        [Display(Name = "Phone Number")]
        public string CustomerPhone { get; set; }

        [Required(ErrorMessage = "Please select visit date")]
        [DataType(DataType.Date)]
        [Display(Name = "Visit Date")]
        public DateTime VisitDate { get; set; } = DateTime.Today.AddDays(1);

        public DateTime BookingDate { get; set; } = DateTime.UtcNow;

        [Required]
        [Range(1, 5, ErrorMessage = "Adults count must be between 1 and 5")]
        [Display(Name = "Number of Adults (₹100 each)")]
        public int NumberOfAdults { get; set; } = 1;

        [Range(0, 4, ErrorMessage = "Children count cannot exceed 4")]
        [Display(Name = "Number of Children (₹50 each)")]
        public int NumberOfChildren { get; set; } = 0;

        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        public string? BookingStatus { get; set; }
        public string? PaymentStatus { get; set; }

        public string? StripeSessionId { get; set; }
        public string? StripePaymentIntentId { get; set; }
        public DateTime? PaymentDate { get; set; }
    }
}