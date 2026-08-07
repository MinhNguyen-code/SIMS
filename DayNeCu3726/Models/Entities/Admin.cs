using DayNeCu3726.Models.Enums;

namespace DayNeCu3726.Models.Entities
{
    /// <summary>
    /// Admin entity. Inherits from User, represents system administrators.
    /// </summary>
    public class Admin : User
    {
        public string AdminCode { get; set; } = string.Empty;

        public Admin()
        {
            Role = UserRole.Admin;
        }
    }
}
