using System.Collections.Generic;
using DayNeCu3726.Models.Entities;

namespace DayNeCu3726.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for Parent entity
    /// </summary>
    public interface IParentRepository : IRepository<Parent>
    {
        Parent? GetByParentCode(string parentCode);
        Parent? GetByEmail(string email);
        Parent? GetByStudentId(string studentId);
        IEnumerable<Parent> SearchByName(string name);
        string GenerateParentCode();
    }
}
