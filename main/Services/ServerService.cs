using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using main.Core;

namespace main.Services
{
    public static class ServerService
    {
        public static async Task<bool> CheckGameMpWebsite(string ip, int port)
        {
            string url = $"http://game-mp.com/";
            var result = false;

            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = client.GetAsync(url).Result)
                {
                    using (HttpContent content = response.Content)
                    {
                        try
                        {
                            string res = await content.ReadAsStringAsync();
                            result = res.Contains($"{ip}:{port}");
                        }
                        catch (Exception)
                        {
                            result = false;
                        }
                    }
                }
            }

            return result;
        }

        public static bool ValidateIPv4(string s)
        {
            return Uri.CheckHostName(s) == UriHostNameType.IPv4;
        }

        public static bool ValidateHostname(string s)
        {
            return Uri.CheckHostName(s) == UriHostNameType.Dns;
        }
    }

    public class ServerQueryService
    {
        private IPAddress serverIp;
        private IPEndPoint serverEndPoint;
        private Socket svrConnect;

        private string szIP;
        private ushort iPort;

        public ServerQueryService(string ip, ushort port, char packetType, int timeout = 500)
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);

            try
            {
                serverIp = new IPAddress(IPAddress.Parse(ip).GetAddressBytes());
                serverEndPoint = new IPEndPoint(serverIp, port);

                svrConnect = new Socket(serverEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);

                svrConnect.SendTimeout = timeout;
                svrConnect.ReceiveTimeout = timeout;
                szIP = ip;
                iPort = port;

                Logger.Write("[SAMPServerQueryService] Connecting to " + ip + ":" + port);

                try
                {
                    using (stream)
                    {
                        using (writer)
                        {
                            string[] szSplitIP = szIP.ToString().Split('.');

                            writer.Write("SAMP".ToCharArray());

                            writer.Write(Convert.ToByte(Convert.ToInt16(szSplitIP[0])));
                            writer.Write(Convert.ToByte(Convert.ToInt16(szSplitIP[1])));
                            writer.Write(Convert.ToByte(Convert.ToInt16(szSplitIP[2])));
                            writer.Write(Convert.ToByte(Convert.ToInt16(szSplitIP[3])));

                            writer.Write(iPort);
                            writer.Write(packetType);
                        }
                    }
                    svrConnect.SendTo(stream.ToArray(), serverEndPoint);
                }
                catch (Exception e)
                {
                    Logger.Write($"[SAMPServerQueryService] Failed to receive packet: {e}");
                }
            }
            catch (Exception e)
            {
                Logger.Write($"[SAMPServerQueryService] Failed to connect to IP: {e}");
            }
        }

        public Dictionary<string, string> read()
        {
            Dictionary<string, string> dData = new Dictionary<string, string>();

            try
            {
                serverIp = new IPAddress(IPAddress.Parse(szIP).GetAddressBytes());
                serverEndPoint = new IPEndPoint(serverIp, iPort);

                EndPoint rawPoint = (EndPoint)serverEndPoint;

                byte[] szReceive = new byte[2048];
                svrConnect.ReceiveFrom(szReceive, ref rawPoint);

                svrConnect.Close();

                MemoryStream stream = new MemoryStream(szReceive);
                BinaryReader read = new BinaryReader(stream);

                using (stream)
                {
                    using (read)
                    {
                        read.ReadBytes(10);

                        switch (read.ReadChar())
                        {
                            case 'i':
                                dData.Add("password", Convert.ToString(read.ReadByte()));
                                dData.Add("players", Convert.ToString(read.ReadInt16()));
                                dData.Add("maxplayers", Convert.ToString(read.ReadInt16()));
                                dData.Add("hostname", new string(read.ReadChars(read.ReadInt32())));
                                dData.Add("gamemode", new string(read.ReadChars(read.ReadInt32())));
                                dData.Add("mapname", new string(read.ReadChars(read.ReadInt32())));
                                break;

                            case 'r':
                                for (int i = 0, iRules = read.ReadInt16(); i < iRules; i++)
                                    dData.Add(new string(read.ReadChars(read.ReadByte())), new string(read.ReadChars(read.ReadByte())));
                                break;

                            case 'c':
                                for (int i = 0, iPlayers = read.ReadInt16(); i < iPlayers; i++)
                                    dData.Add(new string(read.ReadChars(read.ReadByte())), Convert.ToString(read.ReadInt32()));
                                break;

                            case 'd':
                                for (int i = 0, iTotalPlayers = read.ReadInt16(); i < iTotalPlayers; i++)
                                {
                                    string id = Convert.ToString(read.ReadByte());
                                    dData.Add(id + ".name", new string(read.ReadChars(read.ReadByte())));
                                    dData.Add(id + ".score", Convert.ToString(read.ReadInt32()));
                                    dData.Add(id + ".ping", Convert.ToString(read.ReadInt32()));
                                }
                                break;
                        }
                    }
                }

                foreach (var d in dData)
                    Logger.Write($"[SAMPServerQueryService] {d.Key} {d.Value}");
            }
            catch (Exception e)
            {
                Logger.Write($"[SAMPServerQueryService] There's been a problem reading the data {e}");
            }

            return dData;
        }
    }
}
