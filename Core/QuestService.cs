using Terramon.Core.Systems;
using Terramon.ID;
using Terraria.DataStructures;
using Terraria.Localization;

namespace Terramon.Core;

public class QuestProgress
{
    public string Uid;
    public ushort Progress;
    public bool completed;

    public void Complete()
    {
        if (completed) return;
        
        completed = true;
        Main.NewText("You have completed the quest " + Language.GetText("Mods.Terramon.Quests." + Uid), Color.Yellow);
    }
}

public class QuestService()
{
    private readonly List<string> _completedQuestIDs = new();
    public readonly List<QuestProgress> ActiveQuests = new();

    public bool ClaimRewardsForQuest(int index)
    {
        if (!ActiveQuests[index].completed) return false;
        var quest = QuestSystem.Quests.Find(q => q.Uid == ActiveQuests[index].Uid);
        Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_GiftOrReward(), 
            quest.Reward.ItemId, quest.Reward.ItemCount);
        _completedQuestIDs.Add(quest.Uid);
        ActiveQuests.RemoveAt(index);
        CheckForNewQuests();
        return true;
    }

    public void CheckForNewQuests()
    {
        bool questsEmpty = ActiveQuests.Count == 0;
        int count = 0;
        foreach (var quest in QuestSystem.Quests)
        {
            if (_completedQuestIDs.Contains(quest.Uid)) continue;
            if (ActiveQuests.FindIndex(q => q.Uid == quest.Uid) != -1) continue;
            if (quest.Dependencies != null && quest.Dependencies.Any(x => !_completedQuestIDs.Contains(x))) continue;

            count++;
            ActiveQuests.Add(new QuestProgress {Uid = quest.Uid, Progress = 0});
        }
        
        if (count > 0 && (!questsEmpty || _completedQuestIDs.Count > 0))
            Main.NewText($"{count} new quest{(count > 1 ? "s are" : " is")} available!", Color.Yellow); //TODO: move into localisation
    }

    public void TriggerPokemonCaught(ushort pokemonID, List<PokemonType> pokemonType, BallID pokeballID)
    {
        Main.NewText("a");
        foreach (var active in ActiveQuests)
        {
            if (active.completed) continue;
            var quest = QuestSystem.Quests.Find(q => q.Uid == active.Uid);
            
            if (quest.Trigger != QuestTrigger.PokemonCaught) continue;
            if (quest.Predicate.PokemonID.HasValue && quest.Predicate.PokemonID != pokemonID) continue;
            if (quest.Predicate.PokemonType.HasValue && quest.Predicate.PokemonType != pokemonType[0] && quest.Predicate.PokemonType != pokemonType[1]) continue;
            if (quest.Predicate.PokeballID.HasValue && quest.Predicate.PokeballID != pokeballID) continue;

            active.Progress++;
            if (active.Progress >= quest.Count)
                active.Complete();
        }
    }
    
    public void TriggerInventoryUpdate(Player player, Item item)
    {
        foreach (var active in ActiveQuests)
        {
            var quest = QuestSystem.Quests.Find(q => q.Uid == active.Uid);
            if (quest.Trigger != QuestTrigger.InventoryUpdated) continue;
            if (item != null && quest.Predicate.ItemId != item.type) continue; //allow null items if every slot is to be checked

            var amount = item?.stack ?? 0;
            foreach (var slot in player.inventory)
            {
                if (slot.type != quest.Predicate.ItemId) continue;
                amount += slot.stack;
            }
            active.Progress = (ushort)amount;
            
            if (amount >= quest.Count)
                active.Complete();
            else
                active.completed = false;
        }
    }
    
    public void TriggerItemUse(Item item, ushort pokemonID, List<PokemonType> pokemonType)
    {
        foreach (var active in ActiveQuests)
        {
            if (active.completed) continue;
            var quest = QuestSystem.Quests.Find(q => q.Uid == active.Uid);
            
            if (quest.Trigger != QuestTrigger.ItemUsed) continue;
            if (quest.Predicate.ItemId != item.type) continue;
            if (quest.Predicate.PokemonID.HasValue && quest.Predicate.PokemonID != pokemonID) continue;
            if (quest.Predicate.PokemonType.HasValue && (pokemonType == null || (quest.Predicate.PokemonType != pokemonType[0] && quest.Predicate.PokemonType != pokemonType[1]))) continue;

            active.Progress++;
            if (active.Progress >= quest.Count)
                active.Complete();
        }
    }
}