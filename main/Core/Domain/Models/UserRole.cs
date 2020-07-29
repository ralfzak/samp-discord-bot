using System;

namespace main.Core.Domain.Models
{
    /// <summary>
    /// The definition of a discord role assignment.
    /// </summary>
    public class UserRole
    {
        public ulong UserId { get; set; }
        public ulong RoleId  { get; set; }
        public ulong AssignedBy { get; set; }
        public DateTime AssignedOn { get; set; }
    }
}