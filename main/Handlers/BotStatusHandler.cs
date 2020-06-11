using System;
using System.Threading.Tasks;
using main.Core;
using Discord.WebSocket;
using System.Threading;
using main.Services;

namespace main.Handlers
{
    #pragma warning disable 4014,1998
    public class BotStatusHandler
    {
        private readonly DiscordSocketClient _discord;
        private readonly StatsService _statsService;
        private Timer _updateTimer;

        public BotStatusHandler(DiscordSocketClient discord, StatsService statsService)
        {
            _discord = discord;
            _statsService = statsService;
            _updateTimer = new Timer(OnPlayerCountUpdateCheck, null, 10000, 603000);
        }
        
        public async Task InitializeAsync()
        {
            await Task.CompletedTask;
        }
        
        private async void OnPlayerCountUpdateCheck(object state)
        {
            try
            {
                var (playersCount, serversCount) = _statsService.GetSampPlayerServerCount();
                _discord.SetGameAsync($"Grand Theft Auto San Andreas - Players Online: {playersCount} - Servers Online: {serversCount}");
            }
            catch (Exception e)
            {
                Logger.Write($"[OnPlayerCountUpdateCheck] Failed: {e.Message}");
            }
        }
    }
}
