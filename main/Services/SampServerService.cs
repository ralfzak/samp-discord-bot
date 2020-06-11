using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using domain.Models;
using main.Core;
using main.Exceptions;

namespace main.Services
{
    public class SampServerService
    {
        private readonly IHttpClient _httpClient;
        private readonly string _hostedTabListUrl;
        
        public SampServerService(IHttpClient httpClient)
        {
            _httpClient = httpClient;
            _hostedTabListUrl = Configuration.GetVariable("Urls.Samp.HostedTabProvider");
        }
        
        public (string ip, ushort port) ParseIpPort(string ipPort)
        {
            string ip = ipPort;
            string _port = "7777";
            if (ipPort.Contains(':'))
            {
                var temp = ipPort.Split(':');
                if (temp.Length != 2)
                {
                    throw new InvalidIpParseException("Wrong format");
                }
                ip = temp[0];
                _port = temp[1];
            }

            if (!UInt16.TryParse(_port, out ushort port))
            {
                throw new InvalidIpParseException("Invalid port");
            }

            if (!ValidateIPv4(ip) && !ValidateHostname(ip))
            {
                throw new InvalidIpParseException("Invalid IP");
            }

            if (!ValidateIPv4(ip) && ValidateHostname(ip))
            {
                ip = GetDnsEntry(ip);
                if (ip == string.Empty)
                {
                    throw new InvalidIpParseException("Failed to find DNS entry");
                }
            }
            
            return (ip, port);
        }

        private string GetDnsEntry(string ip)
        {
            try
            {
                return Dns.GetHostAddressesAsync(ip).Result[0].ToString();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public SampServerData GetServerData(string ip, ushort port)
        {
            var generalInfo = SendAndReadPacketData(ip, port, (char)SampPackets.GeneralInfo, 2500);
            var rulesInfo = SendAndReadPacketData(ip, port, (char)SampPackets.RulesInfo, 3000);

            try
            {
                return new SampServerData
                {
                    Ip = ip,
                    Port = port,
                    Hostname = generalInfo["hostname"],
                    Gamemode = generalInfo["gamemode"],
                    MaxPlayers = Int32.Parse(generalInfo["maxplayers"]),
                    CurrentPlayers = Int32.Parse(generalInfo["players"]),
                    Version = rulesInfo["version"],
                    Url = rulesInfo["weburl"],
                    IsHostedTab = IsOnHostedTab(ip, port)
                };
            }
            catch (Exception)
            {
                throw new UnableToConnectToServerException($"{ip}:{port}");
            }
        }

        private bool ValidateIPv4(string ip) =>
            Uri.CheckHostName(ip) == UriHostNameType.IPv4 &&
            IPAddress.Parse(ip).AddressFamily == AddressFamily.InterNetwork &&
            ip != string.Empty &&
            ip.Count(i => i == '.') == 3;

        private bool ValidateHostname(string hostname) => 
            Uri.CheckHostName(hostname) == UriHostNameType.Dns;
        
        private bool IsOnHostedTab(string ip, ushort port) =>
            _httpClient.GetContent(_hostedTabListUrl).Contains($"{ip}:{port}");
        
        private Dictionary<string, string> SendAndReadPacketData(
            string ip,
            ushort port,
            char packet,
            int timeout
        )
        {
            try
            {
                var sentPacket = SendPacket(ip, port, packet, timeout);
                return ReadResponse(sentPacket);
            }
            catch (InvalidSocketDataException)
            {
                throw new UnableToConnectToServerException($"{ip}:{port}");
            }
        }

        private Dictionary<string, string> ReadResponse(SocketStructure socket)
        {
            var rawData = new byte[2048];
            EndPoint rawPoint = socket.IpEndPoint as EndPoint;
            try
            {
                socket.Socket.ReceiveFrom(rawData, ref rawPoint);
                socket.Socket.Close();
                return ParseResponse(rawData);
            }
            catch (Exception e)
            {
                throw new InvalidSocketDataException(e.Message);
            }
        }

        private SocketStructure SendPacket(string ip, ushort port, char packet, int timeout)
        {
            var ipAddress = new IPAddress(IPAddress.Parse(ip).GetAddressBytes());
            var ipEndPoint = new IPEndPoint(ipAddress, port);
            var socket = new Socket(ipEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            socket.SendTimeout = timeout;
            socket.ReceiveTimeout = timeout;
            
            try
            {
                socket.SendTo(BuildPacketBytes(ip, port, packet), ipEndPoint);
            }
            catch (Exception e)
            {
                throw new InvalidSocketDataException(e.Message);
            }
            
            return new SocketStructure
            {
                IpAddress = ipAddress,
                IpEndPoint = ipEndPoint,
                Socket = socket
            };
        }

        private byte[] BuildPacketBytes(string ip, ushort port, char packetType)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write("SAMP".ToCharArray());
                    foreach (var @decimal in ip.Split('.'))
                    {
                        writer.Write(Convert.ToByte(Convert.ToInt16(@decimal)));
                    }
                    writer.Write(port);
                    writer.Write(packetType);
                }

                return stream.ToArray();
            }
        }

        private Dictionary<string, string> ParseResponse(byte[] rawData)
        {
            var responseData = new Dictionary<string, string>();
            using (var stream = new MemoryStream(rawData))
            {
                using (var reader = new BinaryReader(stream))
                {
                    reader.ReadBytes(10);
                    switch (reader.ReadChar())
                    {
                        case (char)SampPackets.GeneralInfo:
                            responseData.Add("password", Convert.ToString(reader.ReadByte()));
                            responseData.Add("players", Convert.ToString(reader.ReadInt16()));
                            responseData.Add("maxplayers", Convert.ToString(reader.ReadInt16()));
                            responseData.Add("hostname", new string(reader.ReadChars(reader.ReadInt32())));
                            responseData.Add("gamemode", new string(reader.ReadChars(reader.ReadInt32())));
                            responseData.Add("mapname", new string(reader.ReadChars(reader.ReadInt32()))); 
                            break;

                        case (char)SampPackets.RulesInfo:
                            for (int i = 0, j = reader.ReadInt16(); i < j; i++)
                                responseData.Add(
                                    new string(reader.ReadChars(reader.ReadByte())),
                                    new string(reader.ReadChars(reader.ReadByte()))
                                );
                            break;
                    }
                }
            }
            return responseData;
        }
        
        private class SocketStructure
        {
            public IPAddress IpAddress { get; set; }
            public IPEndPoint IpEndPoint { get; set; }
            public Socket Socket { get; set; }
        }
        
        private enum SampPackets
        {
            GeneralInfo = 'i',
            RulesInfo = 'r'
        }
    }
}
