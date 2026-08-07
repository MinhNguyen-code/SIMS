using DayNeCu3726.Models.Entities;
using DayNeCu3726.Models.Enums;
using DayNeCu3726.Models.ViewModels;
using DayNeCu3726.Repositories.Interfaces;
using DayNeCu3726.Services.Interfaces;

namespace DayNeCu3726.Services
{
    public class FacultyService : IFacultyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthService _authService;

        public FacultyService(IUnitOfWork unitOfWork, IAuthService authService)
        {
            _unitOfWork = unitOfWork;
            _authService = authService;
        }

        public IEnumerable<Faculty> GetAllFaculties()
        {
            return _unitOfWork.Users.GetByRole(UserRole.Faculty).Cast<Faculty>();
        }

        public FacultyListViewModel GetFacultiesPaged(int page, int pageSize, string? search)
        {
            var query = _unitOfWork.Users.GetByRole(UserRole.Faculty).Cast<Faculty>();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                query = query.Where(f => 
                    f.FullName.ToLower().Contains(s) || 
                    f.Email.ToLower().Contains(s) ||
                    f.FacultyCode.ToLower().Contains(s) ||
                    f.Department.ToLower().Contains(s)
                );
            }

            var totalCount = query.Count();
            var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new FacultyListViewModel
            {
                Faculties = items.Select(MapToViewModel),
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                HasPreviousPage = page > 1,
                HasNextPage = page < totalPages
            };
        }

        public Faculty? GetFacultyById(string id)
        {
            var user = _unitOfWork.Users.GetById(id);
            return user as Faculty;
        }

        public (bool Success, string Message, Faculty? Faculty) CreateFaculty(FacultyViewModel model)
        {
            if (_unitOfWork.Users.EmailExists(model.Email))
            {
                return (false, "Email is already registered.", null);
            }

            var faculty = new Faculty
            {
                FullName = model.FullName,
                Email = model.Email,
                Department = model.Department,
                Position = model.Position,
                Specialization = model.Specialization,
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                IsActive = model.IsActive,
                PasswordHash = _authService.HashPassword("Faculty@123"), // Default password
                FacultyCode = string.IsNullOrWhiteSpace(model.FacultyCode) 
                    ? GenerateFacultyCode() 
                    : model.FacultyCode
            };

            try
            {
                _unitOfWork.Users.Add(faculty);
                _unitOfWork.SaveChanges();
                return (true, "Faculty created successfully.", faculty);
            }
            catch (Exception ex)
            {
                return (false, $"Error creating faculty: {ex.Message}", null);
            }
        }

        public (bool Success, string Message) UpdateFaculty(string id, FacultyViewModel model)
        {
            var user = _unitOfWork.Users.GetById(id);
            if (user == null || user.Role != UserRole.Faculty)
            {
                return (false, "Faculty not found.");
            }

            if (user.Email != model.Email && _unitOfWork.Users.EmailExists(model.Email))
            {
                return (false, "Email is already in use by another user.");
            }

            var faculty = (Faculty)user;
            faculty.FullName = model.FullName;
            faculty.Email = model.Email;
            faculty.Department = model.Department;
            faculty.Position = model.Position;
            faculty.Specialization = model.Specialization;
            faculty.PhoneNumber = model.PhoneNumber;
            faculty.Address = model.Address;
            faculty.IsActive = model.IsActive;

            try
            {
                _unitOfWork.Users.Update(faculty);
                _unitOfWork.SaveChanges();
                return (true, "Faculty updated successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating faculty: {ex.Message}");
            }
        }

        public (bool Success, string Message) DeleteFaculty(string id)
        {
            var user = _unitOfWork.Users.GetById(id);
            if (user == null || user.Role != UserRole.Faculty)
            {
                return (false, "Faculty not found.");
            }

            var faculty = (Faculty)user;
            if (faculty.TeachingCourses.Any())
            {
                return (false, "Cannot delete faculty because they are assigned to courses.");
            }

            try
            {
                _unitOfWork.Users.Delete(id);
                _unitOfWork.SaveChanges();
                return (true, "Faculty deleted successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting faculty: {ex.Message}");
            }
        }

        private string GenerateFacultyCode()
        {
            return $"FAC{DateTime.Now.Year}{new Random().Next(100, 999)}";
        }

        private FacultyViewModel MapToViewModel(Faculty f)
        {
            return new FacultyViewModel
            {
                Id = f.Id,
                FullName = f.FullName,
                Email = f.Email,
                FacultyCode = f.FacultyCode,
                Department = f.Department,
                Position = f.Position,
                Specialization = f.Specialization,
                PhoneNumber = f.PhoneNumber,
                Address = f.Address,
                IsActive = f.IsActive
            };
        }
    }
}
