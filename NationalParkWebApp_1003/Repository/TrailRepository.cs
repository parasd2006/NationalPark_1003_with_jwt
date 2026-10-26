using NationalParkWebApp_1003.Models;
using NationalParkWebApp_1003.Repository.IRepository;

namespace NationalParkWebApp_1003.Repository
{
    public class TrailRepository : Repository<Trail>, ITrailRepository
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public TrailRepository(IHttpClientFactory httpClientFactory) : base(httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
    }
}
