using System;

namespace main.Core.Domain.Models
{
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
