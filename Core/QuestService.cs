using Terramon.Core.Systems;
using Terramon.ID;
using Terraria.DataStructures;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace Terramon.Core;

public class QuestProgress
{
    public string Uid;
    public ushort Progress;
    public bool Completed;

    public void Complete()
    {
        if (Completed) return;
        
        Completed = true;
        Main.NewText(Language.GetTextValue("Mods.Terramon.Misc.QuestCompleted", Language.GetText("Mods.Terramon.Quests." + Uid)), Color.Yellow);
    }
}

public class QuestService()
{
    private readonly List<string> _completedQuestIDs = [];
    public readonly List<QuestProgress> ActiveQuests = [];

    public bool ClaimRewardsForQuest(int index)
    {
        if (!ActiveQuests[index].Completed) return false;
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
            Main.NewText(Language.GetTextValue(count > 1 ? "Mods.Terramon.Misc.QuestAvailableMulti" : "Mods.Terramon.Misc.QuestAvailable", count), Color.Yellow);
    }

    public void TriggerPokemonCaught(ushort pokemonID, List<PokemonType> pokemonType, BallID pokeballID)
    {
        foreach (var active in ActiveQuests)
        {
            if (active.Completed) continue;
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
            var response = QuestSystem.Quests.Count + ", " + active.Uid;
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
                active.Completed = false;
        }
    }
    
    public void TriggerItemUse(Item item, ushort pokemonID, List<PokemonType> pokemonType)
    {
        foreach (var active in ActiveQuests)
        {
            if (active.Completed) continue;
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

    public void LoadData(TagCompound tag)
    {
        LoadActive(tag);
        LoadCompleted(tag);
    }

    private void LoadActive(TagCompound tag)
    {
        const string tagName = "questAct";
        if (!tag.ContainsKey(tagName)) return;

        ActiveQuests.Clear();
        
        var activeQuests = tag.GetList<TagCompound>(tagName);
        foreach (var quest in activeQuests)
        {
            var item = new QuestProgress();
            item.Uid = quest.GetString("uid");
            item.Progress = (ushort)quest.GetShort("prog");
            item.Completed = quest.GetBool("completed");
            ActiveQuests.Add(item);
        }
    }
    
    private void LoadCompleted(TagCompound tag)
    {
        const string tagName = "questComp";
        if (!tag.ContainsKey(tagName)) return;

        _completedQuestIDs.Clear();
        _completedQuestIDs.AddRange(tag.GetList<string>(tagName));
    }
    
    public void SaveData(TagCompound tag)
    {
        SaveActive(tag);
        SaveCompleted(tag);
    }

    private void SaveActive(TagCompound tag)
    {
        var master = new List<TagCompound>();
        foreach (var quest in ActiveQuests)
        {
            var item = new TagCompound();
            item["uid"] = quest.Uid;
            item["prog"] = (short)quest.Progress;
            item["completed"] = quest.Completed;
            master.Add(item);
        }
        tag["questAct"] = master;
    }

    private void SaveCompleted(TagCompound tag)
    {
       tag["questComp"] = _completedQuestIDs;
    }
}