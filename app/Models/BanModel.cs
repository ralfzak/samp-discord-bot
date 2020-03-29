namespace app.Models
{
    class BanModel
    {
        public ulong UId;
        public string Name;
        public ulong ByUId;
        public string ByName;
        public long ExpiresOn;
        public string Expired;
        public string BannedOn;
        public string Reason;
    }
}
