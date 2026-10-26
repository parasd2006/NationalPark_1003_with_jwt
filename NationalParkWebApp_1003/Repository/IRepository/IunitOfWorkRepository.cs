namespace NationalParkWebApp_1003.Repository.IRepository
{
    public interface IunitOfWorkRepository
    {
        INationalParkRepository NationalPark { get; }
        ITrailRepository Trail { get; }
        IAppointmentRepository Appointment { get; }
    }
}
