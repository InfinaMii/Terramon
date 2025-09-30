using Terramon.Helpers;
using Terraria.Localization;

namespace Terramon.Content.Commands;

public class QuestStatusCommand : DebugCommand
{
    public override CommandType Type => CommandType.Chat;

    public override string Command => "queststatus";

    public override string Description => Language.GetTextValue("Mods.Terramon.Commands.QuestStatus.Description");

    public override string Usage => Language.GetTextValue("Mods.Terramon.Commands.QuestStatus.Usage");

    public override void Action(CommandCaller caller, string input, string[] args)
    {
        base.Action(caller, input, args);
        if (!Allowed) return;

        var quests = TerramonPlayer.LocalPlayer.GetQuests().ActiveQuests;
        for (int i = 0; i < quests.Count; i++)
            Main.NewText($"{i}: {Language.GetText("Mods.Terramon.Quests." + quests[i].Uid)}", quests[i].completed ? Color.Yellow : Color.White);
    }
}