using Terramon.Core.Systems;
using Terramon.ID;
using Terraria.Localization;

namespace Terramon.Core;

public class QuestProgress
{
    public string Uid;
    public ushort Progress;
    public bool completed;

    public void Complete()
    {
        if (completed) return; //TODO: rewards
        
        completed = true;
        Main.NewText("You have completed the quest " + Language.GetText("Mods.Terramon.Quests." + Uid));
    }
}

public class QuestService()
{
    
    private List<string> _completedQuestIDs = new();
    private List<QuestProgress> _activeQuests = new();

    private List<QuestProgress> ActiveQuests
    {
        get => _activeQuests;
        set => _activeQuests = value;
    }
    
    public void GetDefaultValues() //TODO: replace with availability check
    {
        foreach (var quest in QuestSystem.Quests)
            _activeQuests.Add(new QuestProgress {Uid = quest.Uid, Progress = 0});
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