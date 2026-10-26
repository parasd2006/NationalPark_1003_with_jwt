using Microsoft.AspNetCore.Mvc;
using NationalParkWebApp_1003.Models;
using NationalParkWebApp_1003.Repository.IRepository;
using NationalParkWebApp_1003.Utility;
using System.Collections.Concurrent;
using NationalParkWebApp_1003.Models.ViewModel;
namespace NationalParkWebApp_1003.Controllers
{
    public class BookingCancelController : Controller
    {
        private readonly IunitOfWorkRepository _unitOfWork;
        private readonly NotificationSender _notificationSender;
        private readonly IHttpClientFactory _clientFactory;

        private static readonly ConcurrentDictionary<string, (string Otp, DateTime Expiry)> OtpStore = new();

        public BookingCancelController(
            IunitOfWorkRepository unitOfWork,
            NotificationSender notificationSender,
            IHttpClientFactory clientFactory)
        {
            _unitOfWork = unitOfWork;
            _notificationSender = notificationSender;
            _clientFactory = clientFactory;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendCancelOtp([FromBody] SendOtpRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.PhoneNumber))
                return Json(new { success = false, message = "Please enter a valid phone number." });

            var cleanPhone = request.PhoneNumber.Trim().Replace(" ", "").Replace("+91", "").Replace("-", "");

            if (cleanPhone.Length < 10)
                return Json(new { success = false, message = "Please enter a valid 10-digit phone number." });

            var allBookings = await _unitOfWork.Appointment.GetAllAsync(SD.AppointmentAPIPath);

            var userBookings = allBookings?
                .Where(a => a.CustomerPhone != null && a.CustomerPhone.Replace(" ", "").Replace("+91", "").
                Replace("-", "").Contains(cleanPhone))
                .ToList();

            if (userBookings == null || !userBookings.Any())
                return Json(new { success = false, message = "No bookings found for this phone number!" });

            var targetBooking = userBookings.LastOrDefault();
            var customerName = targetBooking?.CustomerName ?? "Customer";

            var otp = new Random().Next(100000, 999999).ToString();
            OtpStore[cleanPhone] = (otp, DateTime.UtcNow.AddMinutes(5));

            try
            {
                _notificationSender.SendCancellationOtpSms(cleanPhone, customerName, otp);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Failed to send OTP SMS: {ex.Message}" });
            }

            return Json(new
            {
                success = true,
                message = $"OTP has been sent to your phone number (+91 {cleanPhone})."
            });
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOtpAndGetBookings([FromBody] VerifyOtpRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.PhoneNumber) || string.IsNullOrWhiteSpace(request.Otp))
                return Json(new { success = false, message = "Invalid verification request." });

            var cleanPhone = request.PhoneNumber.Trim().Replace(" ", "").Replace("+91", "").Replace("-", "");

            if (!OtpStore.TryGetValue(cleanPhone, out var record))
                return Json(new { success = false, message = "OTP record not found or expired. Please request a new OTP." });

            if (record.Expiry < DateTime.UtcNow)
            {
                OtpStore.TryRemove(cleanPhone, out _);
                return Json(new { success = false, message = "OTP has expired! Please request a new OTP." });
            }

            if (record.Otp != request.Otp.Trim())
            {
                return Json(new { success = false, message = "Invalid OTP entered! Please check your SMS." });
            }

            OtpStore.TryRemove(cleanPhone, out _);

            var allBookings = await _unitOfWork.Appointment.GetAllAsync(SD.AppointmentAPIPath);

            var userBookings = allBookings?
                .Where(a => a.CustomerPhone != null && a.CustomerPhone.Replace(" ", "").Replace("+91", "").
                Replace("-", "").Contains(cleanPhone))
                .OrderByDescending(b => b.Id)
                .Select(b => new {
                    id = b.Id,
                    customerName = b.CustomerName,
                    customerPhone = b.CustomerPhone,
                    parkName = b.NationalPark != null ? b.NationalPark.Name : "N/A",
                    visitDate = b.VisitDate.ToString("dd-MM-yyyy"),
                    amount = b.TotalAmount,
                    bookingStatus = b.BookingStatus,
                    paymentStatus = b.PaymentStatus
                })
                .ToList();

            return Json(new { success = true, bookings = userBookings });
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmCancel(int id)
        {
            var appointment = await _unitOfWork.Appointment.GetAsync(SD.AppointmentAPIPath, id);
            if (appointment == null)
                return Json(new { success = false, message = "Booking record not found!" });

            if (appointment.BookingStatus == SD.BookingStatusCancelled)
                return Json(new { success = false, message = "This booking is already cancelled." });

            var client = _clientFactory.CreateClient();
            var apiUrl = $"{SD.AppointmentAPIPath.TrimEnd('/')}/CancelBooking/{id}";
            var response = await client.PostAsync(apiUrl, null);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                return Json(new { success = false, message = "Cancellation/Refund process failed: " + errorBody });
            }

            try
            {
                string parkName = appointment.NationalPark?.Name;
                if (string.IsNullOrWhiteSpace(parkName))
                {
                    var park = await _unitOfWork.NationalPark.GetAsync(SD.NationalParkAPIPath, appointment.NationalParkId);
                    parkName = park?.Name ?? "National Park";
                }

                _notificationSender.SendCancellationSms(
                    appointment.CustomerPhone,
                    appointment.CustomerName,
                    appointment.Id,
                    parkName
                );
            }
            catch (Exception ex)
            {
            }

            return Json(new { success = true, message = $"Booking #{id} has been cancelled successfully!" });
        }

            }

   
}