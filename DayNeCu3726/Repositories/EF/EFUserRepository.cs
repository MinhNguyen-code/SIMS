using Microsoft.EntityFrameworkCore;
using DayNeCu3726.Infrastructure;
using DayNeCu3726.Models.Entities;
using DayNeCu3726.Models.Enums;
using DayNeCu3726.Repositories.Interfaces;

namespace DayNeCu3726.Repositories.EF
{
    public class EFUserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<User> _dbSet;

        public EFUserRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Users;
        }

        public User? GetById(string id)
        {
            return _dbSet.Find(id);
        }

        public User? GetByEmail(string email)
        {
            return _dbSet.FirstOrDefault(u => u.Email.ToLower() == email.ToLower());
        }

        public IEnumerable<User> GetAll()
        {
            return _dbSet.ToList();
        }

        public IEnumerable<User> GetByRole(UserRole role)
        {
            return _dbSet.Where(u => u.Role == role).ToList();
        }

        public void Add(User user)
        {
            _dbSet.Add(user);
        }

        public void Update(User user)
        {
            _dbSet.Update(user);
        }

        public void Delete(string id)
        {
            var user = GetById(id);
            if (user != null)
            {
                _dbSet.Remove(user);
            }
        }

        public bool EmailExists(string email)
        {
            return _dbSet.Any(u => u.Email.ToLower() == email.ToLower());
        }
    }
}
