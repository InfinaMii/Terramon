using System;
using Terraria.Localization;

namespace Terramon.Content.Commands;

public class DexEditCommand : DebugCommand
{
    public override CommandType Type
        => CommandType.Chat;

    public override string Command
        => "dexedit";

    public override string Usage
        => "/dexedit <id> <status>";

    public override string Description
        => Language.GetTextValue("Mods.Terramon.Commands.DexEdit.Description");

    protected override int MinimumArgumentCount => 2;

    public override void Action(CommandCaller caller, string input, string[] args)
    {
        base.Action(caller, input, args);
        if (!Allowed) return;

        var hasValidId = int.TryParse(args[0], out var id);
        if (!hasValidId)
        {
            caller.Reply("Failed to parse ID argument as integer", Color.Red);
            return;
        }

        var hasValidStatus = int.TryParse(args[1], out var status);
        if (!hasValidStatus)
        {
            caller.Reply("Failed to parse status argument as integer", Color.Red);
            return;
        }

        var statusName = Enum.GetName((PokedexEntryStatus)status);
        if (statusName == null)
        {
            caller.Reply("Status argument is out of range", Color.Red);
            return;
        }

        var player = caller.Player.GetModPlayer<TerramonPlayer>();
        var success = player.UpdatePokedex((ushort)id, (PokedexEntryStatus)status, true);
        if (success)
            caller.Reply($"Successfully set Pokédex entry {id} to status {statusName}", new Color(255, 240, 20));
        else
            caller.Reply($"Pokédex entry {id} is out of range", Color.Red);
    }
}