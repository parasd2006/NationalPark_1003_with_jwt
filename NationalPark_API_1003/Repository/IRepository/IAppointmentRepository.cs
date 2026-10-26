using NationalPark_API_1003.Models;

namespace NationalPark_API_1003.Repository.IRepository
{
    public interface IAppointmentRepository
    {
        ICollection<Appointment> GetAppointments();
        ICollection<string> GetFullyBookedDates(int nationalParkId);

        int GetTotalPersonsBookedByDate(int nationalParkId, DateTime visitDate);

        Appointment GetAppointment(int appointmentId);
        bool AppointmentExists(int appointmentId);
        bool CreateAppointment(Appointment appointment);
        bool UpdateAppointment(Appointment appointment);
        bool DeleteAppointment(Appointment appointment);
        bool Save();

        ICollection<Appointment> GetAppointmentsByNationalPark(int nationalParkId);

        bool UpdateStripePaymentDetails(int appointmentId, string sessionId, string paymentIntentId);
        bool UpdateStatus(int appointmentId, string bookingStatus, string paymentStatus);
    }
}