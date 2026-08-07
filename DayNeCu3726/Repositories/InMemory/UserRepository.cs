using DayNeCu3726.Models.Entities;
using DayNeCu3726.Models.Enums;
using DayNeCu3726.Repositories.Interfaces;

namespace DayNeCu3726.Repositories.InMemory
{
    /// <summary>
    /// In-memory User repository for authentication purposes.
    /// Stores all user types (Admin, Faculty, Student) polymorphically.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private static readonly List<User> _store = new();

        public User? GetById(string id)
            => _store.FirstOrDefault(u => u.Id == id);

        public User? GetByEmail(string email)
            => _store.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

        public IEnumerable<User> GetAll()
            => _store.AsReadOnly();

        public IEnumerable<User> GetByRole(UserRole role)
            => _store.Where(u => u.Role == role);

        public void Add(User user)
            => _store.Add(user);

        public void Update(User user)
        {
            var index = _store.FindIndex(u => u.Id == user.Id);
            if (index >= 0)
                _store[index] = user;
        }

        public void Delete(string id)
        {
            var user = GetById(id);
            if (user != null)
                _store.Remove(user);
        }

        public bool EmailExists(string email)
            => _store.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    }
}
