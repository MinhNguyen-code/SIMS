using System.Collections.Generic;
using DayNeCu3726.Models.Entities;
using DayNeCu3726.Models.ViewModels;

namespace DayNeCu3726.Services.Interfaces
{
    public interface IAssignmentService
    {
        IEnumerable<AssignmentViewModel> GetAssignmentsForStudent(string studentId);
        IEnumerable<AssignmentViewModel> GetAssignmentsForFaculty(string facultyId);
        IEnumerable<AssignmentViewModel> GetAllAssignments();
        
        Assignment? GetAssignmentById(string id);
        
        void CreateAssignment(CreateAssignmentViewModel model);
        
        // Student side
        void SubmitAssignment(string assignmentId, string studentId, string filePath, string originalFileName);
        
        // Faculty side
        IEnumerable<SubmissionViewModel> GetSubmissionsForAssignment(string assignmentId);
        SubmissionViewModel? GetSubmissionById(string submissionId);
        void GradeSubmission(string submissionId, double grade, string feedback);
    }
}
