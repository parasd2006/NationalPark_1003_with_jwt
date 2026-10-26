using Microsoft.EntityFrameworkCore;
using NationalPark_API_1003.Data;
using NationalPark_API_1003.Models;
using NationalPark_API_1003.Repository.IRepository;

namespace NationalPark_API_1003.Repository
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly ApplicationDbContext _context;

        public AppointmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public ICollection<Appointment> GetAppointments()
        {
            return _context.Appointments
                .Include(a => a.NationalPark)
                .Include(a => a.Trail)
                .OrderByDescending(a => a.BookingDate)
                .ToList();
        }

        public Appointment GetAppointment(int appointmentId)
        {
            return _context.Appointments
                .Include(a => a.NationalPark)
                .Include(a => a.Trail)
                .FirstOrDefault(a => a.Id == appointmentId);
        }

        public bool AppointmentExists(int appointmentId)
        {
            return _context.Appointments.Any(a => a.Id == appointmentId);
        }

        public bool CreateAppointment(Appointment appointment)
        {
            _context.Appointments.Add(appointment);
            return Save();
        }

        public bool UpdateAppointment(Appointment appointment)
        {
            _context.Appointments.Update(appointment);
            return Save();
        }

        public bool DeleteAppointment(Appointment appointment)
        {
            _context.Appointments.Remove(appointment);
            return Save();
        }

        public bool Save()
        {
            return _context.SaveChanges() >= 0;
        }

        public ICollection<Appointment> GetAppointmentsByNationalPark(int nationalParkId)
        {
            return _context.Appointments
                .Include(a => a.NationalPark)
                .Where(a => a.NationalParkId == nationalParkId)
                .ToList();
        }

        public bool UpdateStripePaymentDetails(int appointmentId, string sessionId, string paymentIntentId)
        {
            var appointment = _context.Appointments.FirstOrDefault(a => a.Id == appointmentId);
            if (appointment != null)
            {
                if (!string.IsNullOrEmpty(sessionId))
                {
                    appointment.StripeSessionId = sessionId;
                }
                if (!string.IsNullOrEmpty(paymentIntentId))
                {
                    appointment.StripePaymentIntentId = paymentIntentId;
                    appointment.PaymentDate = DateTime.UtcNow;
                }
                return Save();
            }
            return false;
        }

        public bool UpdateStatus(int appointmentId, string bookingStatus, string paymentStatus)
        {
            var appointment = _context.Appointments.FirstOrDefault(a => a.Id == appointmentId);
            if (appointment != null)
            {
                if (!string.IsNullOrEmpty(bookingStatus))
                {
                    appointment.BookingStatus = bookingStatus;
                }
                if (!string.IsNullOrEmpty(paymentStatus))
                {
                    appointment.PaymentStatus = paymentStatus;
                }
                return Save();
            }
            return false;
        }

        public ICollection<string> GetFullyBookedDates(int nationalParkId)
        {
            return _context.Appointments
                 .Where(a => a.NationalParkId == nationalParkId && a.VisitDate.Date >= DateTime.Today)
                 .GroupBy(a => a.VisitDate.Date)
                 .Where(g => g.Sum(x => x.NumberOfAdults + x.NumberOfChildren) >= 100)
                 .Select(g => g.Key.ToString("yyyy-MM-dd"))
                 .ToList();
        }

        public int GetTotalPersonsBookedByDate(int nationalParkId, DateTime visitDate)
        {
            return _context.Appointments
                    .Where(a => a.NationalParkId == nationalParkId && a.VisitDate.Date == visitDate.Date)
                    .Sum(a => a.NumberOfAdults + a.NumberOfChildren);
        }
    }
}