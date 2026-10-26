using NationalPark_API_1003.Models;

namespace NationalPark_API_1003.Repository.IRepository
{
    public interface IUserRepository
    {
        bool IsUniqueUser(string username);
        User Authenticate(string username, string password);
         User Register(string username, string password);
    }
}
