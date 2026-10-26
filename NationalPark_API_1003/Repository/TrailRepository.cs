using Microsoft.EntityFrameworkCore;
using NationalPark_API_1003.Data;
using NationalPark_API_1003.Models;
using NationalPark_API_1003.Repository.IRepository;

namespace NationalPark_API_1003.Repository
{
    
   
    public class TrailRepository : ITrailRepository
    {
        private readonly ApplicationDbContext _context;
        public TrailRepository(ApplicationDbContext context)
        {
            _context= context;
        }
        public bool CreateTrail(Trail trail)
        {
            _context.Trails.Add(trail);
            return Save();

        }

        public bool DeleteTrail(Trail trail)
        {
            _context.Trails.Remove(trail);
            return Save();

        }

        public Trail GetTrail(int trailId)
        {
            return _context.Trails.Include(n => n.NationalPark).
                FirstOrDefault(t => t.Id == trailId);

        }

        public ICollection<Trail> GetTrails()
        {
            return _context.Trails.Include(n => n.NationalPark).ToList();

        }

        public ICollection<Trail> GetTrailsNationalPark(int nationalParkId)
        {
            return _context.Trails.Include(n => n.NationalPark).Where(t => t.NationalParkId == nationalParkId).ToList();

        }

        public bool Save()
        {
            return _context.SaveChanges() >= 0 ? true : false;

        }

        public bool TrailExists(int trailId)
        {
            return _context.Trails.Any(x => x.Id == trailId);
        }

        public bool TrailExists(string trailName)
        {
            return _context.Trails.Any(x => x.Name == trailName);
        }

        public bool UpdateTrail(Trail trail)
        {
            _context.Trails.Update(trail);
            return Save();
        }
    }
}
