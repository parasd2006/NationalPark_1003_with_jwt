namespace NationalParkWebApp_1003
{
    public static class SD
    {
        // API Endpoints
        public static string ApiBaseURL = "https://localhost:7218/";
        public static string NationalParkAPIPath = ApiBaseURL + "api/NationalPark";
        public static string TrailAPIPath = ApiBaseURL + "api/Trail";
        public static string AppointmentAPIPath = ApiBaseURL + "api/appointments";

        // Booking Status
        public const string BookingStatusPending = "Pending";
        public const string BookingStatusApproved = "Approved";
        public const string BookingStatusCancelled = "Cancelled";
        public const string BookingStatusCompleted = "Completed";

        // Payment Status
        public const string PaymentStatusPending = "Pending";
        public const string PaymentStatusApproved = "Approved";
        public const string PaymentStatusRejected = "Rejected";
        public const string PaymentStatusRefunded = "Refunded";
    }
}