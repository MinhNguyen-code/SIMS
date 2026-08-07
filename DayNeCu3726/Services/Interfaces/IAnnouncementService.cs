using DayNeCu3726.Models.ViewModels;

namespace DayNeCu3726.Services.Interfaces
{
    public interface IAnnouncementService
    {
        IEnumerable<AnnouncementViewModel> GetAllAnnouncements();
        IEnumerable<AnnouncementViewModel> GetAnnouncementsByCourse(string courseId);
        AnnouncementViewModel? GetAnnouncementById(string id);
        (bool success, string message) CreateAnnouncement(string authorId, string authorName, CreateAnnouncementViewModel model);
        (bool success, string message) DeleteAnnouncement(string id);
    }
}
