using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using Discord;

namespace Terramon.Core.Systems;

public class DiscordRichPresence : ModSystem
{
    private Discord.Discord _discord;
    
    private bool _discordOpen;
    private long _startTimestamp;
    
    public override void OnModLoad()
    {
        _discordOpen = LoadDiscord();
        if (!_discordOpen) return;
        
        _startTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        UpdateActivity("Idle", "In the menu");
    }

    public override void PostUpdatePlayers()
    {
        
    }

    private bool LoadDiscord()
    {
        try
        {
            _discord = new Discord.Discord(1326726202391789649, (ulong)CreateFlags.NoRequireDiscord);
        }
        catch
        {
            Terramon.Instance.Logger.Debug("Couldn't connect to Discord - client not running?");
            return false;
        }

        return true;
    }

    private void UpdateActivity(string state, string description)
    {
        _discord.GetActivityManager().UpdateActivity(new Discord.Activity
        {
            State = state,
            Details = description,
            Timestamps = { Start = _startTimestamp },
            Instance = true
        }, result =>
        { Terramon.Instance.Logger.Debug(result == Result.Ok
                ? "Successfully updated Discord activity status"
                : "Failed to update Discord activity status");
        });
    }
}