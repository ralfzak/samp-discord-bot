using System;

namespace main.Core.Domain.Models
{
    public class Bans
    {
        public long Id { get; set; }
        public ulong Userid { get; set; }
        public string Name { get; set; }
        public ulong ByUserid { get; set; }
        public string ByName { get; set; }
        public DateTime? ExpiresOn { get; set; }
        public string Reason { get; set; }
        public DateTimeOffset BannedOn { get; set; }
        public short Lifted { get; set; }
    }
}
