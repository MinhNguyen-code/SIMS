using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using DayNeCu3726.Infrastructure;
using DayNeCu3726.Models.Entities;
using DayNeCu3726.Models.Enums;
using DayNeCu3726.Repositories.Interfaces;

namespace DayNeCu3726.Repositories.EF
{
    /// <summary>
    /// Entity Framework implementation of IAnnouncementRepository
    /// </summary>
    public class EFAnnouncementRepository : EFRepository<Announcement>, IAnnouncementRepository
    {
        public EFAnnouncementRepository(AppDbContext context) : base(context)
        {
        }

        public override IEnumerable<Announcement> GetAll()
        {
            return _dbSet.Include(a => a.Course).OrderByDescending(a => a.CreatedAt).ToList();
        }

        public IEnumerable<Announcement> GetByCourse(string courseId)
        {
            return _dbSet.Include(a => a.Course).Where(a => a.CourseId == courseId).ToList();
        }

        public IEnumerable<Announcement> GetByScope(AnnouncementScope scope)
        {
            return _dbSet.Where(a => a.Scope == scope).ToList();
        }

        public IEnumerable<Announcement> GetByAuthor(string authorId)
        {
            return _dbSet.Where(a => a.AuthorId == authorId).ToList();
        }

        public IEnumerable<Announcement> GetPinnedAnnouncements()
        {
            return _dbSet.Where(a => a.IsPinned).ToList();
        }

        public IEnumerable<Announcement> GetRecent(int count = 10)
        {
            return _dbSet.OrderByDescending(a => a.CreatedAt).Take(count).ToList();
        }
    }
}
