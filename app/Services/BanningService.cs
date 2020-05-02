using System;
using System.Collections.Generic;
using app.Core;
using app.Models;

namespace app.Services
{
    public class BanningService
    {
        public void StoreBan(ulong uid, string name, ulong byuid, string byname, int secondsadd, string reason, int timedban)
        {
            DataService.Put(
                "INSERT INTO `bans` (`uid`, `name`, `byuid`, `byname`, `expires_on`, `expired`, `reason`) VALUES (@userid, @username, @byuserid, @byusername, ((UNIX_TIMESTAMP(NOW()) + @secondsadd) * @timedban), 'N', @reasonban)", 
                new Dictionary<string, object>()
                {
                    { "@userid", uid },
                    { "@username", name },
                    { "@byuserid", byuid },
                    { "@byusername", byname },
                    { "@secondsadd", secondsadd },
                    { "@reasonban", reason },

                    { "@timedban", timedban }
                });

            Logger.Write($"[StoreBan] {uid} {name} {byuid} {byname} {secondsadd} {reason} {timedban}");
        }

        public void RemoveBan(ulong uid)
        {
            DataService.Update("UPDATE `bans` SET `expired` = 'Y' WHERE `uid`=@userid", new Dictionary<string, object>()
            {
                {"@userid", uid}
            });

            Logger.Write($"[RemoveBan] {uid}");
        }

        public List<BanModel> GetBans(string search)
        {
            List<BanModel> bans = new List<BanModel>();
            string nameUidSearch = "`name`=@search";
            if (Int64.TryParse(search, out long v))
            {
                nameUidSearch = "`uid`=@search";
            }

            var data = DataService.Get(
                $"SELECT `uid`, `name`, `byuid`, `byname`, `expires_on`, DATE_FORMAT(banned_on, '%D %M %Y') `banned_on`, `reason` FROM `bans` WHERE {nameUidSearch} AND `expired` = 'N'", 
                new Dictionary<string, object>()
                {
                    {"@search", search}
                });

            if (data.Count > 0)
            {
                for (int i = 0; i != data["uid"].Count; i++)
                {
                    bans.Add(new BanModel
                    {
                        UId = (ulong)(long)data["uid"][i],
                        Name = (string)data["name"][i],
                        ByUId = (ulong)(long)data["byuid"][i],
                        ByName = (string)data["byname"][i],
                        ExpiresOn = (long)data["expires_on"][i],
                        BannedOn = (string)data["banned_on"][i],
                        Reason = (string)data["reason"][i],
                        Expired = "N"
                    });
                }
            }

            return bans;
        }

        public List<BanModel> GetExpiredBans()
        {
            List<BanModel> bans = new List<BanModel>();
            var data = DataService.Get("SELECT `uid`, `name`, `byuid`, `byname`, `expires_on`, DATE_FORMAT(banned_on, '%D %M %Y') `banned_on`, `reason` FROM `bans` WHERE `expires_on` < UNIX_TIMESTAMP(NOW()) AND `expires_on` != 0 AND `expired` = 'N'", null);
            if (data.Count > 0)
            {
                for (int i = 0; i != data["uid"].Count; i++)
                {
                    bans.Add(new BanModel
                    {
                        UId = (ulong)(long)data["uid"][i],
                        Name = (string)data["name"][i],
                        ByUId = (ulong)(long)data["byuid"][i],
                        ByName = (string)data["byname"][i],
                        ExpiresOn =(long)data["expires_on"][i],
                        BannedOn = (string)data["banned_on"][i],
                        Reason = (string)data["reason"][i],
                        Expired = "N"
                    });
                }
            }

            return bans;
        }
    }
}
