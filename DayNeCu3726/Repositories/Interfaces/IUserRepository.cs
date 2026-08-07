using DayNeCu3726.Models.Entities;
using DayNeCu3726.Models.Enums;

namespace DayNeCu3726.Repositories.Interfaces
{
    /// <summary>
    /// User-specific repository interface (for authentication).
    /// </summary>
    public interface IUserRepository
    {
        User? GetById(string id);
        User? GetByEmail(string email);
        IEnumerable<User> GetAll();
        IEnumerable<User> GetByRole(UserRole role);
        void Add(User user);
        void Update(User user);
        void Delete(string id);
        bool EmailExists(string email);
    }
}
