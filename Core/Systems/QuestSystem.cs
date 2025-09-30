using System.Text.RegularExpressions;
using Terramon.Content.Items;
using Terramon.Content.Items.PokeBalls;
using Terramon.ID;
using Terraria.Localization;

namespace Terramon.Core.Systems;

public class TerramonQuest(string uid, ushort count, QuestTrigger trigger, QuestPredicate predicate, QuestReward reward, params string[] dependencies)
{
    public string Uid = uid;
    public ushort Count = count;
    public QuestTrigger Trigger = trigger;
    public QuestPredicate Predicate = predicate;
    public QuestReward Reward = reward; //TODO: consume/don't consume items
    public string[] Dependencies = dependencies;
}

public enum QuestTrigger
{
    InventoryUpdated,
    ItemUsed,
    PokemonCaught,
    PokemonDefeated,
    PokemonEvolved
}

public struct QuestPredicate
{
    public int? ItemId;
    public BallID? PokeballID;
    public PokemonType? PokemonType;
    public ushort? PokemonID;
}

public struct QuestReward(int itemId, ushort itemCount = 1)
{
    public int ItemId = itemId;
    public ushort ItemCount = itemCount;
}

public class QuestSystem: ModSystem
{
    public static List<TerramonQuest> Quests;
        
    public override void PostSetupContent()
    {
        Quests = new();
        
        AddQuest(nameof(Terramon), new TerramonQuest("Test.Get5Dirt", 5, QuestTrigger.InventoryUpdated, 
            new(){ ItemId = ItemID.DirtBlock }, new(ModContent.ItemType<CherishBallItem>())));
        
        AddQuest(nameof(Terramon), new TerramonQuest("Test.CatchPikachu", 1, QuestTrigger.PokemonCaught, 
            new(){ PokemonID = 25 }, new(ModContent.ItemType<CherishBallItem>())));
        
        AddQuest(nameof(Terramon), new TerramonQuest("Test.Catch5WithGreatBall", 5, QuestTrigger.PokemonCaught, 
            new(){ PokeballID = BallID.GreatBall }, new(ModContent.ItemType<CherishBallItem>())));
        
        AddQuest(nameof(Terramon), new TerramonQuest("Test.Catch5WaterType", 5, QuestTrigger.PokemonCaught, 
            new(){ PokemonType = PokemonType.Water }, new(ModContent.ItemType<CherishBallItem>())));
        
        AddQuest(nameof(Terramon), new TerramonQuest("Test.CatchFireWithUltraBall", 1, QuestTrigger.PokemonCaught, 
            new(){ PokemonType = PokemonType.Fire, PokeballID = BallID.UltraBall }, new(ModContent.ItemType<CherishBallItem>())));
        
        AddQuest(nameof(Terramon), new TerramonQuest("Test.Use5RareCandies", 5, QuestTrigger.ItemUsed, 
            new(){ ItemId = ModContent.ItemType<RareCandy>()}, new(ModContent.ItemType<CherishBallItem>())));
        
        AddQuest(nameof(Terramon), new TerramonQuest("Test.ThrowPokeBall", 1, QuestTrigger.ItemUsed, 
            new(){ ItemId = ModContent.ItemType<PokeBallItem>()}, new(ModContent.ItemType<CherishBallItem>())));
        
        AddQuest(nameof(Terramon), new TerramonQuest("Test.UseCandyOnWeedle", 1, QuestTrigger.ItemUsed, 
            new(){ ItemId = ModContent.ItemType<RareCandy>(), PokemonID = 13 }, new(ModContent.ItemType<CherishBallItem>())));
        
        AddQuest(nameof(Terramon), new TerramonQuest("Test.UseCandyOnWaterPokemon", 1, QuestTrigger.ItemUsed, 
            new(){ ItemId = ModContent.ItemType<RareCandy>(), PokemonType = PokemonType.Water }, new(ModContent.ItemType<CherishBallItem>()), 
            "Test.Get5Dirt"));
    }

    public void AddQuest(string mod, TerramonQuest quest)
    {
        Quests.Add(quest); //TODO: use mod name
        var nameLoc = quest.Uid.Split('.')[^1];
        nameLoc = Regex.Replace(nameLoc, @"(?<!^)(?=[A-Z0-9])", " ");
        Language.GetOrRegister("Mods.Terramon.Quests." + quest.Uid, () => nameLoc);
    }

    public void GetAllVisibleQuests()
    {
        
    }
}