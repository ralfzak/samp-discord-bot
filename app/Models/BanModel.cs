using System;
using System.Collections.Generic;
using System.Text;

namespace app.Models
{
    class BanModel
    {
        public ulong uid;
        public string name;
        public ulong byuid;
        public string byname;
        public int expires_on;
        public string expired;
        public string banned_on;
        public string reason;
    }
}
