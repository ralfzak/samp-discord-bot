﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace app.Services
{
    public enum VERIFICATION_STATES
    {
        NONE = 0,
        WAITING_CONFIRM = 1
    }

    public class VerificationService
    {
        private readonly DiscordSocketClient _discord;

        public VerificationService(IServiceProvider services)
        {
            _discord = services.GetRequiredService<DiscordSocketClient>();
            
            _discord.UserJoined += OnUserJoinServer;
            _discord.UserLeft += OnUserLeaveServer;

            LoggerService.Write("Binded the user (connect / disconnect) verification events!");
        }

        public async Task InitializeAsync()
        {
            await Task.CompletedTask;
        }

        public async Task OnUserJoinServer(SocketGuildUser user)
        {
            if (UserService.IsUserVerified(user.Id))
            {
                var verifiedRole = _discord.Guilds.FirstOrDefault(g => g.Id == Program.GUILD_ID)
                    .Roles.FirstOrDefault(r => r.Id == Program.VERIFIED_ROLE_ID);
                
                LoggerService.Write($"> JOIN VERIFIED: {user.Id} - ROLE SET");
                await user.AddRoleAsync(verifiedRole);
            }

            CacheService.ClearCache(user.Id);
            await Task.CompletedTask;
        }

        public async Task OnUserLeaveServer(SocketGuildUser user)
        {
            CacheService.ClearCache(user.Id);
            await Task.CompletedTask;
        }
        
        public static async Task<string> GetForumProfileContentAsync(int profileID)
        {
            string url = $"{Program.FORUM_PROFILE_URL}{profileID}";
            string result = string.Empty;

            using (HttpClient client = new HttpClient()) {
                using (HttpResponseMessage response = client.GetAsync(url).Result) {
                    using (HttpContent content = response.Content) {
                        result = await content.ReadAsStringAsync();
                    }
                }
            }

            return result;
        }

        public static async Task<string> GetForumProfileIfContainsCodeAsync(int profile_id, string token)
        {
            string profile_page = await GetForumProfileContentAsync(profile_id);
            Match match = Regex.Match(profile_page, @"<title>SA-MP Forums - View Profile: (.*)</title>");

            if (match.Success && profile_page.Contains(token))
            {
                var fname = match.Groups[0].Value.Remove(0, 36).Replace("</title>", "");
                LoggerService.Write($"[GetForumProfileIfContainsCodeAsync] fetching forumid {profile_id}: " +
                                    $"got forumName as {fname}");
                return fname;
            }

            return string.Empty;
        }

        public static async Task<string> GetForumProfileNameAsync(int profile_id)
        {
            string profile_page = await GetForumProfileContentAsync(profile_id);
            Match match = Regex.Match(profile_page, @"<title>SA-MP Forums - View Profile: (.*)</title>");

            if (match.Success)
            {
                var fname = match.Groups[0].Value.Remove(0, 36).Replace("</title>", "");
                LoggerService.Write($"[GetForumProfileNameAsync] fetching forumid {profile_id}: " +
                                    $"got forumName as {fname}");
                return fname;
            }

            return string.Empty;
        }
    }
}
