using System.Collections.Generic;
using DayNeCu3726.Models.Entities;
using DayNeCu3726.Models.Enums;

namespace DayNeCu3726.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for Announcement entity
    /// </summary>
    public interface IAnnouncementRepository : IRepository<Announcement>
    {
        IEnumerable<Announcement> GetByCourse(string courseId);
        IEnumerable<Announcement> GetByScope(AnnouncementScope scope);
        IEnumerable<Announcement> GetByAuthor(string authorId);
        IEnumerable<Announcement> GetPinnedAnnouncements();
        IEnumerable<Announcement> GetRecent(int count = 10);
    }
}
