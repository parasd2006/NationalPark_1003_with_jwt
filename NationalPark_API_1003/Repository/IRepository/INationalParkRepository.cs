using NationalPark_API_1003.Models;

namespace NationalPark_API_1003.Repository.IRepository
{
    public interface INationalParkRepository
    {
        ICollection<NationalPark> GetNationalParks();//Mujhe saare National Parks do. getall
        NationalPark GetNationalPark(int nationalParkId);//find Ye single park find karne ke liye hai.
        bool NationalParkExists(int nationalParkId);//Kya is ID ka National Park database mein exist karta hai?
        bool NationalParkExists(string nationalParkName);//Ye same kaam naam se karta hai:
        bool CreateNationalPark(NationalPark nationalPark);//Database mein new NationalPark add karna.
        bool UpdateNationalPark(NationalPark nationalPark);
        bool DeleteNationalPark(NationalPark nationalPark);
        bool Save();
    }
}
