using Terramon.Content.Items;
using Terramon.Core.Loaders;
using Terraria.Utilities;

namespace Terramon.Content.Tiles.MusicBoxes;

[LoadGroup("MusicBoxes")]
public abstract class MusicItem : TerramonItem
{
    public override string Texture => "Terramon/Assets/Tiles/MusicBoxes/" + GetType().Name;
    
    protected override int UseRarity => ItemRarityID.LightRed;

    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.maxStack = 1;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTurn = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.autoReuse = true;
        Item.consumable = true;
        Item.width = 30;
        Item.height = 26;
        Item.value = 100000;
        Item.accessory = true;
    }
    
    public override bool? PrefixChance(int pre, UnifiedRandom rand)
    {
        return false;
    }
}