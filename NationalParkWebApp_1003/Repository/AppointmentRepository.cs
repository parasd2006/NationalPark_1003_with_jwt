using NationalParkWebApp_1003.Models;
using NationalParkWebApp_1003.Repository.IRepository;

namespace NationalParkWebApp_1003.Repository
{
    public class AppointmentRepository : Repository<Appointment>, IAppointmentRepository
    {
        private readonly IHttpClientFactory _clientFactory;

        public AppointmentRepository(IHttpClientFactory clientFactory) : base(clientFactory)
        {
            _clientFactory = clientFactory;
        }
    }
}