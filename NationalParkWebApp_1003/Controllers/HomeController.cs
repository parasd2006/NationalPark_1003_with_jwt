using Microsoft.AspNetCore.Mvc;
using NationalParkWebApp_1003.Models;
using NationalParkWebApp_1003.Models.NationalParkVM;
using NationalParkWebApp_1003.Repository.IRepository;
using NationalParkWebApp_1003.Utility;
using Stripe;
using System.Diagnostics;

namespace NationalParkWebApp_1003.Controllers
{
    public class HomeController : Controller
    {
        private readonly IunitOfWorkRepository _unitofwork;
        private readonly IConfiguration _configuration;
        private readonly NotificationSender _notificationSender;

        public HomeController(IunitOfWorkRepository unitOfWork, IConfiguration configuration,
            NotificationSender notificationSender)
        {
            _unitofwork = unitOfWork;
            _configuration = configuration;
            _notificationSender = notificationSender;
        }

        public async Task<IActionResult> Index()
        {
            IndexVM indexVM = new IndexVM()
            {
                NationalParkList = await _unitofwork.NationalPark.GetAllAsync(SD.NationalParkAPIPath),
                TrailList = await _unitofwork.Trail.GetAllAsync(SD.TrailAPIPath)
            };
            return View("Index", indexVM);
        }

        [HttpGet]
        public async Task<IActionResult> BookAppointment(int nationalParkId)
        {
            var park = await _unitofwork.NationalPark.GetAsync(SD.NationalParkAPIPath, nationalParkId);
            if (park == null)
            {
                return NotFound();
            }

            var allTrails = await _unitofwork.Trail.GetAllAsync(SD.TrailAPIPath);
            var parkTrails = allTrails?.Where(t => t.NationalParkId == nationalParkId).ToList();

            ViewBag.TrailList = parkTrails?.Select(t => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Text = $"{t.Name} ({t.Difficulty})",
                Value = t.Id.ToString()
            }).ToList() ?? new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();

            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(SD.AppointmentAPIPath + "/fullyBookedDates/" + nationalParkId);
                ViewBag.FullyBookedDates = response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : "[]";
            }

            ViewBag.StripePublishableKey = _configuration.GetValue<string>("Stripe:PublishableKey");
            var appointment = new Appointment()
            {
                NationalParkId = nationalParkId,
                NationalPark = park,
                VisitDate = DateTime.Today.AddDays(1),
                NumberOfAdults = 1,
                NumberOfChildren = 0,
                TotalAmount = 100
            };

            return View(appointment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookAppointment(Appointment appointment, string stripeToken)
        {
            if ((appointment.NumberOfAdults + appointment.NumberOfChildren) > 5)
            {
                ModelState.AddModelError("", "Total persons (Adults + Children) cannot exceed 5.");
            }

            if (appointment.NumberOfAdults < 1)
            {
                ModelState.AddModelError("NumberOfAdults", "At least 1 adult is required.");
            }

            if (ModelState.IsValid)
            {
                appointment.TotalAmount = (appointment.NumberOfAdults * 100) + (appointment.NumberOfChildren * 50);
                appointment.BookingDate = DateTime.UtcNow;

                if (!string.IsNullOrEmpty(stripeToken))
                {
                    try
                    {
                        long amountInSmallestUnit = Convert.ToInt64(appointment.TotalAmount * 100);

                        var paymentMethodOptions = new PaymentMethodCreateOptions
                        {
                            Type = "card",
                            Card = new PaymentMethodCardOptions
                            {
                                Token = stripeToken
                            }
                        };
                        var paymentMethodService = new PaymentMethodService();
                        var paymentMethod = paymentMethodService.Create(paymentMethodOptions);

                        var paymentIntentOptions = new PaymentIntentCreateOptions
                        {
                            Amount = amountInSmallestUnit,
                            Currency = "inr",
                            Description = "National Park Appointment Booking",
                            PaymentMethod = paymentMethod.Id,
                            Confirm = true,
                            ReturnUrl = "https://localhost"
                        };

                        var paymentIntentService = new PaymentIntentService();
                        PaymentIntent intent = paymentIntentService.Create(paymentIntentOptions);

                        if (intent.Status.ToLower() == "succeeded")
                        {
                            appointment.StripePaymentIntentId = intent.Id;
                            appointment.BookingStatus = SD.BookingStatusApproved;
                            appointment.PaymentStatus = SD.PaymentStatusApproved;
                            appointment.PaymentDate = DateTime.UtcNow;

                            bool isCreated = await _unitofwork.Appointment.CreateAsync(SD.AppointmentAPIPath, appointment);
                            if (isCreated)
                            {
                                var park = await _unitofwork.NationalPark.GetAsync(SD.NationalParkAPIPath, appointment.NationalParkId);
                                string parkName = park?.Name ?? "National Park";
                                string visitDateFormatted = appointment.VisitDate.ToString("dd MMMM yyyy");

                                try
                                {
                                    await _notificationSender.SendBookingEmailAsync(
                                        appointment.CustomerEmail,
                                        appointment.CustomerName,
                                        parkName,
                                        visitDateFormatted,
                                        appointment.TotalAmount,
                                        intent.Id
                                    );
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine($"[EMAIL ERROR]: {ex.Message}");
                                }
                                try
                                {
                                    _notificationSender.SendBookingSms(
                                        appointment.CustomerPhone,
                                        appointment.CustomerName,
                                        parkName,
                                        visitDateFormatted
                                    );
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine($"[TWILIO SMS ERROR]: {ex.Message}");
                                    Console.WriteLine($"[TWILIO SMS ERROR]: {ex.Message}");
                                }

                                try
                                {
                                    _notificationSender.MakeBookingCall(
                                        appointment.CustomerPhone,
                                        appointment.CustomerName,
                                        parkName,
                                        visitDateFormatted
                                    );
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine($"[TWILIO CALL ERROR]: {ex.Message}");
                                    Console.WriteLine($"[TWILIO CALL ERROR]: {ex.Message}");
                                }

                                return RedirectToAction(nameof(BookingConfirmation), new
                                {
                                    nationalParkId = appointment.NationalParkId,
                                    paymentIntentId = intent.Id
                                });
                            }
                            else
                            {
                                ModelState.AddModelError("", "Payment successful, but failed to save booking to database.");
                            }
                        }
                        else
                        {
                            appointment.PaymentStatus = SD.PaymentStatusRejected;
                            ModelState.AddModelError("", "Payment was rejected by Stripe.");
                        }
                    }
                    catch (Exception ex)
                    {
                        appointment.PaymentStatus = SD.PaymentStatusRejected;
                        ModelState.AddModelError("", "Payment processing failed: " + ex.Message);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Payment token was not received from Stripe.");
                }
            }

            appointment.NationalPark = await _unitofwork.NationalPark.GetAsync(SD.NationalParkAPIPath, appointment.NationalParkId);

            var allTrails = await _unitofwork.Trail.GetAllAsync(SD.TrailAPIPath);
            var parkTrails = allTrails?.Where(t => t.NationalParkId == appointment.NationalParkId).ToList();
            ViewBag.TrailList = parkTrails?.Select(t => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Text = $"{t.Name} ({t.Difficulty})",
                Value = t.Id.ToString()
            }).ToList() ?? new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();

            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(SD.AppointmentAPIPath + "/fullyBookedDates/" + appointment.NationalParkId);
                ViewBag.FullyBookedDates = response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : "[]";
            }

            ViewBag.StripePublishableKey = _configuration.GetValue<string>("Stripe:PublishableKey");

            return View(appointment);
        }

        [HttpGet]
        public async Task<IActionResult> BookingConfirmation(int nationalParkId, string paymentIntentId)
        {
            var park = await _unitofwork.NationalPark.GetAsync(SD.NationalParkAPIPath, nationalParkId);

            ViewBag.NationalParkId = nationalParkId;
            ViewBag.ParkName = park?.Name;
            ViewBag.ParkState = park?.State;
            ViewBag.PaymentIntentId = paymentIntentId;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}