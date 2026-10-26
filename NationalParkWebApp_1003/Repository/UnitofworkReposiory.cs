using NationalParkWebApp_1003.Repository.IRepository;

namespace NationalParkWebApp_1003.Repository
{
    public class UnitofworkReposiory : IunitOfWorkRepository
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public UnitofworkReposiory(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory= httpClientFactory;
            NationalPark = new NationalParkRepository(httpClientFactory);
            Trail = new TrailRepository(httpClientFactory);
            Appointment = new AppointmentRepository(httpClientFactory);
        }
        public INationalParkRepository NationalPark { private set;  get;  }
        public ITrailRepository Trail { private set; get;  }

        public IAppointmentRepository Appointment { private set; get; }
    }
}
