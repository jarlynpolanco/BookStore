using BookStore_UI.Models;
using System.Threading.Tasks;

namespace BookStore_UI.Contracts
{
    public interface IAuthenticationRepository
    {
        public Task<bool> Register(RegistrationModel user);
    }
}
