using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NationalPark_API_1003.Models;
using NationalPark_API_1003.Models.DTOs;
using NationalPark_API_1003.Repository.IRepository;
using Stripe;

namespace NationalPark_API_1003.Controllers
{
    [Route("api/appointments")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentRepository _appointmentRepo;
        private readonly IMapper _mapper;

        public AppointmentController(IAppointmentRepository appointmentRepo, IMapper mapper)
        {
            _appointmentRepo = appointmentRepo;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult GetAppointments()
        {
            var appointmentList = _appointmentRepo.GetAppointments();
            var dtoList = _mapper.Map<List<AppointmentDto>>(appointmentList);
            return Ok(dtoList);
        }

        [HttpGet("{id:int}", Name = "GetAppointment")]
        public IActionResult GetAppointment(int id)
        {
            var appointment = _appointmentRepo.GetAppointment(id);
            if (appointment == null)
                return NotFound();

            var dto = _mapper.Map<AppointmentDto>(appointment);
            return Ok(dto);
        }

        [HttpGet("fullyBookedDates/{nationalParkId:int}")]
        public IActionResult GetFullyBookedDates(int nationalParkId)
        {
            var dates = _appointmentRepo.GetFullyBookedDates(nationalParkId);
            return Ok(dates);
        }

        [HttpPost]
        public IActionResult CreateAppointment([FromBody] AppointmentDto appointmentDto)
        {
            if (appointmentDto == null)
                return BadRequest(ModelState);

            int alreadyBooked = _appointmentRepo.GetTotalPersonsBookedByDate(appointmentDto.NationalParkId, appointmentDto.VisitDate);
            int newPersons = appointmentDto.NumberOfAdults + appointmentDto.NumberOfChildren;

            if ((alreadyBooked + newPersons) > 100)
            {
                int remaining = 100 - alreadyBooked;
                if (remaining <= 0)
                {
                    ModelState.AddModelError("", "This National Park is fully booked for the selected date! (100 persons limit)");
                }
                else
                {
                    ModelState.AddModelError("", $"Only {remaining} slots remaining for this date. You tried to book {newPersons}.");
                }
                return BadRequest(ModelState);
            }

            appointmentDto.TotalAmount = (appointmentDto.NumberOfAdults * 100) + (appointmentDto.NumberOfChildren * 50);

            var appointment = _mapper.Map<Appointment>(appointmentDto);
            appointment.Id = 0;
            appointment.BookingDate = DateTime.UtcNow;

            appointment.BookingStatus = string.IsNullOrEmpty(appointmentDto.BookingStatus) ? "Pending" : appointmentDto.BookingStatus;
            appointment.PaymentStatus = string.IsNullOrEmpty(appointmentDto.PaymentStatus) ? "Pending" : appointmentDto.PaymentStatus;

            if (!_appointmentRepo.CreateAppointment(appointment))
            {
                ModelState.AddModelError("", "Something went wrong while saving the appointment");
                return StatusCode(500, ModelState);
            }

            var resultDto = _mapper.Map<AppointmentDto>(appointment);
            return CreatedAtRoute("GetAppointment", new { id = appointment.Id }, resultDto);
        }

        [HttpPut("updateStripe/{id:int}")]
        public IActionResult UpdateStripeDetails(int id, [FromBody] AppointmentDto appointmentDto)
        {
            if (appointmentDto == null || id != appointmentDto.Id)
                return BadRequest();

            if (!_appointmentRepo.UpdateStripePaymentDetails(id, appointmentDto.StripeSessionId!, appointmentDto.StripePaymentIntentId!))
            {
                return StatusCode(500, "Error updating Stripe details");
            }

            return NoContent();
        }

        [HttpPut("updateStatus/{id:int}")]
        public IActionResult UpdateStatus(int id, [FromBody] AppointmentDto appointmentDto)
        {
            if (appointmentDto == null || id != appointmentDto.Id)
                return BadRequest();

            if (!_appointmentRepo.UpdateStatus(id, appointmentDto.BookingStatus, appointmentDto.PaymentStatus))
            {
                return StatusCode(500, "Error updating status");
            }

            return NoContent();
        }

       
        // ==========================================
        [HttpPost("CancelBooking/{id:int}")]
        public async Task<IActionResult> CancelBooking(int id)
        {
            var appointment = _appointmentRepo.GetAppointment(id);
            if (appointment == null)
            {
                return NotFound(new { success = false, message = "Appointment record not found!" });
            }

            if (appointment.BookingStatus == "Cancelled")
            {
                return BadRequest(new { success = false, message = "This booking is already cancelled." });
            }

            string updatedPaymentStatus = "Refunded";

            // If payment was Approved and Stripe PaymentIntentId exists, trigger automatic refund via RefundService
            if (appointment.PaymentStatus == "Approved" && !string.IsNullOrEmpty(appointment.StripePaymentIntentId))
            {
                try
                {
                    var options = new RefundCreateOptions
                    {
                        PaymentIntent = appointment.StripePaymentIntentId
                    };
                    var service = new RefundService();
                    await service.CreateAsync(options);
                    updatedPaymentStatus = "Refunded";
                }
                catch (StripeException ex)
                {
                    return BadRequest(new { success = false, message = $"Stripe Refund Error: {ex.Message}" });
                }
            }

            // Update existing DB status using repository: BookingStatus = "Cancelled", PaymentStatus = "Refunded"
            if (!_appointmentRepo.UpdateStatus(id, "Cancelled", updatedPaymentStatus))
            {
                return StatusCode(500, new { success = false, message = "Database status update failed." });
            }

            return Ok(new { success = true, message = "Booking successfully cancelled and refund initiated!" });
        }
    }
}