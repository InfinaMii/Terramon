using Terramon.Content.Items.PokeBalls;

namespace Terramon.Content.Tiles.MusicBoxes;

public class MusicBoxCenter : MusicTile
{
    public override void MouseOver(int i, int j)
    {
        var player = Main.LocalPlayer;
        player.noThrow = 2;
        player.cursorItemIconEnabled = true;
        player.cursorItemIconID = ModContent.ItemType<MusicItemCenter>();
    }
}

public class MusicItemCenter : MusicItem
{
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        MusicLoader.AddMusicBox(Mod, MusicLoader.GetMusicSlot(Mod, "Sounds/Music/PokeCenter"),
            ModContent.ItemType<MusicItemCenter>(), ModContent.TileType<MusicBoxCenter>());
    }

    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.createTile = ModContent.TileType<MusicBoxCenter>();
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.MusicBox)
            .AddIngredient(ModContent.ItemType<PokeBallItem>())
            .AddTile(TileID.TinkerersWorkbench)
            .Register();
    }
}