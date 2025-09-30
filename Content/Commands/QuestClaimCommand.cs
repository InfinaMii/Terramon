using Terramon.Helpers;
using Terraria.Localization;

namespace Terramon.Content.Commands;

public class QuestClaimCommand : DebugCommand
{
    public override CommandType Type => CommandType.Chat;

    public override string Command => "questclaim";

    public override string Description => Language.GetTextValue("Mods.Terramon.Commands.QuestClaim.Description");

    public override string Usage => Language.GetTextValue("Mods.Terramon.Commands.QuestClaim.Usage");
    
    protected override int MinimumArgumentCount => 1;
    
    public override void Action(CommandCaller caller, string input, string[] args)
    {
        base.Action(caller, input, args);
        if (!Allowed) return;

        int.TryParse(args[0], out int questId);
        if (questId <= -1 || TerramonPlayer.LocalPlayer.GetQuests().ActiveQuests.Count <= questId)
        {
            Main.NewText("Index out of range!", Color.Red);
            return;
        }

        if (!TerramonPlayer.LocalPlayer.GetQuests().ClaimRewardsForQuest(questId))
            Main.NewText("Quest hasn't been completed!", Color.Red);
    }
}