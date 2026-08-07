using DayNeCu3726.Models.Entities;
using DayNeCu3726.Models.Enums;
using DayNeCu3726.Models.ViewModels;
using DayNeCu3726.Repositories.Interfaces;
using DayNeCu3726.Services.Interfaces;

namespace DayNeCu3726.Services
{
    public class AnnouncementService : IAnnouncementService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AnnouncementService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<AnnouncementViewModel> GetAllAnnouncements()
        {
            var announcements = _unitOfWork.Announcements.GetAll()
                .OrderByDescending(a => a.IsPinned)
                .ThenByDescending(a => a.CreatedAt);

            return announcements.Select(a => new AnnouncementViewModel
            {
                AnnouncementId = a.AnnouncementId,
                Title = a.Title,
                Content = a.Content,
                AuthorId = a.AuthorId,
                AuthorName = a.AuthorName,
                CourseId = a.CourseId,
                CourseCode = a.Course?.CourseCode,
                CourseName = a.Course?.Name,
                Scope = a.Scope,
                IsPinned = a.IsPinned,
                CreatedAt = a.CreatedAt
            });
        }

        public IEnumerable<AnnouncementViewModel> GetAnnouncementsByCourse(string courseId)
        {
            var announcements = _unitOfWork.Announcements.GetByCourse(courseId);
            return announcements.Select(a => new AnnouncementViewModel
            {
                AnnouncementId = a.AnnouncementId,
                Title = a.Title,
                Content = a.Content,
                AuthorId = a.AuthorId,
                AuthorName = a.AuthorName,
                CourseId = a.CourseId,
                CourseCode = a.Course?.CourseCode,
                CourseName = a.Course?.Name,
                Scope = a.Scope,
                IsPinned = a.IsPinned,
                CreatedAt = a.CreatedAt
            });
        }

        public AnnouncementViewModel? GetAnnouncementById(string id)
        {
            var a = _unitOfWork.Announcements.GetById(id);
            if (a == null) return null;

            return new AnnouncementViewModel
            {
                AnnouncementId = a.AnnouncementId,
                Title = a.Title,
                Content = a.Content,
                AuthorId = a.AuthorId,
                AuthorName = a.AuthorName,
                CourseId = a.CourseId,
                CourseCode = a.Course?.CourseCode,
                CourseName = a.Course?.Name,
                Scope = a.Scope,
                IsPinned = a.IsPinned,
                CreatedAt = a.CreatedAt
            };
        }

        public (bool success, string message) CreateAnnouncement(string authorId, string authorName, CreateAnnouncementViewModel model)
        {
            try
            {
                var announcement = new Announcement
                {
                    AnnouncementId = Guid.NewGuid().ToString(),
                    Title = model.Title,
                    Content = model.Content,
                    AuthorId = authorId,
                    AuthorName = authorName,
                    CourseId = model.Scope == AnnouncementScope.Course ? model.CourseId : null,
                    Scope = model.Scope,
                    IsPinned = model.IsPinned,
                    CreatedAt = DateTime.UtcNow
                };

                _unitOfWork.Announcements.Add(announcement);
                _unitOfWork.SaveChanges();

                return (true, "Announcement created successfully!");
            }
            catch (Exception ex)
            {
                return (false, "Error creating announcement: " + ex.Message);
            }
        }

        public (bool success, string message) DeleteAnnouncement(string id)
        {
            try
            {
                var announcement = _unitOfWork.Announcements.GetById(id);
                if (announcement == null)
                    return (false, "Announcement not found.");

                _unitOfWork.Announcements.Delete(id);
                _unitOfWork.SaveChanges();

                return (true, "Announcement deleted successfully!");
            }
            catch (Exception ex)
            {
                return (false, "Error deleting announcement: " + ex.Message);
            }
        }
    }
}
