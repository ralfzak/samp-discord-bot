using System;

namespace main.Core.Domain.Models
{
    /// <summary>
    /// Responsible for carrying single verification related data.
    /// </summary>
    public class Verifications
    {
        public ulong Userid { get; set; }
        public int? ForumId { get; set; }
        public string ForumName { get; set; }
        public string VerifiedBy { get; set; }
        public DateTime VerifiedOn { get; set; }
        public DateTimeOffset? DeletedOn { get; set; }
    }
}
