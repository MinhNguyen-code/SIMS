using DayNeCu3726.Models.Entities;
using DayNeCu3726.Models.ViewModels;

namespace DayNeCu3726.Services.Interfaces
{
    public interface IFacultyService
    {
        IEnumerable<Faculty> GetAllFaculties();
        FacultyListViewModel GetFacultiesPaged(int page, int pageSize, string? search);
        Faculty? GetFacultyById(string id);
        (bool Success, string Message, Faculty? Faculty) CreateFaculty(FacultyViewModel model);
        (bool Success, string Message) UpdateFaculty(string id, FacultyViewModel model);
        (bool Success, string Message) DeleteFaculty(string id);
    }
}
