namespace main.Core.Domain.Models
{
    /// <summary>
    /// Responsible for carrying single SAMP server related data.
    /// </summary>
    public class SampServerData
    {
        public string Ip { get; set; }
        public ushort Port { get; set; }
        public string Hostname { get; set; }
        public string Gamemode { get; set; }
        public int MaxPlayers { get; set; }
        public int CurrentPlayers { get; set; }
        public string Version { get; set; }
        public string Url { get; set; }
        public bool IsHostedTab { get; set; }
    }
}
