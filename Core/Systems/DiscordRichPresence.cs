using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using DiscordRPC;
using DiscordRPC.Logging;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Xna.Framework.Input;

namespace Terramon.Core.Systems;

public class DiscordRichPresence : ModSystem
{
    public static DiscordRichPresence Instance => ModContent.GetInstance<DiscordRichPresence>();
    
    private DiscordRpcClient _client;
    private bool _discordOpen;
    private DateTime _startTime;

    private string _details;
    private string _state;
    private string _smallImage;

    private DetailID _detailId;
    private StateID _stateId;
    private LargeImageID _largeImageId;

    private int lastActivePokemonId = -1;

    private enum DetailID
    {
        InMenu,
        InWorld
    }
    private enum StateID
    {
        None,
        ExploringWith,
        FightingWith
    }

    private enum LargeImageID
    {
        IconForest,
        IconCorruption,
        IconCrimson,
        IconHallow,
        IconForestNight
    }
    

    public void EnterWorld()
    {
        _detailId = DetailID.InWorld;
        _stateId = StateID.ExploringWith;
        UpdateActivity();
    }
    
    public override void OnWorldUnload()
    { 
        _detailId = DetailID.InMenu;
        _stateId = StateID.None;
        _largeImageId = LargeImageID.IconForest;
        UpdateActivity();
    }

    
    public override void UpdateUI(GameTime gameTime)
    {
        _client.Invoke();

        bool triggerResend = false;

        if (_detailId == DetailID.InWorld)
        {
            var activePokemon = TerramonPlayer.LocalPlayer.GetActivePokemon();
            if (activePokemon == null && lastActivePokemonId != -1)
            {
                triggerResend = true;
                lastActivePokemonId = -1;
            }
            else if (activePokemon != null && activePokemon.ID != lastActivePokemonId)
            {
                triggerResend = true;
                lastActivePokemonId = activePokemon.ID;
            }

            if (Main.LocalPlayer.ZoneHallow)
            {
                if (_largeImageId != LargeImageID.IconHallow)
                {
                    triggerResend = true;
                    _largeImageId = LargeImageID.IconHallow;
                }
            }
            else if (Main.LocalPlayer.ZoneCrimson)
            {
                if (_largeImageId != LargeImageID.IconCrimson)
                {
                    triggerResend = true;
                    _largeImageId = LargeImageID.IconCrimson;
                }
            }
            else if (Main.LocalPlayer.ZoneCorrupt)
            {
                if (_largeImageId != LargeImageID.IconCorruption)
                {
                    triggerResend = true;
                    _largeImageId = LargeImageID.IconCorruption;
                }
            }
            else if (!Main.dayTime)
            {
                if (_largeImageId != LargeImageID.IconForestNight)
                {
                    triggerResend = true;
                    _largeImageId = LargeImageID.IconForestNight;
                }
            }
            else if (_largeImageId != LargeImageID.IconForest)
            {
                triggerResend = true;
                _largeImageId = LargeImageID.IconForest;
            }
        }

        if (triggerResend)
            UpdateActivity();
    }

    
    public void UpdateActivity()
    {
        switch (_detailId)
        {
            case DetailID.InMenu:
                _details = "In the menu";
                break;
            case DetailID.InWorld:
                _details = "Playing in " + Main.worldName;
                break;
        }
            
        if (lastActivePokemonId != -1)
        {
            var pokemonName = "";

            if (_detailId == DetailID.InWorld)
            {
                var pokemon = TerramonPlayer.LocalPlayer.GetActivePokemon();
                if (pokemon != null)
                {
                    pokemonName = pokemon.DisplayName;
                    if (pokemon.IsShiny)
                        pokemonName += "\u2605"; //unicode star
                    _smallImage = pokemon.Ball.ToString().ToLower();
                }
                else
                    _smallImage = null;
            }

            switch (_stateId)
            {
                case StateID.None:
                    _state = null;
                    break;
                case StateID.ExploringWith:
                    _state = "Exploring with " + pokemonName;
                    break;
                case StateID.FightingWith:
                    _state = "Fighting a boss with " + pokemonName;
                    break;
            }
        }
        else
            _state = null;

        SetPresence();
    }
    
    
    public override void OnModLoad()
    {
        _discordOpen = LoadDiscord();
        if (!_discordOpen) return;

        _startTime = DateTime.UtcNow;
        _detailId = DetailID.InMenu;
        _stateId = StateID.None;
        _largeImageId = LargeImageID.IconForest;
        UpdateActivity();
        _client.UpdateStartTime();
    }
    
    
    private bool LoadDiscord()
    {
        _client = new DiscordRpcClient("1326726202391789649");
        var success = _client.Initialize();
        
        if (!success)
            Terramon.Instance.Logger.Debug("Couldn't connect to Discord - client not running?");

        return success;
    }
    

    private void SetPresence()
    {
        Timestamps timestamps = null;
        if (_client.CurrentPresence != null)
            timestamps = _client.CurrentPresence.Timestamps;
        _client.SetPresence(new RichPresence
        {
            State = _state,
            Details = _details,
            Assets = new Assets
            {
                LargeImageKey = _largeImageId.ToString().ToLower(),
                SmallImageKey = _smallImage
            },
            Timestamps = timestamps
        });
    }
    
    
    public override void Unload()
    {
        _client.Dispose();
    }
}