using DayNeCu3726.Infrastructure;
using DayNeCu3726.Models.Entities;
using DayNeCu3726.Repositories.Interfaces;

namespace DayNeCu3726.Repositories.EF
{
    public class EFAssignmentRepository : EFRepository<Assignment>, IAssignmentRepository
    {
        public EFAssignmentRepository(AppDbContext context) : base(context)
        {
        }
    }
}
