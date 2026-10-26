using NationalPark_API_1003.Models;

namespace NationalPark_API_1003.Repository.IRepository
{
    public interface ITrailRepository
    {
        ICollection<Trail> GetTrails();
        Trail GetTrail(int trailId);
        bool TrailExists(int trailId);
        bool TrailExists(string trailName);
        bool CreateTrail(Trail trail);
        bool UpdateTrail(Trail trail);
        bool DeleteTrail(Trail trail);
        bool Save();
        ICollection<Trail> GetTrailsNationalPark(int nationalParkId);
    }
}
